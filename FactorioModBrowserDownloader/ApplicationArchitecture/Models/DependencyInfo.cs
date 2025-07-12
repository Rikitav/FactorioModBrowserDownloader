using FactorioNexus.ApplicationArchitecture.Models;
using FactorioNexus.ApplicationArchitecture.Serialization;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json.Serialization;

namespace FactorioNexus.ApplicationArchitecture.Models
{
    public enum CompareOperators
    {
        Less,
        Equal,
        Greater
    }

    public enum DependencyModifierOperator
    {
        Required,
        Incompatible,
        Optional,
        Hidden,
        DontAffectLoad
    }

    [JsonConverter(typeof(JsonDependencyInfoConverter))]
    public class DependencyInfo : IEquatable<DependencyInfo>
    {
        public string ModId { get; set; }
        public DependencyModifier Modifier { get; set; }
        public VersionComparer? Comparer { get; set; }
        public Version? Version { get; set; }

        public DependencyInfo()
        {
            ModId = null!;
            Modifier = DependencyModifier.Required;
        }

        public DependencyInfo(string modId)
        {
            ModId = modId ?? throw new ArgumentNullException(nameof(modId));
            Modifier = DependencyModifier.Required;
        }

        public override string ToString()
        {
            StringBuilder dependency = new StringBuilder();

            // If prefix is not null? adding its string representation
            if (!Modifier.IsRequired)
                dependency.Append(Modifier.ToString()).Append(' ');

            // Adding mod name
            dependency.Append(ModId);

            // if operator and version is not null, adding
            if (Comparer != null && Version != null)
                dependency.Append(' ').Append(Comparer.ToString()).Append(' ').Append(Version.ToString());

            // Building result
            return dependency.ToString();
        }

        public override bool Equals(object? obj)
        {
            if (obj is null)
                return false;

            if (obj is not DependencyInfo dependencyInfo)
                return false;

            return Equals(dependencyInfo);
        }

        public bool Equals(DependencyInfo? other)
        {
            if (other == null)
                return false;

            if (ModId != other.ModId)
                return false;

            if (Modifier != other.Modifier)
                return false;

            if (Comparer != other.Comparer)
                return false;

            if (Version == null && other.Version == null)
                return true;

            if (Version != null && other.Version != null)
                return Version == other.Version;

            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                Modifier.GetHashCode(),
                ModId.GetHashCode(),
                Comparer?.GetHashCode(),
                Version?.GetHashCode());
        }
    }

    public class DependencyModifier(DependencyModifierOperator modifier) : IEquatable<DependencyModifier>
    {
        private static readonly Dictionary<string, DependencyModifierOperator> Modifiers = new Dictionary<string, DependencyModifierOperator>()
        {
            { "!", DependencyModifierOperator.Incompatible },
            { "?", DependencyModifierOperator.Optional },
            { "~", DependencyModifierOperator.DontAffectLoad },
            { "(?)", DependencyModifierOperator.Hidden }
        };

        public static DependencyModifier Required => new DependencyModifier(DependencyModifierOperator.Required);

        private readonly DependencyModifierOperator _modifier = modifier;

        public bool IsRequired => _modifier == DependencyModifierOperator.Required;
        public bool IsOptional => _modifier != DependencyModifierOperator.Optional;

        public static bool TryParse(string? modifier, [NotNullWhen(true)] out DependencyModifier? dependencyModifier)
        {
            dependencyModifier = null;
            if (string.IsNullOrEmpty(modifier))
            {
                dependencyModifier = new DependencyModifier(DependencyModifierOperator.Required);
                return true;
            }

            if (!Modifiers.TryGetValue(modifier, out DependencyModifierOperator modifierOperator))
                return false;

            dependencyModifier = new DependencyModifier(modifierOperator);
            return true;
        }

        public bool Equals(DependencyModifier? other)
        {
            if (other is null)
                return false;

            return _modifier == other._modifier;
        }

        public override bool Equals(object? obj)
        {
            if (obj is not DependencyModifier modifier)
                return false;

            return Equals(modifier);
        }

        public override int GetHashCode()
        {
            return _modifier.GetHashCode();
        }

        public override string ToString() => _modifier switch
        {
            DependencyModifierOperator.Optional => "?",
            DependencyModifierOperator.Incompatible => "!",
            DependencyModifierOperator.Hidden => "(?)",
            DependencyModifierOperator.DontAffectLoad => "~",
            _ => throw new NotSupportedException(),
        };
    }

    public class VersionComparer(CompareOperators comparer, bool orEqual = false) : IEquatable<VersionComparer>, IEqualityComparer<Version>
    {
        private static readonly Dictionary<char, CompareOperators> Operators = new Dictionary<char, CompareOperators>()
        {
            { '<', CompareOperators.Less },
            { '>', CompareOperators.Greater },
            { '=', CompareOperators.Equal }
        };

        public static VersionComparer Equal => new VersionComparer(CompareOperators.Equal, true);

        private readonly bool _orEqual = orEqual;
        private readonly CompareOperators _operator = comparer;

        public CompareOperators Operator => _operator;
        public bool IsStrong => !_orEqual;

        public static bool TryParse(string comparerString, [NotNullWhen(true)] out VersionComparer? versionComparer)
        {
            versionComparer = null;
            if (comparerString is { Length: < 0 or > 2 })
                return false;

            if (!Operators.TryGetValue(comparerString[0], out CompareOperators comparer))
                return false;

            if (comparerString.Length == 2 && comparerString[1] != '=')
                return false;

            versionComparer = new VersionComparer(comparer, comparerString.Length == 2);
            return true;
        }

        public bool Equals(Version? left, Version? right)
        {
            if (left is null | right is null)
                return left is null && right is null;

#pragma warning disable CS8602
            if (_orEqual && left.Equals(right))
                return true;
#pragma warning restore CS8602

            return _operator switch
            {
                CompareOperators.Less => left < right,
                CompareOperators.Equal => left == right,
                CompareOperators.Greater => left > right,
                _ => false
            };
        }

        public override bool Equals(object? obj)
        {
            if (obj is not VersionComparer comparer)
                return false;

            return Equals(comparer);
        }

        public bool Equals(VersionComparer? other)
        {
            if (other is null)
                return false;

            return _operator == other._operator && _orEqual == other._orEqual;
        }

        public override int GetHashCode()
            => HashCode.Combine(_operator.GetHashCode(), _orEqual.GetHashCode());

        public int GetHashCode([DisallowNull] Version obj)
            => obj.GetHashCode();

        public override string ToString() => _operator switch
        {
            CompareOperators.Less => _orEqual ? "<=" : "<",
            CompareOperators.Equal => "=",
            CompareOperators.Greater => _orEqual ? ">=" : ">",
            _ => throw new NotSupportedException(),
        };
    }
}
