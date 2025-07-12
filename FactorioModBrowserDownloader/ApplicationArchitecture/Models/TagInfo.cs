using FactorioNexus.ApplicationArchitecture.Serialization;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace FactorioNexus.ApplicationArchitecture.Models
{
    [JsonConverter(typeof(JsonTagInfoConverter))]
    public class TagInfo : IEquatable<TagInfo>
    {
        /// <summary>
        /// Id of tag
        /// </summary>
        [JsonPropertyName("id")]
        public int? Id { get; set; }

        /// <summary>
        /// Inner name of tag
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// Display name of tag
        /// </summary>
        [JsonPropertyName("title")]
        public string Title { get; set; }

        /// <summary>
        /// Description of tag
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; set; }

        public TagInfo()
        {
            Id = null;
            Name = string.Empty;
            Title = string.Empty;
            Description = string.Empty;
        }

        public TagInfo(string defaultName)
        {
            Id = null;
            Name = defaultName;
            Title = defaultName.FirstLetterToUpper();
            Description = defaultName;
        }

        public TagInfo(int? id, string name, string title, string description)
        {
            Id = id;
            Name = name;
            Title = title;
            Description = description;
        }

        public static bool TryParse(string name, [NotNullWhen(true)] out TagInfo? tagInfo)
        {
            return Known.TryGetValue(name, out tagInfo);
        }

        public static readonly Dictionary<string, TagInfo> Known = new Dictionary<string, TagInfo>()
        {
            { "transportation",   new TagInfo(12, "transportation", "Transportation", "Transportation of the player, be it vehicles or teleporters.") },
            { "logistics",        new TagInfo(13, "logistics", "Logistics", "Augmented or new ways of transporting materials - belts, inserters, pipes!") },
            { "combat",           new TagInfo(null, "combat", "Combat", "New ways to deal with enemies, be it attack or defense.") },
            { "enemies",          new TagInfo(17, "enemies", "Enemies", "Changes to enemies or entirely new enemies to deal with.") },
            { "armor",            new TagInfo(18, "armor", "Armor", "Armors or armor equipment.") },
            { "environment",      new TagInfo(null, "environment", "Environment", "Map generation and terrain modification.") },
            { "logistic-network", new TagInfo(20, "logistic-network", "Logistics", "Related to roboports and logistic robots") },
            { "circuit-network",  new TagInfo(null, "circuit-network", "Circuit", "Entities which interact with the circuit network.") },
            { "storage",          new TagInfo(21, "storage", "Storage", "Greater than just chests.") },
            { "power",            new TagInfo(22, "power", "Power", "Production 	Changes to power production and distribution.") },
            { "manufacturing",    new TagInfo(23, "manufacturing", "Manufacture", "Furnaces, assembling machines, production chains") },
            { "blueprints",       new TagInfo(24, "blueprints", "Blueprints", "Change blueprint behavior.") },
            { "cheats",           new TagInfo(25, "cheats", "Cheats", "Play it your way.") },
            { "mining",           new TagInfo(27, "mining", "Mining", "New ores and resources as well as machines.") },
            { "fluids",           new TagInfo(null, "fluids", "Fluids", "Things related to oil and other fluids.") },
            { "trains",           new TagInfo(29, "trains", "Trains", "Trains are great, but what if they could do even more? ") },
            { "planets",          new TagInfo(null, "planets", "Planets", "Greater horizons to explore!") },
        };

        public bool Equals(TagInfo? other)
        {
            if (other is null)
                return false;

            return Name.Equals(other.Name);
        }

        public override bool Equals(object? obj)
        {
            if (obj is not TagInfo tag)
                return false;

            return Equals(tag);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                Id?.GetHashCode(),
                Name.GetHashCode(),
                Title.GetHashCode(),
                Description?.GetHashCode());
        }

        public override string ToString()
            => Name;
    }
}
