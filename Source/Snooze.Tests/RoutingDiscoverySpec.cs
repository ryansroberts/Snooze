using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;
using Machine.Specifications;
using Snooze.Routing;

namespace Snooze
{
    [Subject(typeof(Url))]
    public class When_converting_an_unmapped_url_to_a_string
    {
        static TestUrl testUrl;
        static string uri;
        public class TestUrl : Url
        {
        } ;

        Establish context = () => testUrl = new TestUrl();

        Because of = () => uri = testUrl.ToString();

        It Should_have_converted = () => uri.ShouldNotBeNull();
    }



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