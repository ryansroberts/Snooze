using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Machine.Specifications;
using Moq;
using Snooze.Routing;
using It = Machine.Specifications.It;

namespace Snooze
{
    [Subject("Routing specifications")]
    public class RoutingSpec
    {
        public class When_routing_a_url : RoutingContext
        {
            Because of = () => RoutingTo("~/customers");

            Behaves_like<Route> mvcroute;
        }

        [Subject(typeof(Url))]
        public class When_routing_a_url_with_a_parameter : RoutingContext
        {
            Because of = () => RoutingTo("~/customers/42");

            Behaves_like<Route> mvcroute;

            It Should_have_captured_the_parameter = () => v("CustomerId").ShouldEqual("42");
        }

        [Subject(typeof(SubUrl<>))]
        public class When_routing_a_suburl : RoutingContext
        {
            Because of = () => RoutingTo("~/customers/42/orders");

            Behaves_like<Route> mvcroute;
        }

        [Subject(typeof(SubUrl<>))]
        public class When_routing_a_suburl_with_parameters : RoutingContext
        {
            Because of = () => RoutingTo("~/customers/42/orders/17");

            Behaves_like<Route> mvcroute;

            It Should_have_captured_the_parent_url_parameter = () => v("CustomerId").ShouldEqual("42");

            It Should_have_captured_the_sub_url_parameter = () => v("OrderId").ShouldEqual("17");
        }

        [Subject(typeof(Url))]
        public class When_routing_a_wildcard : RoutingContext
        {
            private Because of = () =>
                                     {
                                         RoutingTo("~/content/foo/bar.xml");
                                     };

            Behaves_like<Route> mvcroute;

			It Should_have_captured_the_path = () => v("Path").ShouldEqual("foo/bar.xml");
        }
    }

    public class RoutingContext
    {
        public class CustomersUrl : Url
        {
        }
        public class CustomerUrl : SubUrl<CustomersUrl>
        {
            public int CustomerId { get; set; }
        }

        public class OrdersUrl : SubUrl<CustomerUrl>
        {
        }

        protected static Mock<HttpContextBase> httpContext;

        public class OrderUrl : SubUrl<OrdersUrl>
        {
            public string OrderId { get; set; }
        }

        public class ContentUrl : Url
        {
            public string Path { get; set; }
        }

        public class FakeController : ResourceController
        {
            public void Get(CustomersUrl url) { }
            public void Get(CustomerUrl url) { }
            public void Get(OrdersUrl url) { }
            public void Get(OrderUrl url) { }
            public void Get(ContentUrl url) { }
        }

        Establish context = () =>
        {
            RouteTable.Routes.Map<CustomersUrl>(c => "customers");
            RouteTable.Routes.Map<CustomerUrl>(c => c.CustomerId.ToString());
            RouteTable.Routes.Map<OrdersUrl>(o => "orders");
            RouteTable.Routes.Map<OrderUrl>(o => o.OrderId);
            RouteTable.Routes.Map<ContentUrl>(c => "content/" + c.Path.CatchAll());
            httpContext = new Mock<HttpContextBase>();
            httpContext.SetupGet(h => h.Request.PathInfo).Returns("");
        };

        protected static RouteData routeData;

        Cleanup after_each = () =>
            {
                ModelBinders.Binders.Clear();
            };

        protected static void RoutingTo(string path)
        {
            httpContext.SetupGet(h => h.Request.AppRelativeCurrentExecutionFilePath).Returns(path);
            routeData = RouteTable.Routes.GetRouteData(httpContext.Object);
        }

        protected static object v(string k)
        {
            return routeData.Values[k];
        }
    }

    [Behaviors]
    public class Route 
    {
        protected static RouteData routeData;

        It Should_be_routable_by_mvc = () => routeData.ShouldNotBeNull();      
    }
}
