using FactorioModBrowserDownloader.ModPortal.Types;

namespace FactorioModBrowserDownloader.ModPortal.Requests
{
    public class GetFullModInfoRequest(string modId) : ApiRequestBase<ModPageFullInfo>("mods", modId, "full")
    {

    }
}
