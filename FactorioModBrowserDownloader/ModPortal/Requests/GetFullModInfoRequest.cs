using FactorioNexus.ModPortal;
using FactorioNexus.ModPortal.Types;

namespace FactorioModBrowserDownloader.ModPortal.Requests
{
    public class GetFullModInfoRequest(string modId) : ApiRequestBase<ModPageFullInfo>("mods", modId, "full")
    {

    }
}
