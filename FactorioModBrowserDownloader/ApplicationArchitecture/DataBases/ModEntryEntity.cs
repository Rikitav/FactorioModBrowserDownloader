using FactorioNexus.ApplicationArchitecture.Models;

namespace FactorioNexus.ApplicationArchitecture.DataBases
{
    public class ModEntryEntity
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string? OwnerName { get; set; }
        public string? Summary { get; set; }
        public int DownloadsCount { get; set; }
        public decimal? Score { get; set; }
        public CategoryInfo? Category { get; set; }
        public ReleaseInfo? LatestRelease { get; set; }
        public ReleaseInfo[]? Releases { get; set; }

        public ReleaseInfo? DisplayLatestRelease => LatestRelease ?? Releases?.LastOrDefault();

        public ModEntryEntity()
        {
            Id = string.Empty;
            Title = string.Empty;
        }

        public ModEntryEntity(ModEntryInfo info)
        {
            Id = info.Id;
            Title = info.Title;
            OwnerName = info.OwnerName;
            Summary = info.Summary;
            DownloadsCount = info.DownloadsCount;
            Score = info.Score;
            Category = info.Category;
            LatestRelease = info.LatestRelease;
            Releases = info.Releases;
        }

        public ModEntryInfo ToInfo() => new ModEntryInfo
        {
            Id = Id,
            Title = Title,
            OwnerName = OwnerName,
            Summary = Summary,
            DownloadsCount = DownloadsCount,
            Score = Score,
            Category = Category,
            LatestRelease = LatestRelease,
            Releases = Releases
        };
    }
}
