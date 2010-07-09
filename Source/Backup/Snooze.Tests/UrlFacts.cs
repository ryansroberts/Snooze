using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Moq;
using Snooze.Routing;
using Xunit;

namespace Snooze
{
    public class UrlFacts : IDisposable
    {
        class CustomerUrl : Url
        {
            public int Id { get; set; }
        }

        class OrderUrl : SubUrl<CustomerUrl>
        {
            public int OrderId { get; set; }
        }

        class ContentUrl : Url
        {
            public string Path { get; set; }
        }

        class TestController : ResourceController
        {
            public void Get(CustomerUrl url) { }
            public void Get(OrderUrl url) { }
            public void Get(ContentUrl url) { }
        }


        RequestContext _requestContext;

        public UrlFacts()
        {
            var http = new Mock<HttpContextBase>();
            http.SetupGet(h => h.Request.ApplicationPath).Returns("/");
            http.Setup(h => h.Response.ApplyAppPathModifier(It.IsAny<string>())).Returns((string s) => s);
            _requestContext = new RequestContext(http.Object, new RouteData());

            RouteTable.Routes.Map<CustomerUrl>(c => "customer/" + c.Id);
            RouteTable.Routes.Map<OrderUrl>(o => "order/" + o.OrderId);
            RouteTable.Routes.Map<ContentUrl>(c => "content/" + c.Path.CatchAll());
        }

        [Fact]
        public void CustomerUrl_ToString_returns_correct_url()
        {
            var url = new CustomerUrl { Id = 1 };
            Assert.Equal("/customer/1", url.ToString(_requestContext));
        }

        [Fact]
        public void CustomerUrl_ToString_returns_correct_url_with_no_HttpContext()
        {
            var url = new CustomerUrl { Id = 1 };
            Assert.Equal("/customer/1", url.ToString());
        }

        [Fact]
        public void OrderUrl_ToString_returns_full_url()
        {
            var url = new OrderUrl { Parent = new CustomerUrl { Id = 1 }, OrderId = 2 };
            Assert.Equal("/customer/1/order/2", url.ToString(_requestContext));
        }

        [Fact]
        public void Can_get_sub_url_using_Concat_method()
        {
            var parent = new CustomerUrl { Id = 1 };
            var child = parent.Concat(new OrderUrl { OrderId = 2 });
            Assert.Equal("/customer/1/order/2", child.ToString(_requestContext));            
        }

        [Fact]
        public void ContentUrl_Path_property_can_have_slashes()
        {
            var url = new ContentUrl { Path = "foo/bar/quz" };
            Assert.Equal("/content/foo/bar/quz", url.ToString(_requestContext));
        }

        public void Dispose()
        {
            RouteTable.Routes.Clear();
            ModelBinders.Binders.Clear();
        }
    }
}
