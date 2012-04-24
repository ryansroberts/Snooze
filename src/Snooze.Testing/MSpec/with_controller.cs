using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Xml;
using System.Xml.Resolvers;
using HtmlAgilityPack;
using Machine.Specifications;
using Moq;
using Newtonsoft.Json.Linq;
using Snooze.Testing;
using Snooze.ViewTesting.Spark;
using Snooze.Routing;

namespace Snooze.MSpec
{
    public class with_controller<TResource, THandler> : with_auto_mocking<THandler>
        where THandler : ResourceController
    {
        protected static ResourceResult result;
        static string pathToApplicationUnderTest;
        protected static HttpCookieCollection _requestCookies;
        protected static NameValueCollection _requestHeaders;



        protected static void application_under_test_is_here(string path)
        {
            ViewEngines.Engines.Clear();
            pathToApplicationUnderTest = new Uri(
                (
				 Path.GetDirectoryName(Assembly.GetCallingAssembly().CodeBase) + "\\..\\..\\" + path).Replace("\\", "/"))
                .AbsoluteUri
                .Replace("file:///", "")
                .Replace("/", "\\");

            ViewEngines.Engines.Add(
                new TestableSparkViewEngine(HttpUtility.UrlDecode(pathToApplicationUnderTest)));
        }

		Establish context =() => {
		                           with_routing<THandler>.enabled();
                                   _requestCookies = new HttpCookieCollection();
                                   _requestHeaders = new NameValueCollection();
		                       };

        Cleanup teardown = () =>
                               {
                                   RouteTable.Routes.Clear();
                                   Snooze.Routing.RouteCollectionExtensions.ClearSnoozeCache();
                                   ModelBinders.Binders.Clear();
                               };

		static string lasturi;

		protected static THandler GetController() { return autoMocker.ClassUnderTest; }

		protected static TResource Resource
		{
			get { return (TResource) result.Resource; }
		}

		protected static ILookup<string, object> ResponseHeaders
		{
			get { return result.Headers; }
		}

	    protected static IList<HttpCookie> ResponseCookies
	    {
            get { return result.Cookies; }
	    }

        protected static string ContentType
        {
            get { return result.ContentType; }
        }

		public static FakeCachePolicy cachepolicy
		{
			get
			{
				var policy = new FakeCachePolicy();
				var context = new Mock<HttpContextBase>();

				context.SetupGet(p => p.Response.ContentType)
					.Returns("test");

				foreach (var action in result.CacheActions)
					action(context.Object, policy);

				return policy;
			}
		}

		public static void public_cached() { cachepolicy.Cachability.ShouldEqual(HttpCacheability.Public); }

		public static void has_etag(string etag) { cachepolicy.Etag.ShouldEqual(etag); }

		static void InvokeAction(string httpMethod, RouteData route, object[] additionalParameters, NameValueCollection queryString)
		{
			var urlType = route.Route.GetType().GetGenericArguments()[0];

			var methods =
				(from m in typeof (THandler).GetMethods()
				where m.Name.Equals(httpMethod, StringComparison.OrdinalIgnoreCase)
				let parameters = m.GetParameters()
				where parameters.Length > 0
				      && parameters[0].ParameterType == route.Route.GetType().GetGenericArguments()[0]
				select m).ToList();

			autoMocker.ClassUnderTest.HttpVerb = (SnoozeHttpVerbs)Enum.Parse(typeof(SnoozeHttpVerbs), httpMethod, true);

			if (methods.Count==0)
				throw new InvalidOperationException("No action for uri " + urlType.Name + " method " + httpMethod);

		    var methodParams = methods[0].GetParameters();
		    var hasMultipleParams = methodParams.Length > 1;

		    var command = additionalParameters.Any() ? CreateCommandFromAdditionalParameters(route, additionalParameters, queryString) : CreateCommandFromUri(route, queryString);


            var actionDict = new Dictionary<string, object>();

            if(command!=null)
            {
                var name = methodParams.Length > 0 ? methodParams[0].Name : "Command";
                actionDict.Add(name,command);
            }

            if (additionalParameters.Length > 1)
            {
                for (var i = 1; i < additionalParameters.Length; i++)
                {
                    actionDict[methodParams[i].Name] = additionalParameters[i];
                }
            }


		    var filterContext = CallActionExecuting(httpMethod, actionDict, methods);


            if (filterContext.Result != null)
            {
                result = filterContext.Result as ResourceResult;
                return;
            }

            if (hasMultipleParams)
			    InvokeUrlAndModel(route, additionalParameters, queryString, methods);
			else
			    InvokeCommand(command, methods);
			
		}


        static ActionExecutingContext CallActionExecuting(string httpMethod, Dictionary<string, object> actionDict, IEnumerable<MethodInfo> methods)
        {
            var executingContext = new ActionExecutingContext(ControllerContext(), 
                new ReflectedActionDescriptor(methods.First(), httpMethod, new ReflectedControllerDescriptor(typeof(THandler))), actionDict);

            typeof(THandler).InvokeMember("OnActionExecuting",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod,
                null,
                class_under_test,
                new[] { executingContext });

            return executingContext;
        }


    	static void InvokeCommand(object command, IEnumerable<MethodInfo> methods)
    	{
	
    		var args = new List<object>(new[] {command});

			result = (ResourceResult) methods.First().Invoke(autoMocker.ClassUnderTest, args.ToArray());
		}

    	static object CreateCommandFromUri(RouteData route, NameValueCollection queryString)
    	{
    		object command;
    		command = FromContext(route, queryString);
    		return command;
    	}

    	static object CreateCommandFromAdditionalParameters(RouteData route,
    	                                                    object[] additionalParameters,
    	                                                    NameValueCollection queryString)
    	{
    		object command;
    		command = additionalParameters.First();
            foreach (var prop in FromContext(route, queryString).GetType().GetProperties().Where(p => p.GetSetMethod(false) != null))
    			command.SetPropertyValue(prop.Name, additionalParameters.First().GetPropertyValue(prop.Name));
    		return command;
    	}

    	static void InvokeUrlAndModel(RouteData route, object[] additionalParameters, NameValueCollection queryString, IEnumerable<MethodInfo> methods)
		{
			var args = new List<object>(new[] {FromContext(route, queryString)});
			args.AddRange(additionalParameters);

			result = (ResourceResult) methods.First().Invoke(autoMocker.ClassUnderTest, args.ToArray());
		}

		protected static Url FromContext(RouteData data, NameValueCollection queryString)
		{
			var url = Activator.CreateInstance(data.Route.GetType().GetGenericArguments()[0]);

			AssignParentUrl(url, data, queryString);

			AssignUrlProperties(data, url, queryString);
			return (Url) url;
		}


		protected static object Convert(object value, Type type)
		{
			if (value != null)
			{
				if (value.GetType() == type)
					return value;
				var converter1 = TypeDescriptor.GetConverter(value);
				if (converter1 != null && converter1.CanConvertTo(type))
					return converter1.ConvertTo(value, type);
				var converter2 = TypeDescriptor.GetConverter(type);
				if (converter2 != null && converter2.CanConvertFrom(value.GetType()))
					return converter2.ConvertFrom(value);
			}
			return null;
		}


		static void AssignUrlProperties(RouteData data, object url, NameValueCollection queryString)
		{
			foreach (var v in data.Values.Where(v => url.GetType().GetProperty(v.Key) != null))
			{
				var pInfo = url.GetType().GetProperty(v.Key);

				pInfo.SetValue(url, Convert(v.Value, pInfo.PropertyType), null);
			}

			foreach (var key in queryString.AllKeys.Where(k => url.GetType().GetProperty(k) != null))
			{
				var pInfo = url.GetType().GetProperty(key);

				pInfo.SetValue(url, Convert(queryString[key], pInfo.PropertyType), null);
			}
		}

		static void AssignParentUrl(object url, RouteData data, NameValueCollection queryString)
		{
			while (url.GetType().BaseType.IsGenericType)
			{
				var parentType = url.GetType().BaseType
					.GetGenericArguments()[0];

				var parentUrl = Activator.CreateInstance(parentType);

				AssignUrlProperties(data, parentUrl, queryString);

				if(url.GetType().GetProperty("Parent") != null)
					url.SetPropertyValue("Parent", parentUrl);

				url = parentUrl;
			}
		}

		protected static void has_expected_resource_type() { Resource.GetType().ShouldEqual(typeof (TResource)); }
	
		static NameValueCollection GetQueryString(string uri) { return HttpUtility.ParseQueryString(new Uri("http://local.com/" + uri).Query); }

		static RouteData GetRouteData(string uri)
		{
			var path = new Uri("http://local.com/" + uri).AbsolutePath;
			var qs = new Uri("http://local.com/" + uri).Query;
		    var context = new FakeHttpContext(new FakeHttpRequest(path) { _queryString = HttpUtility.ParseQueryString(qs),
                                                                          _cookies = _requestCookies,
                                                                          _headers = _requestHeaders
            });

		    foreach (var route in RouteTable.Routes)
		    {
		        Console.WriteLine(route);
		    }
            

			var routeData = RouteTable.Routes.GetRouteData(context);

			if (routeData == null)
				throw new InvalidOperationException("No route for " + uri);
			return routeData;
		}

		protected static void FauxHttp(string method, string uri, params object[] @params)
		{
			if (uri.StartsWith("/"))
				throw new ApplicationException("Uris in specs should not start with /");

			lasturi = uri;
			CallValidatorsOn(@params);

			InvokeAction(method, GetRouteData(uri), @params, GetQueryString(uri));
		}

		private static void CallValidatorsOn(object[] @params)
		{
			if (!@params.Any()) return;
			var metadata = ModelMetadataProviders.Current.GetMetadataForType(() => @params[0], @params[0].GetType());
			var controllerContext = ControllerContext();

			var validators = ModelValidatorProviders.Providers.GetValidators(metadata, controllerContext);

			foreach (var error in
								  from p in @params.Where(p => p != null)
								  from validator in validators
								  from error in validator.Validate(p)
								  select error)
			{
				class_under_test.ModelState.AddModelError(error.MemberName, error.Message);
			}
		}

		protected static void get(string uri) { FauxHttp("GET", uri, new object[] {}); }

		protected static void get<TUrl>(string uri) { FauxHttp("GET", uri, new object[] { }); }

		protected static void get(string uri, params object[] @params) { FauxHttp("GET", uri, @params); }


        protected static void copy(string uri) { FauxHttp("COPY", uri, new object[] { }); }

        protected static void copy(string uri, params object[] @params) { FauxHttp("COPY", uri, @params); }


		protected static void post(string uri, params object[] @params) { FauxHttp("POST", uri, @params); }

		protected static void put(string uri, params object[] @params) { FauxHttp("PUT", uri, @params); }

		protected static void delete(string uri, params object[] @params) { FauxHttp("DELETE", uri, @params); }

		protected static void patch(string uri, params object[] @params) { FauxHttp("PATCH", uri, @params); }

		protected static void is_403() { result.StatusCode.ShouldEqual(403); }

		protected static void is_404() { result.StatusCode.ShouldEqual(404); }

		protected static void is_200() { result.StatusCode.ShouldEqual(200); }

		protected static void is_201() { result.StatusCode.ShouldEqual(201); }

		protected static void is_303() { result.StatusCode.ShouldEqual(303); }

		protected static void is_304() { result.StatusCode.ShouldEqual(304); }

		protected static void is_301(string location)
		{
			result.StatusCode.ShouldEqual(301);
			has_location_header(location);
		}

		protected static void has_header(string key)
		{
			result.Headers.Where(h => h.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase))
				.ShouldNotBeEmpty();
		}

