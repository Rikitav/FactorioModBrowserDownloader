using FactorioNexus.Infrastructure.Models;

namespace FactorioNexus.Infrastructure.Requests
{
    public class GetFullModInfoRequest(string modId) : ApiRequestBase<ModEntryFull>("mods", modId, "full")
    {

    }
}
