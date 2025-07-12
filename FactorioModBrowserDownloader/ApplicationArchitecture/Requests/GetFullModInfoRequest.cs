using FactorioNexus.ApplicationArchitecture.Models;

namespace FactorioNexus.ApplicationArchitecture.Requests
{
    public class GetFullModInfoRequest(string modId) : ApiRequestBase<ModEntryFull>("mods", modId, "full")
    {

    }
}
