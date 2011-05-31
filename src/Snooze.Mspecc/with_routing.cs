using System.Web.Routing;
using RouteCollectionExtensions = Snooze.Routing.RouteCollectionExtensions;

namespace Snooze.MSpec
{
    public static class with_routing<TResource>
    {
        public static void enabled()
        {
            RouteCollectionExtensions.FromAssemblyWithType<TResource>(RouteTable.Routes);
        }
    }
}