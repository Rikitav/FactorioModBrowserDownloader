using FactorioNexus.ApplicationArchitecture.DataBases;
using FactorioNexus.Infrastructure.Models;
using System.Text.Json;

namespace FactorioNexus.Infrastructure.Services.Abstractions
{
    public interface IDatabaseIndexer
    {
        public Task RepopulateFrom(JsonDocument document, CancellationToken cancellationToken = default);
        public IEnumerable<ModEntryInfo> GetEntries(QueryFilterSettings queryFilters, CancellationToken cancellationToken = default);
    }
}
