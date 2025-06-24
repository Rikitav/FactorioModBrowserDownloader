using FactorioNexus.ModPortal.Types;
using System.Text;

namespace FactorioNexus.Services
{
    public class DependencyVersionRange
    {
        private ReleaseInfo? _latestMatchingReleaseInfo;
        private bool _isTopStrong = false;
        private bool _isBottomStrong = false;
        private bool _isEqual = false;

        public string ModId
        {
            get;
            private set;
        }

        public ReleaseInfo? LatestMatchingRelease
        {
            get => _latestMatchingReleaseInfo;
            private set => _latestMatchingReleaseInfo = value;
        }

        private Version? _top;
        private Version? _bottom;

        public DependencyVersionRange(DependencyInfo initDependency)
        {
            ModId = initDependency.ModId;
            Tweak(initDependency);
        }

        public async Task<bool> TryFindLatestMatchingRelease()
        {
            ModPageFullInfo dependencyModPage = await ModsBrowsingManager.FetchFullModInfo(ModId);
            return dependencyModPage.TryFindRelease(this, out _latestMatchingReleaseInfo);
        }

        public bool IsInside(ModStoreEntry store)
            => IsInside(store.Info.ModVersion ?? throw new ArgumentNullException(nameof(store.Info.ModVersion)));

        public bool IsInside(ReleaseInfo release)
            => IsInside(release.Version);

        public bool IsInside(Version version)
        {
            /*
            bool fitTopBound = _isTopStrong ? release.Version < _top : release.Version <= _top;
            bool fitBottomBound = _isBottomStrong ? release.Version > _bottom : release.Version >= _bottom;
            return fitTopBound && fitBottomBound;
            */

            if (_isEqual)
                return version.Equals(_top);

            bool fitTopBound = false;
            bool fitBottomBound = false;

            if (_top != null)
                fitTopBound = _isTopStrong ? version < _top : version <= _top;

            if (_bottom != null)
                fitBottomBound = _isBottomStrong ? version > _bottom : version >= _bottom;

            if (_top != null && _bottom != null)
                return fitTopBound && fitBottomBound;

            if (_top != null)
                return fitTopBound;

            if (_bottom != null)
                return fitBottomBound;

            return false;
        }

        public void Tweak(DependencyInfo dependency)
        {
            if (dependency.ModId != ModId)
                throw new ArgumentException("Invalid dependency modId");

            if (dependency.Version == null)
                return; // Any version

            if (_isEqual)
                return; // no need to tweak range anymore as exact needed verion already found

            switch (dependency.Operator)
            {
                case VersionOperator.Less:
                    {
                        if (dependency.Version < _top)
                            _top = dependency.Version;

                        _isTopStrong = true;
                        break;
                    }

                case VersionOperator.LessOrEqual:
                    {
                        if (dependency.Version < _top)
                        {
                            _top = dependency.Version;
                            _isTopStrong = false;
                        }

                        break;
                    }

                case VersionOperator.Equal:
                    {
                        _top = dependency.Version;
                        _bottom = dependency.Version;
                        _isEqual = true;
                        break;
                    }

                case VersionOperator.MoreOrEqual:
                    {
                        if (dependency.Version > _bottom)
                        {
                            _bottom = dependency.Version;
                            _isBottomStrong = false;
                        }

                        break;
                    }

                case VersionOperator.More:
                    {
                        if (dependency.Version > _bottom)
                            _bottom = dependency.Version;

                        _isBottomStrong = true;
                        break;
                    }
            }
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

                dependency.Append(")");
            }

            return dependency.ToString();
        }
    }
}
