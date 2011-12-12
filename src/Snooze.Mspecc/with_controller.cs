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
using MvcContrib.TestHelper.Fakes;
using Snooze.ViewTesting.Spark;

namespace Snooze.MSpec
{
	public class with_controller<TResource, THandler> : with_auto_mocking<THandler>
		where THandler : ResourceController
	{
		static ResourceResult result;
		static string pathToApplicationUnderTest;

        protected static void application_under_test_is_here(string path)
        {
            application_under_test_is_here(path, null);
        }

	    protected static void application_under_test_is_here(string path, ISparkSettings sparkSettings)
		{
			ViewEngines.Engines.Clear();
			pathToApplicationUnderTest = new Uri(
				(Path.GetDirectoryName(Assembly.GetCallingAssembly().CodeBase) + "\\..\\..\\" + path).Replace("\\", "/"))
				.AbsoluteUri
				.Replace("file:///", "")
				.Replace("/", "\\");
			ViewEngines.Engines.Add(
                new TestableSparkViewEngine(pathToApplicationUnderTest, sparkSettings));
		}

		Establish routing = with_routing<THandler>.enabled;
		[ThreadStatic] static string lasturi;

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

		static void InvokeAction(string httpMethod,
		                         RouteData route,
		                         object[] additionalParameters,
		                         NameValueCollection queryString)
		{
			var urlType = route.Route.GetType().GetGenericArguments()[0];

			var methods =
				from m in typeof (THandler).GetMethods()
				where m.Name.Equals(httpMethod, StringComparison.OrdinalIgnoreCase)
				let parameters = m.GetParameters()
				where parameters.Length > 0
				      && parameters[0].ParameterType.Equals(route.Route.GetType().GetGenericArguments()[0])
				select m;

            autoMocker.ClassUnderTest.HttpVerb = (HttpVerbs)Enum.Parse(typeof(HttpVerbs), httpMethod, true);

			if (methods.Count() == 0)
				throw new InvalidOperationException("No action for uri " + urlType.Name + " method " + httpMethod);

			if (methods.First().GetParameters().Count() > 1)
			{
				InvokeUrlAndModel(route, additionalParameters, queryString, methods);
			}
			else
			{
				InvokeCommand(route, additionalParameters, queryString, methods);
			}

		}

		static void InvokeCommand(
            RouteData route, object[] additionalParameters, 
            NameValueCollection queryString, 
            IEnumerable<MethodInfo> methods)
		{
			var command = FromContext(route, queryString);

			if (additionalParameters.Any())
			{
				foreach (var prop in additionalParameters.First().GetType().GetProperties())
				{
					command.SetPropertyValue(prop.Name, additionalParameters.First().GetPropertyValue(prop.Name));
				}
			}

			var args = new List<object>(new[] {command});

			result = (ResourceResult) methods.First().Invoke(autoMocker.ClassUnderTest,
				args.ToArray());
		}

		static void InvokeUrlAndModel(RouteData route, object[] additionalParameters, 
            NameValueCollection queryString, 
            IEnumerable<MethodInfo> methods)
		{
			var args = new List<object>(new[] {FromContext(route, queryString)});
			args.AddRange(additionalParameters);

			result = (ResourceResult) methods.First().Invoke(autoMocker.ClassUnderTest,
				args.ToArray());
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
			var context = new FakeHttpContext("~" + path, null, null, HttpUtility.ParseQueryString(qs), null, null);

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

			InvokeAction(method, GetRouteData(uri), @params, GetQueryString(uri));
		}

		protected static void get(string uri) { FauxHttp("GET", uri, new object[] {}); }

		protected static void get(string uri, params object[] @params) { FauxHttp("GET", uri, @params); }

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
			var header = result.Headers.Where(h => h.Key.Equals("location", StringComparison.InvariantCultureIgnoreCase))
				.FirstOrDefault();

			header.ShouldNotBeNull();
			header.First().ShouldEqual(value);
		}

		protected static JObject conneg_json(string accept = "application/json")
		{
			var httpContext = Render(accept);

			return JObject.Parse(((HttpResponseForViewExecution) httpContext.Response).Content());
		}

		protected static HtmlDocument conneg_html(string accept = "text/html")
		{
			var httpContext = Render(accept);

			var doc = new HtmlDocument();
		
			doc.LoadHtml(((HttpResponseForViewExecution) httpContext.Response).Content());
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

			using (var reader = XmlReader.Create(new StringReader( ((HttpResponseForViewExecution) httpContext.Response).Content()),settings))
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
			var httpContext = FakeHttpContext.Root();
			var request = new HttpRequestForViewExecution(new[] {accept}, lasturi);
			var reponse = new HttpResponseForViewExecution();
			httpContext.SetRequest(request);
			httpContext.SetResponse(reponse);
			result.ExecuteResult(new ControllerContext(
				new RequestContext(httpContext, GetRouteData(lasturi)), GetController()));

			httpContext.Response.OutputStream.Seek(0, SeekOrigin.Begin);
			return httpContext;
		}
	}


	public class HttpRequestForViewExecution : FakeHttpRequest
	{
		string[] acceptTypes;
		public HttpRequestForViewExecution(string[] acceptTypes,string uri) : base(uri,null,null)
		{
			this.acceptTypes = acceptTypes;
		}

		public override NameValueCollection QueryString
		{
			get
			{
				return new NameValueCollection();
			}
		}

		public override string[] AcceptTypes
		{
			get { return acceptTypes; }
		}
	}

	public class HttpResponseForViewExecution : FakeHttpResponse
	{
		public MemoryStream Buf = new MemoryStream();
		public StreamWriter writer;

		public override Stream OutputStream
		{
			get { return Buf; }
		}

		public string Content()
		{
			writer.Flush();
			Buf.Seek(0, SeekOrigin.Begin);

			return Buf.ReadToEnd();
		}

		public override string ContentType { get; set; }
		public override void AddHeader(string name, string value)
		{
			
		}


		public override TextWriter Output
		{
			get
			{
				return  writer?? (writer = new StreamWriter(Buf));
			}
			set { throw new NotImplementedException(); }
		}
	}



}
