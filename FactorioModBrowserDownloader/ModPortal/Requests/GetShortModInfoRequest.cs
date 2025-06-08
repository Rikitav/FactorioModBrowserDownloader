using FactorioNexus.ModPortal.Types;

namespace FactorioNexus.ModPortal.Requests
{
    public class GetShortModInfoRequest(string modId) : ApiRequestBase<ModPageFullInfo>("mods", modId)
    {

    }
}
