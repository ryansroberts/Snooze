using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Machine.Specifications;
using Moq;
using Snooze.Routing;
using It = Machine.Specifications.It;

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

    public class Excluding_properties_from_the_query_string
    {
        public class IdUrl : Url
        {
            public string Id { get; set; }
        }

        public class FakeController : ResourceController
        {
            public void Get(IdUrl url) { }
        }

        protected static Mock<HttpContextBase> httpContext;

        static string s;

        Establish context = () =>
                                        {
                                            RouteTable.Routes.Map<IdUrl>(u => "");
                                            httpContext = new Mock<HttpContextBase>();
                                            httpContext.SetupGet(h => h.Request.PathInfo).Returns("");
                                            Url.DoNotMapToUrlWhere(p => p.Name == "Id" && p.DeclaringType == typeof(IdUrl));
                                        };

        Because of = () => s = (new IdUrl() { Id = "id" }).ToString();

        It should_not_have_id_in_the_query_string = () =>
            s.Contains("?").ShouldBeFalse();

        Cleanup after_each = () =>
        {
            RouteTable.Routes.Clear();
            ModelBinders.Binders.Clear();
        };
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