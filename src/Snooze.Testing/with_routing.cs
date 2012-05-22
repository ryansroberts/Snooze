using System.Web.Routing;
using RouteCollectionExtensions = Snooze.Routing.RouteCollectionExtensions;

namespace Snooze.Testing
{
    public static class with_routing<TResource>
    {
        public static void enabled()
        {
            if (RouteTable.Routes.Count > 0) return;
                RouteCollectionExtensions.FromAssemblyWithType<TResource>(RouteTable.Routes);
        }
    }
}