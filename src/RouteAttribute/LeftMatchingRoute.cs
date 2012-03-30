using System.Globalization;
using System.Web.Routing;

namespace StackExchange.Helpers
{
    internal class LeftMatchingRoute : Route
    {
        private readonly string neededOnTheLeft;
        public LeftMatchingRoute(string url, IRouteHandler handler)
            : base(url, handler)
        {
            int idx = url.IndexOf('{');
            neededOnTheLeft = "~/" + (idx >= 0 ? url.Substring(0, idx) : url).TrimEnd('/');
        }
        public override RouteData GetRouteData(System.Web.HttpContextBase httpContext)
        {
            if (!httpContext.Request.AppRelativeCurrentExecutionFilePath.StartsWith(neededOnTheLeft, true, CultureInfo.InvariantCulture)) return null;
            return base.GetRouteData(httpContext);
        }
    }
}
