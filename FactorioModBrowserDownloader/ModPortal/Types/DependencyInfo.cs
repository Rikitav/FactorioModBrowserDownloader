using System.Text;

namespace FactorioNexus.ModPortal.Types
{
    public class DependencyInfo()
    {
        public required string ModId { get; set; }
        public DependencyModifier? Prefix { get; set; }
        public VersionOperator? Operator { get; set; }
        public Version? Version { get; set; }

        /*
        public bool ValidateRelease(ReleaseInfo release)
        {
            if (Version == null)
                return true; // Any version

            return Operator switch
            {
                VersionOperator.Less => release.Version < Version,
                VersionOperator.LessOrEqual => release.Version <= Version,
                VersionOperator.Equal => release.Version == Version,
                VersionOperator.MoreOrEqual => release.Version >= Version,
                VersionOperator.More => release.Version > Version,
                _ => false,
            };
        }
        */

        public override string ToString()
        {
            StringBuilder dependency = new StringBuilder();

            // If prefix is not null? adding its string representation
            if (Prefix != null && Prefix != DependencyModifier.Required)
                dependency.Append(PrefixToString(Prefix.Value)).Append(' ');

            // Adding mod name
            dependency.Append(ModId);

            // if operator and version is not null, adding
            if (Operator != null && Version != null)
                dependency.Append(' ').Append(OperatorToString(Operator.Value)).Append(' ').Append(Version.ToString());

            // Building result
            return dependency.ToString();
        }

        private static string PrefixToString(DependencyModifier prefix) => prefix switch
        {
            DependencyModifier.DontAffect => "~",
            DependencyModifier.Hidden => "(?)",
            DependencyModifier.Optional => "?",
            DependencyModifier.Incompatible => "!",
            DependencyModifier.Required => string.Empty,
            _ => throw new ArgumentOutOfRangeException(nameof(prefix))
        };

        private static string OperatorToString(VersionOperator versionOperator) => versionOperator switch
        {
            VersionOperator.Less => "<",
            VersionOperator.LessOrEqual => "<=",
            VersionOperator.Equal => "=",
            VersionOperator.MoreOrEqual => ">=",
            VersionOperator.More => ">",
            _ => throw new ArgumentOutOfRangeException(nameof(versionOperator))
        };
    }

    public enum VersionOperator
    {
        Less,
        LessOrEqual,
        Equal,
        MoreOrEqual,
        More
    }

    public enum DependencyModifier
    {
        Required,
        Incompatible,
        Optional,
        Hidden,
        DontAffect
    }
}
