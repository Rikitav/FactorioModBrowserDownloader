using FactorioNexus.ApplicationArchitecture.DataBases;
using FactorioNexus.ApplicationArchitecture.Models;
using System.Text.Json;

namespace FactorioNexus.ApplicationArchitecture.Dependencies
{
    public interface IDatabaseIndexer
    {
        public Task RepopulateFrom(JsonDocument document, CancellationToken cancellationToken = default);
        public IEnumerable<ModEntryInfo> GetEntries(QueryFilterSettings queryFilters, CancellationToken cancellationToken = default);
    }
}