		protected static void has_location_header(string value)
		{
			var header = result.Headers.FirstOrDefault(h => h.Key.Equals("location", StringComparison.InvariantCultureIgnoreCase));

			header.ShouldNotBeNull();
			header.First().ShouldEqual(value);
		}

		protected static void resource_is_of_type<T>()
		{
			result.Resource.ShouldBeOfType<T>();
		}


		protected static JObject conneg_json(string accept = "application/json")
		{
			var httpContext = Render(accept);

            return JObject.Parse(httpContext._response.ResponseOutput);
		}

		protected static HtmlDocument conneg_html(string accept = "text/html")
		{
			var httpContext = Render(accept);

			var doc = new HtmlDocument();

            doc.LoadHtml(httpContext._response.ResponseOutput);
			return doc;
		}

		public static void markup_is_valid_according_to_dtd(string accept = "text/html")
		{

			var items = new List<string>();
			var settings = new XmlReaderSettings();
			settings.ValidationEventHandler += (s, e) => items.Add(e.Message);
			settings.ValidationType = ValidationType.DTD;
			settings.DtdProcessing = DtdProcessing.Parse;
			settings.XmlResolver = new XmlPreloadedResolver();

			var httpContext = Render(accept);

			using (var reader = XmlReader.Create(new StringReader(httpContext._response.ResponseOutput),settings))
			{
				while (reader.Read()) {}
			}

			if(items.Any())
				throw new SpecificationException("Markup is invalid\r\n" + items
				                                                           	.Aggregate(new StringBuilder(),
				                                                           		(s, r) =>
				                                                           		{
				                                                           			s.Append(r);
				                                                           			s.Append("\r\n");
				                                                           			return s;
				                                                           		},
				                                                           		s => s.ToString()));

		}
	
		static FakeHttpContext Render(string accept)
		{
			var controllerContext = ControllerContext(accept);

			result.ExecuteResult(controllerContext);
            controllerContext.HttpContext.Response.Flush();
			controllerContext.HttpContext.Response.OutputStream.Seek(0, SeekOrigin.Begin);
			return (FakeHttpContext) controllerContext.HttpContext;
		}

    	static ControllerContext ControllerContext(string accept = "*/*")
    	{
            var httpContext = new FakeHttpContext(new FakeHttpRequest(lasturi)
            {
                _acceptTypes = new[] { accept },
                _cookies = _requestCookies,
                _headers = _requestHeaders
            });
    		return  new ControllerContext(new RequestContext(httpContext, GetRouteData(lasturi)), GetController());
    	}
    }


	


}
