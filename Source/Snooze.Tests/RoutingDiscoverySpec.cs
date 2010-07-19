using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;
using Machine.Specifications;
using Snooze.Routing;

namespace Snooze
{
    [Subject("Convention based route discovery")]
    public class RoutingDiscoverySpec
    {
        public class When_discovering_available_routes_for_an_assembly
        {
            static IEnumerable<IRouteRegistration> registrations;
            static RoutingRegistrationDiscovery discovery;

            Establish context = () => discovery = new RoutingRegistrationDiscovery();

            Because of = () => registrations =  discovery.Scan(typeof(TestRegistration).Assembly);

            It Should_have_discovered_the_test_registration = () => 
                registrations.First().ShouldBeOfType<TestRegistration>();

         }
    }

    public class TestRegistration : IRouteRegistration
    {
        public void Register(RouteCollection routes)
        {
            throw new NotImplementedException();
        }
    }
}