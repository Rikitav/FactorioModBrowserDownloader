using FactorioNexus.ModPortal.Types;

namespace FactorioNexus.ModPortal.Requests
{
    public class GetFullModInfoRequest(string modId) : ApiRequestBase<ModPageFullInfo>("mods", modId, "full")
    {

    }
}
