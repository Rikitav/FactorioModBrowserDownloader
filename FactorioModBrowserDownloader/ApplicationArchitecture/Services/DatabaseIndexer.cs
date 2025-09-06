using FactorioNexus.ApplicationArchitecture.DataBases;
using FactorioNexus.ApplicationArchitecture.Dependencies;
using FactorioNexus.ApplicationArchitecture.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace FactorioNexus.ApplicationArchitecture.Services
{
    public class DatabaseIndexer : IDatabaseIndexer
    {
        public async Task RepopulateFrom(JsonDocument document, CancellationToken cancellationToken)
        {
            using IndexedModPortalDatabase dataBase = App.Services.GetRequiredService<IndexedModPortalDatabase>();
            await dataBase.Database.EnsureDeletedAsync(cancellationToken);
            await dataBase.Database.EnsureCreatedAsync(cancellationToken);

            foreach (JsonElement element in document.RootElement.GetProperty("results").EnumerateArray())
            {
                cancellationToken.ThrowIfCancellationRequested();
                ModEntryInfo? modPage = element.Deserialize<ModEntryInfo>(Constants.JsonOptions);
                if (modPage == null)
                    continue;

                ModEntryEntity entity = new ModEntryEntity(modPage);
                await dataBase.Items.AddAsync(entity, cancellationToken);
            }

            await dataBase.SaveChangesAsync(cancellationToken);
        }

        public IEnumerable<ModEntryInfo> GetEntries(QueryFilterSettings queryFilters, CancellationToken cancellationToken = default)
        {
            using IndexedModPortalDatabase dataBase = App.Services.GetRequiredService<IndexedModPortalDatabase>();
            foreach (ModEntryInfo entry in dataBase.Items.Where(queryFilters.CanPass).OrderBy(queryFilters.SortDescending, queryFilters.SortSelector).Select(entity => entity.ToInfo()).ToList())
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return entry;
            }
        }
    }
}
