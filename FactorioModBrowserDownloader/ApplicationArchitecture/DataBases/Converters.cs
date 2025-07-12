using FactorioNexus.ApplicationArchitecture.Models;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;

namespace FactorioNexus.ApplicationArchitecture.DataBases
{
    public class CategoryConverter : ValueConverter<CategoryInfo?, string?>
    {
        public CategoryConverter(ConverterMappingHints? mappingHints = null)
            : base(v => ToProvider(v), p => FromProvider(p), mappingHints) { }

        private static string? ToProvider(CategoryInfo? version)
            => version?.Name;

        private static CategoryInfo? FromProvider(string? provider)
            => provider is null ? null : CategoryInfo.Known.GetValueOrDefault(provider);
    }

    public class VersionConverter : ValueConverter<Version, string>
    {
        public VersionConverter(ConverterMappingHints? mappingHints = null)
            : base(v => ToProvider(v), p => FromProvider(p), mappingHints) { }

        private static string ToProvider(Version? version)
            => version?.ToString() ?? "0.0";

        private static Version FromProvider(string? provider)
            => Version.Parse(provider ?? "0.0");
    }

    public class JsonValueConverter<TModel> : ValueConverter<TModel?, string?>
    {
        public JsonValueConverter(ConverterMappingHints? mappingHints = null)
            : base(v => ToProvider(v), p => FromProvider(p), mappingHints) { }

        private static string? ToProvider(TModel? model)
            => JsonSerializer.Serialize(model, Constants.JsonOptions);

        private static TModel? FromProvider(string? provider)
            => provider is null ? default : JsonSerializer.Deserialize<TModel>(provider, Constants.JsonOptions);
    }
}
