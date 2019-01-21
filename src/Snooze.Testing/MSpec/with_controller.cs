using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Routing;
using HtmlAgilityPack;
using Machine.Specifications;
using Newtonsoft.Json.Linq;
using Snooze.Testing;

namespace Snooze.MSpec
{
    public class with_controller<TResource, THandler> : with_auto_mocking<THandler>
        where THandler : ResourceController
    {
        protected static with_controller_implementation<TResource, THandler> controllerImplmentation;

        Establish context = () =>{
                                    controllerImplmentation = new with_controller_implementation<TResource, THandler>();
                                    controllerImplmentation.Init(() => autoMocker.ClassUnderTest, message => new SpecificationException(message));
                                };

        Cleanup tearDown = () =>
                               {
                                   controllerImplmentation.CleanUp();
                                   controllerImplmentation = null;
                               };

        protected static void application_under_test_is_here(string path)
        {
            controllerImplmentation.application_under_test_is_here(Assembly.GetCallingAssembly(), path);
        }



        protected static THandler GetController() { return autoMocker.ClassUnderTest; }

        protected static TResource Resource
        {
            get { return controllerImplmentation.Resource; }
        }

        protected static ResourceResult Result
        {
            get { return controllerImplmentation.Result; }
        }

        public static HttpCookieCollection RequestCookies { get { return controllerImplmentation.RequestCookies; } }

        public static NameValueCollection RequestHeaders { get { return controllerImplmentation.RequestHeaders; } }

        protected static ILookup<string, object> ResponseHeaders
        {
            get { return controllerImplmentation.ResponseHeaders; }
        }

        protected static IList<HttpCookie> ResponseCookies
        {
            get { return controllerImplmentation.ResponseCookies; }
        }

        protected static string ContentType
        {
            get { return controllerImplmentation.ContentType; }
        }

        public static FakeCachePolicy cachepolicy
        {
            get
            {
                return controllerImplmentation.cachepolicy;
            }
        }

        public static void public_cached() { cachepolicy.Cachability.ShouldEqual(HttpCacheability.Public); }

        public static void has_etag(string etag) { cachepolicy.Etag.ShouldEqual(etag); }


        protected static Url FromContext(RouteData data, NameValueCollection queryString)
        {
            return controllerImplmentation.FromContext(data, queryString);
        }


        protected static object Convert(object value, Type type)
        {
            return controllerImplmentation.Convert(value, type);
        }


        protected static void has_expected_resource_type() { Resource.GetType().ShouldEqual(typeof(TResource)); }

        protected static void get(string uri, params object[] @params) { controllerImplmentation.get(uri, @params); }

        protected static void copy(string uri, params object[] @params) { controllerImplmentation.copy(uri, @params); }

        protected static void post(string uri, params object[] @params) { controllerImplmentation.post(uri, @params); }

        protected static void put(string uri, params object[] @params) { controllerImplmentation.put(uri, @params); }

        protected static void delete(string uri, params object[] @params) { controllerImplmentation.delete(uri, @params); }

        protected static void patch(string uri, params object[] @params) { controllerImplmentation.patch(uri, @params); }


        protected static void is_400() { controllerImplmentation.Result.StatusCode.ShouldEqual(400); }

        protected static void is_403() { controllerImplmentation.Result.StatusCode.ShouldEqual(403); }

        protected static void is_404() { controllerImplmentation.Result.StatusCode.ShouldEqual(404); }

        protected static void is_200() { controllerImplmentation.Result.StatusCode.ShouldEqual(200); }

        protected static void is_201() { controllerImplmentation.Result.StatusCode.ShouldEqual(201); }

        protected static void is_303() { controllerImplmentation.Result.StatusCode.ShouldEqual(303); }

        protected static void is_304() { controllerImplmentation.Result.StatusCode.ShouldEqual(304); }

        protected static void is_301(string location)
        {
            controllerImplmentation.Result.StatusCode.ShouldEqual(301);
            has_location_header(location);
        }

        protected static void has_header(string key)
        {
            controllerImplmentation.Result.Headers.Where(h => h.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase)).ShouldNotBeEmpty();
        }

        protected static void has_location_header(string value)
        {
            var header = controllerImplmentation.Result.Headers.FirstOrDefault(h => h.Key.Equals("location", StringComparison.InvariantCultureIgnoreCase));

            header.ShouldNotBeNull();
            header.First().ShouldEqual(value);
        }

        protected static void resource_is_of_type<T>()
        {
            controllerImplmentation.Result.Resource.ShouldBeOfExactType<T>();
        }

        protected static JObject conneg_json(string accept = "application/json")
        {
            return controllerImplmentation.conneg_json(accept);
        }

        protected static HtmlDocument conneg_html(string accept = "text/html")
        {
            return controllerImplmentation.conneg_html(accept);
        }

        public static void markup_is_valid_html5(string accept = "text/html")
        {
            controllerImplmentation.markup_is_valid_html5(accept);
        }


        public static void markup_is_valid_according_to_dtd(string accept = "text/html")
        {
            controllerImplmentation.markup_is_valid_according_to_dtd(accept);
        }

    }


}
