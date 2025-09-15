using FactorioNexus.Infrastructure.Models;

namespace FactorioNexus.Infrastructure.Requests
{
    public class GetShortModInfoRequest(string modId) : ApiRequestBase<ModEntryShort>("mods", modId)
    {

    }
}
