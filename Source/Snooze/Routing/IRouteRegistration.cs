using System.Web.Routing;

namespace Snooze.Routing
{
    public interface IRouteRegistration
    {
        void Register(RouteCollection routes);
    }
}