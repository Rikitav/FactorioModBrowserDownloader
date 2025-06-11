using System.Text.Json.Serialization;

namespace FactorioNexus.ModPortal.Types
{
    public class CategoryInfo
    {
        /// <summary>
        /// Inner name of category
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// Display name of category
        /// </summary>
        [JsonPropertyName("title")]
        public string Title { get; set; }

        /// <summary>
        /// Description of category
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; set; }

        public CategoryInfo()
        {
            Name = string.Empty;
            Title = string.Empty;
            Description = string.Empty;
        }

        public CategoryInfo(string name, string title, string description)
        {
            Name = name;
            Title = title;
            Description = description;
        }

        public static readonly Dictionary<string, CategoryInfo> Known = new Dictionary<string, CategoryInfo>()
        {
            { "no-category", new CategoryInfo("no-category", "no-category", "no-category") },
            { "content", new CategoryInfo("content", "Content", "Mods introducing new content into the game.") },
            { "overhaul", new CategoryInfo("overhaul", "Overhaul", "Large total conversion mods.") },
            { "tweaks", new CategoryInfo("tweaks", "Tweaks", "Small changes concerning balance, gameplay, or graphics.") },
            { "utilities", new CategoryInfo("utilities", "Utilities", "Providing the player with new tools or adjusting the game interface, without fundamentally changing gameplay.") },
            { "scenarios", new CategoryInfo("scenarios", "Scenarios", "Scenarios, maps, and puzzles.") },
            { "mod-packs", new CategoryInfo("mod-packs", "Mod packs", "Collections of mods with tweaks to make them work together.") },
            { "localizations", new CategoryInfo("localizations", "Localizations", "Translations for other mods.") },
            { "internal", new CategoryInfo("internal", "Internal", "Lua libraries for use by other mods and submods that are parts of a larger mod. ") }
        };
    }
}
