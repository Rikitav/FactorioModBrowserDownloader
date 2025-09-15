using FactorioNexus.ApplicationArchitecture.Serialization;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json.Serialization;

namespace FactorioNexus.Infrastructure.Models
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
        public CompatibilityTag Modifier { get; set; }
        public VersionComparer? Comparer { get; set; }
        public Version? Version { get; set; }

        public DependencyInfo()
        {
            ModId = null!;
            Modifier = CompatibilityTag.Required;
        }

        public DependencyInfo(string modId)
        {
            ModId = modId ?? throw new ArgumentNullException(nameof(modId));
            Modifier = CompatibilityTag.Required;
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

        public struct CompatibilityTag(DependencyModifierOperator modifier) : IEquatable<CompatibilityTag>
        {
            private static readonly Dictionary<string, DependencyModifierOperator> Modifiers = new Dictionary<string, DependencyModifierOperator>()
            {
                { "!", DependencyModifierOperator.Incompatible },
                { "?", DependencyModifierOperator.Optional },
                { "~", DependencyModifierOperator.DontAffectLoad },
                { "(?)", DependencyModifierOperator.Hidden }
            };

            public static CompatibilityTag Required => new CompatibilityTag(DependencyModifierOperator.Required);

            private readonly DependencyModifierOperator _modifier = modifier;

            public readonly bool IsRequired => _modifier == DependencyModifierOperator.Required;
            public readonly bool IsOptional => _modifier != DependencyModifierOperator.Optional;

            public static bool TryParse(string? modifier, [NotNullWhen(true)] out CompatibilityTag? dependencyModifier)
            {
                dependencyModifier = null;
                if (string.IsNullOrEmpty(modifier))
                {
                    dependencyModifier = new CompatibilityTag(DependencyModifierOperator.Required);
                    return true;
                }

                if (!Modifiers.TryGetValue(modifier, out DependencyModifierOperator modifierOperator))
                    return false;

                dependencyModifier = new CompatibilityTag(modifierOperator);
                return true;
            }


            public override readonly bool Equals(object? obj)
            {
                if (obj is not CompatibilityTag modifier)
                    return false;

                return Equals(modifier);
            }

            public readonly bool Equals(CompatibilityTag other) => _modifier == other._modifier;
            public override readonly int GetHashCode() => _modifier.GetHashCode();

            public override readonly string ToString() => _modifier switch
            {
                DependencyModifierOperator.Optional => "?",
                DependencyModifierOperator.Incompatible => "!",
                DependencyModifierOperator.Hidden => "(?)",
                DependencyModifierOperator.DontAffectLoad => "~",
                _ => throw new NotSupportedException(),
            };

            public static bool operator ==(CompatibilityTag left, CompatibilityTag right) => left.Equals(right);
            public static bool operator !=(CompatibilityTag left, CompatibilityTag right) => !left.Equals(right);
        }

        public struct VersionComparer(CompareOperators comparer, bool orEqual = false) : IEquatable<VersionComparer>, IEqualityComparer<Version>
        {
            private static readonly Dictionary<char, CompareOperators> Operators = new Dictionary<char, CompareOperators>()
            {
                { '<', CompareOperators.Less },
                { '>', CompareOperators.Greater },
                { '=', CompareOperators.Equal }
            };

            public static VersionComparer Equal => new VersionComparer(CompareOperators.Equal, true);

            public readonly CompareOperators Operator => comparer;
            public readonly bool IsStrong => !orEqual;

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
                if (!IsStrong && left.Equals(right))
                    return true;
#pragma warning restore CS8602

                return Operator switch
                {
                    CompareOperators.Less => left < right,
                    CompareOperators.Equal => left == right,
                    CompareOperators.Greater => left > right,
                    _ => false
                };
            }

            public override readonly bool Equals(object? obj)
            {
                if (obj is not VersionComparer comparer)
                    return false;

                return Equals(comparer);
            }

            public readonly bool Equals(VersionComparer other) => Operator == other.Operator && IsStrong == other.IsStrong;
            public readonly int GetHashCode([DisallowNull] Version obj) => obj.GetHashCode();
            public override readonly int GetHashCode() => HashCode.Combine(Operator.GetHashCode(), IsStrong.GetHashCode());

            public override readonly string ToString() => Operator switch
            {
                CompareOperators.Less => !IsStrong ? "<=" : "<",
                CompareOperators.Equal => "=",
                CompareOperators.Greater => !IsStrong ? ">=" : ">",
                _ => throw new NotSupportedException(),
            };

            public static bool operator ==(VersionComparer left, VersionComparer right) => left.Equals(right);
            public static bool operator !=(VersionComparer left, VersionComparer right) => !left.Equals(right);
        }
    }
}
