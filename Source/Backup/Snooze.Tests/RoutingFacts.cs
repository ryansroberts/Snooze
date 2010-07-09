using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Moq;
using Xunit;
using System.Collections.Specialized;

namespace Snooze.Routing
{
    public class RoutingFacts : IDisposable
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

        public RoutingFacts()
        {
            RouteTable.Routes.Map<CustomersUrl>(c => "customers");
            RouteTable.Routes.Map<CustomerUrl>(c => c.CustomerId.ToString());
            RouteTable.Routes.Map<OrdersUrl>(o => "orders");
            RouteTable.Routes.Map<OrderUrl>(o => o.OrderId);
            RouteTable.Routes.Map<ContentUrl>(c => "content/" + c.Path.CatchAll());
            httpContext = new Mock<HttpContextBase>();
            httpContext.SetupGet(h => h.Request.PathInfo).Returns("");
        }

        public void Dispose()
        {
            RouteTable.Routes.Clear();
            ModelBinders.Binders.Clear();
        }

        Mock<HttpContextBase> httpContext;

        [Fact]
        public void Can_route_CustomersUrl()
        {
            httpContext.SetupGet(h => h.Request.AppRelativeCurrentExecutionFilePath).Returns("~/customers");
            var routeData = RouteTable.Routes.GetRouteData(httpContext.Object);
            Assert.NotNull(routeData);
        }

        [Fact]
        public void Can_route_CustomerUrl()
        {
            httpContext.SetupGet(h => h.Request.AppRelativeCurrentExecutionFilePath).Returns("~/customers/42");
            var routeData = RouteTable.Routes.GetRouteData(httpContext.Object);
            Assert.NotNull(routeData);
            Assert.Equal("42", routeData.Values["CustomerId"]);
        }

        [Fact]
        public void Can_route_OrdersUrl()
        {
            httpContext.SetupGet(h => h.Request.AppRelativeCurrentExecutionFilePath).Returns("~/customers/42/orders");
            var routeData = RouteTable.Routes.GetRouteData(httpContext.Object);
            Assert.NotNull(routeData);
            Assert.Equal("42", routeData.Values["CustomerId"]);
        }

        [Fact]
        public void Can_route_OrderUrl()
        {
            httpContext.SetupGet(h => h.Request.AppRelativeCurrentExecutionFilePath).Returns("~/customers/42/orders/17");
            var routeData = RouteTable.Routes.GetRouteData(httpContext.Object);
            Assert.NotNull(routeData);
            Assert.Equal("42", routeData.Values["CustomerId"]);
            Assert.Equal("17", routeData.Values["OrderId"]);
        }

        [Fact]
        public void Can_route_path_with_ContentUrl()
        {
            httpContext.SetupGet(h => h.Request.AppRelativeCurrentExecutionFilePath).Returns("~/content/foo/bar.xml");
            var routeData = RouteTable.Routes.GetRouteData(httpContext.Object);
            Assert.NotNull(routeData);
            Assert.Equal("foo/bar.xml", routeData.Values["Path"]);
        }
    }
}
