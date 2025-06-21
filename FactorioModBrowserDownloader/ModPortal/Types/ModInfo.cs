using FactorioNexus.ModPortal.Converters;
using System.Text.Json.Serialization;

namespace FactorioNexus.ModPortal.Types
{
    public class ModInfo
    {
        /// <summary>
        /// Mandatory field.
        /// The internal name of mod.
        /// The game accepts anything as a mod name, however the mod portal restricts mod names to only consist of alphanumeric characters, dashes and underscores.
        /// Note that the mod folder or mod zip file name has to contain the mod name, where the restrictions of the file system apply.
        /// The game accepts mod names with a maximum length of 100 characters.
        /// The mod portal only accepts mods with names that are longer than 3 characters and shorter than 50 characters.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// Mandatory field.
        /// Defines the version of the mod in the format "number.number.number" for "Major.Middle.Minor",
        /// for example "0.6.4". Each number can range from 0 to 65535.
        /// </summary>
        [JsonPropertyName("version")]
        public Version? ModVersion { get; set; }

        /// <summary>
        /// Mandatory field.
        /// The display name of the mod, so it is not recommended to use someUgly_pRoGrAmMeR-name here.
        /// Can be overwritten with a locale entry in the mod-name category, using the internal mod name as the key.
        /// The game will reject a title field that is longer than 100 characters.However, this can be worked around by using the locale entry.The mod portal does not restrict mod title length. 
        /// </summary>
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        /// <summary>
        /// Mandatory field.
        /// The author of the mod. This field does not have restrictions, it can also be a list of authors etc.
        /// The mod portal ignores this field, it will simply display the uploader's name as the author.
        /// </summary>
        [JsonPropertyName("author")]
        public string? Author { get; set; }

        /// <summary>
        /// Optional field.
        /// How the mod author can be contacted, for example an email address.
        /// </summary>
        [JsonPropertyName("contact")]
        public string? Contact { get; set; }

        /// <summary>
        /// Optional field.
        /// A short description of what your mod does.
        /// This is all that people get to see in-game.
        /// Can be overwritten with a locale entry in the mod-description category, using the internal mod name as the key.
        /// </summary>
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        /// <summary>
        /// Optional field in the format "major.minor".
        /// The Factorio version that this mod supports. This can only be one Factorio version, not multiple. However, it includes all .sub versions. While the field is optional, usually mods are developed for versions higher than the default 0.12, so the field has to be added anyway.
        /// Adding a sub part, e.g. "0.18.27" will make the mod portal reject the mod and the game act weirdly. That means this shouldn't be done; use only the major and minor components "major.minor", for example "1.0".
        /// Mods with the factorio_version "0.18" can also be loaded in 1.0 and the mod portal will return them when queried for factorio_version 1.0 mods.
        /// </summary>
        [JsonPropertyName("factorio_version")]
        public Version? FactorioVersion { get; set; }

        /// <summary>
        /// Optional field.
        /// Where the mod can be found on the internet.
        /// Note that the in-game mod browser shows the mod portal link additionally to this field. Please don't put "None" here, it makes the field on the mod portal website look ugly.
        /// Just leave the field empty if the mod doesn't have a website/forum thread/discord.
        /// </summary>
        [JsonPropertyName("homepage")]
        public string? Homepage { get; set; }

        /// <summary>
        /// Optional field.
        /// Mods that this mod depends on or is incompatible with. If this mod depends on another, the other mod will load first, see Data lifecycle.
        /// An empty array allows to work around the default and have no dependencies at all.
        /// </summary>
        [JsonPropertyName("dependencies"), JsonConverter(typeof(JsonDependencyInfoConverter))]
        public DependencyInfo[]? Dependencies { get; set; }

        /// <summary>
        /// Optional field. Indicates that mod require 2.0 Space age DLC feature. 
        /// </summary>
        [JsonPropertyName("quality_required")]
        public bool? QualityRequired { get; set; }

        /// <summary>
        /// Optional field. Indicates that mod require 2.0 Space age DLC feature. 
        /// </summary>
        [JsonPropertyName("space_travel_required")]
        public bool? SpaceTravelRequired { get; set; }

        /// <summary>
        /// Optional field. Indicates that mod require 2.0 Space age DLC feature. 
        /// </summary>
        [JsonPropertyName("spoiling_required")]
        public bool? SpoilingRequired { get; set; }

        /// <summary>
        /// Optional field. Indicates that mod require 2.0 Space age DLC feature. 
        /// </summary>
        [JsonPropertyName("freezing_required")]
        public bool? FreezingReqired { get; set; }

        /// <summary>
        /// Optional field. Indicates that mod require 2.0 Space age DLC feature. 
        /// </summary>
        [JsonPropertyName("segmented_units_required")]
        public bool? SegmentedUnitsRequired { get; set; }

        /// <summary>
        /// Optional field. Indicates that mod require 2.0 Space age DLC feature. 
        /// </summary>
        [JsonPropertyName("expansion_shaders_required")]
        public bool? ExpansionShadersRequired { get; set; }
    }

    public class DependencyInfo()
    {
        public required string ModId { get; set; }
        public DependencyModifier? Prefix { get; set; }
        public VersionOperator? Operator { get; set; }
        public Version? Version { get; set; }

        public bool ValidateRelease(ReleaseInfo release)
        {
            if (Version == null)
                return true;

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
