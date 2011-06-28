using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Machine.Specifications;
using Moq;
using Snooze.Routing;
using It = Machine.Specifications.It;
using MoqIt = Moq.It;

namespace Snooze
{
    [Subject("Url specifications")]
    public class UrlSpec
    {

        public class When_converting_a_url_with_querystring : UrlContext
        {
            Because of = () => url = new UrlWithQueryString() { OrderId = 1};

            It Should_match_the_configured_route = () => UrlWithContext.ShouldEqual("/urlwithqs?OrderId=1");
        }


        public class When_converting_a_descendant_url_with_querystring: UrlContext
        {
            Because of = () => url = new UrlWithQueryStringDescendantUrl { OrderId = 1, Another = "test" };

            It Should_match_the_configured_route = () => UrlWithContext.ShouldEqual("/urlwithqs2/test?OrderId=1");
        }


        public class When_converting_a_url_to_a_string_with_a_request_context : UrlContext
        {
            Because of = () => url = new CustomerUrl { Id = 1 };

            It Should_match_the_configured_route = () => UrlWithContext.ShouldEqual("/customer/1");
        }

        public class When_converting_a_url_to_a_string_without_a_request_context : UrlContext
        {
            Because of = () => url = new CustomerUrl { Id = 1 };

            It Should_match_the_configured_route = () => UrlWithNoContext.ShouldEqual("/customer/1");
        }

        public class When_converting_a_suburl_to_a_string_with_a_request_context : UrlContext
        {
            Because of = () => url = new OrderUrl { Parent = new CustomerUrl { Id = 1 }, OrderId = 2 };

            It Should_match_the_configured_route = () => UrlWithContext.ShouldEqual("/customer/1/order/2");
        }

        public class When_converting_a_suburl_to_a_string_with_no_request_context : UrlContext
        {
            Because of = () => url = new OrderUrl { Parent = new CustomerUrl { Id = 1 }, OrderId = 2 };

            It Should_match_the_configured_route = () => UrlWithNoContext.ShouldEqual("/customer/1/order/2");
        }

        public class When_concatenating_a_url : UrlContext
        {
            Because of = () => url = new CustomerUrl { Id = 1 }.Concat(new OrderUrl { OrderId = 2 });

            It Should_match_the_configured_route = () => UrlWithContext.ShouldEqual("/customer/1/order/2");
        }

        public class When_using_a_wildcard_url : UrlContext
        {
            Because of = () => url = new ContentUrl { Path = "path/to/something" };

            It Should_allow_slashes = () => UrlWithContext.ShouldEqual("/content/path/to/something");
        }

    }

    public class UrlContext
    {
        protected class CustomerUrl : Url
        {
            public int Id { get; set; }
        }

        protected class UrlWithQueryStringDescendantUrl : UrlWithQueryString
        {
            public string Another { get; set; }
        }

        protected class OrderUrl : SubUrl<CustomerUrl>
        {
            public int OrderId { get; set; }
        }

        protected class UrlWithQueryString : Url
        {
            public int OrderId { get; set; }
        }

        protected class ContentUrl : Url
        {
            public string Path { get; set; }
        }

        class TestController : ResourceController
        {
            public void Get(CustomerUrl url) { }
            public void Get(OrderUrl url) { }
            public void Get(ContentUrl url) { }
            public void Get(UrlWithQueryString url) { }
            public void Get(UrlWithQueryStringDescendantUrl url) { }

        }


        static RequestContext _requestContext;

        Establish context = () =>
            {
                var http = new Mock<HttpContextBase>();
                http.SetupGet(h => h.Request.ApplicationPath).Returns("/");
                http.Setup(h => h.Response.ApplyAppPathModifier(MoqIt.IsAny<string>())).Returns((string s) => s);
                _requestContext = new RequestContext(http.Object, new RouteData());

                RouteTable.Routes.Map<CustomerUrl>(c => "customer/" + c.Id);                
                RouteTable.Routes.Map<OrderUrl>(o => "order/" + o.OrderId);
                RouteTable.Routes.Map<ContentUrl>(c => "content/" + c.Path.CatchAll());
                RouteTable.Routes.Map<UrlWithQueryString>(u => "urlwithqs");
                RouteTable.Routes.Map<UrlWithQueryStringDescendantUrl>(c => "urlwithqs2/" + c.Another);

            };

        protected static Url url;

        protected static string UrlWithContext
        {
            get { return url.ToString(_requestContext); }
        }

        protected static string UrlWithNoContext
        {
            get { return url.ToString(); }        
        }

        Cleanup after_each =()=>
        {
            RouteTable.Routes.Clear();
            ModelBinders.Binders.Clear();
        };
    }
}
