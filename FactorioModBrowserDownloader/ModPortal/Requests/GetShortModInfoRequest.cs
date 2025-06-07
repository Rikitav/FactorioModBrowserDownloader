using FactorioModBrowserDownloader.ModPortal.Types;

namespace FactorioModBrowserDownloader.ModPortal.Requests
{
    public class GetShortModInfoRequest(string modId) : ApiRequestBase<ModPageFullInfo>("mods", modId)
    {

    }
}
