using FactorioNexus.ApplicationArchitecture.Models;

namespace FactorioNexus.ApplicationArchitecture.Requests
{
    public class GetShortModInfoRequest(string modId) : ApiRequestBase<ModEntryShort>("mods", modId)
    {

    }
}
