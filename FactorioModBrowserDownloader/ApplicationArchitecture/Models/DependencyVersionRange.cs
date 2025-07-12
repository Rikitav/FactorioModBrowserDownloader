using System.Text;

namespace FactorioNexus.ApplicationArchitecture.Models
{
    public class DependencyVersionRange
    {
        private readonly List<DependencyInfo> _tweakHistory;

        private ReleaseInfo? _latestMatchingReleaseInfo;
        private Version? _top;
        private Version? _bottom;

        private bool _isTopStrong = false;
        private bool _isBottomStrong = false;
        private bool _isEqual = false;

        public IEnumerable<DependencyInfo> TweakHistory
        {
            get => _tweakHistory;
        }

        public string ModId
        {
            get;
            private set;
        }

        public bool HasBottomBound
        {
            get => _bottom != null;
        }

        public bool HasTopBound
        {
            get => _top != null;
        }

        public ReleaseInfo? LatestMatchingRelease
        {
            get => _latestMatchingReleaseInfo;
            private set => _latestMatchingReleaseInfo = value;
        }

        public DependencyVersionRange(DependencyInfo initDependency)
        {
            _tweakHistory = [initDependency];
            ModId = initDependency.ModId;
            Tweak(initDependency);
        }

        public async Task<bool> TryFindLatestMatchingRelease()
        {
            try
            {
                ModEntryFull dependencyModPage = await App.Instance.Client.FetchFullModInfo(ModId);
                return dependencyModPage.TryFindRelease(this, out _latestMatchingReleaseInfo);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool IsInside(ModStoreEntry store)
            => IsInside(store.Info.ModVersion ?? throw new ArgumentNullException(nameof(store.Info.ModVersion)));

        public bool IsInside(ReleaseInfo release)
            => IsInside(release.Version);

        public bool IsInside(Version version)
            => FitBottomBound(version) & FitTopBound(version);

        private bool FitBottomBound(Version version)
            => !HasBottomBound || (_isBottomStrong ? version > _bottom : version >= _bottom);

        private bool FitTopBound(Version version)
            => !HasTopBound || (_isTopStrong ? version < _top : version <= _top);

        public void Tweak(DependencyInfo dependency)
        {
            if (dependency.ModId != ModId)
                throw new ArgumentException("Invalid dependency modId");

            if (dependency.Comparer == null)
                return; // Any version

            if (dependency.Version == null)
                return; // Any version

            if (_isEqual)
                return; // no need to tweak range anymore as exact needed version already found
            
            switch (dependency.Comparer.Operator)
            {
                case CompareOperators.Less:
                    {
                        if (dependency.Version < _top)
                            _top = dependency.Version;

                        _isTopStrong = dependency.Comparer.IsStrong;
                        break;
                    }

                case CompareOperators.Equal:
                    {
                        _top = dependency.Version;
                        _bottom = dependency.Version;
                        _isEqual = true;
                        break;
                    }

                case CompareOperators.Greater:
                    {
                        if (dependency.Version > _bottom)
                            _bottom = dependency.Version;

                        _isBottomStrong = dependency.Comparer.IsStrong;
                        break;
                    }
            }

            _tweakHistory.Add(dependency);
        }

        public override string ToString()
        {
            StringBuilder dependency = new StringBuilder();
            dependency.Append(ModId);

            if (_top != null || _bottom != null)
            {
                dependency.Append(" (");
                if (_isEqual)
                {
                    dependency.Append("= ").Append(_top ?? _bottom);
                }
                else
                {
                    if (_top != null)
                        dependency.Append(_top).Append(' ').Append(_isTopStrong ? "<" : "<=").Append(' ');

                    dependency.Append("value");
                    if (_bottom != null)
                        dependency.Append(' ').Append(_isBottomStrong ? ">" : ">=").Append(' ').Append(_bottom);
                }

                dependency.Append(')');
            }

            return dependency.ToString();
        }
    }
}
