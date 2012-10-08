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
using HtmlParserSharp;
using Moq;
using Newtonsoft.Json.Linq;
using Snooze.ViewTesting.Spark;

namespace Snooze.Testing
{
    internal class with_controller_implementation<TResource, THandler>
        where THandler : ResourceController
    {
        string pathToApplicationUnderTest;
        RouteData routeData;
        string lasturi;
        Func<THandler> classUnderTest;
        Func<string, Exception> createException;

        internal void application_under_test_is_here(Assembly callingAssembly, string path)
        {
            ViewEngines.Engines.Clear();
            pathToApplicationUnderTest = new Uri(
                (
                 Path.GetDirectoryName(callingAssembly.CodeBase) + "\\..\\..\\" + path).Replace("\\", "/"))
                .AbsoluteUri
                .Replace("file:///", "")
                .Replace("/", "\\");

            ViewEngines.Engines.Add(
                new TestableSparkViewEngine(HttpUtility.UrlDecode(pathToApplicationUnderTest)));
        }

        public void Init(Func<THandler> getHandler, Func<string, Exception> createSpecificationException)
        {
            createException = createSpecificationException;
            classUnderTest = getHandler;
            with_routing<THandler>.enabled();
            RequestCookies = new HttpCookieCollection();
            RequestHeaders = new NameValueCollection();
        }

        public void CleanUp()
        {
            RouteTable.Routes.Clear();
            Routing.RouteCollectionExtensions.ClearSnoozeCache();
            ModelBinders.Binders.Clear();
            routeData = null;
            lasturi = null;
            Result = null;
            pathToApplicationUnderTest = null;
            RequestCookies = null;
            RequestHeaders = null;
        }


        public HttpCookieCollection RequestCookies { get; private set; }
        public NameValueCollection RequestHeaders { get; private set; }

        public THandler GetController() { return classUnderTest(); }

        public TResource Resource
		{
			get { return (TResource) Result.Resource; }
		}

        public ResourceResult Result { get; private set; }

        public ILookup<string, object> ResponseHeaders
		{
			get { return Result.Headers; }
		}

        public IList<HttpCookie> ResponseCookies
	    {
            get { return Result.Cookies; }
	    }

        public string ContentType
        {
            get { return Result.ContentType; }
        }

		public FakeCachePolicy cachepolicy
		{
			get
			{
				var policy = new FakeCachePolicy();
				var context = new Mock<HttpContextBase>();

				context.SetupGet(p => p.Response.ContentType)
					.Returns("test");

				foreach (var action in Result.CacheActions)
					action(context.Object, policy);

				return policy;
			}
		}



		void InvokeAction(string httpMethod, RouteData route, object[] additionalParameters, NameValueCollection queryString)
		{
			var urlType = route.Route.GetType().GetGenericArguments()[0];

			var methods =
				(from m in typeof (THandler).GetMethods()
				where m.Name.Equals(httpMethod, StringComparison.OrdinalIgnoreCase)
				let parameters = m.GetParameters()
				where parameters.Length > 0
				      && parameters[0].ParameterType == route.Route.GetType().GetGenericArguments()[0]
				select m).ToList();

            classUnderTest().HttpVerb = (SnoozeHttpVerbs)Enum.Parse(typeof(SnoozeHttpVerbs), httpMethod, true);

			if (methods.Count==0)
				throw new InvalidOperationException("No action for uri " + urlType.Name + " method " + httpMethod);

		    var methodParams = methods[0].GetParameters();
		    var hasMultipleParams = methodParams.Length > 1;

		    var command = additionalParameters.Any() ? CreateCommandFromAdditionalParameters(route, additionalParameters, queryString) : CreateCommandFromUri(route, queryString);

		    var controllerContext = ControllerContext();

            CallValidatorsOn(controllerContext, additionalParameters.Any() ? additionalParameters : new[] { command });

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


            var filterContext = CallActionExecuting(controllerContext, httpMethod, actionDict, methods);

            if (filterContext.Result != null)
            {
                CallResultExecuting(controllerContext, filterContext.Result);
                Result = filterContext.Result as ResourceResult;
                return;
            }

            if (hasMultipleParams)
			    InvokeUrlAndModel(route, additionalParameters, queryString, methods);
			else
			    InvokeCommand(command, methods);
			
		}


        ActionExecutingContext CallActionExecuting(ControllerContext controllerContext, string httpMethod, Dictionary<string, object> actionDict, IEnumerable<MethodInfo> methods)
        {
            var context = new ActionExecutingContext(controllerContext, new ReflectedActionDescriptor(methods.First(), httpMethod, new ReflectedControllerDescriptor(typeof(THandler))), actionDict);

            typeof(THandler).InvokeMember("OnActionExecuting",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod,
                null,
                classUnderTest(), new[] { context });

            return context;
        }


        void CallResultExecuting(ControllerContext controllerContext, ActionResult result)
        {
            var context = new ResultExecutingContext(controllerContext,result);

            typeof(THandler).InvokeMember("OnResultExecuting",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod, null, classUnderTest(), new[] { context });

        }


    	void InvokeCommand(object command, IEnumerable<MethodInfo> methods)
    	{
	
    		var args = new List<object>(new[] {command});

            Result = (ResourceResult)methods.First().Invoke(classUnderTest(), args.ToArray());
		}

    	object CreateCommandFromUri(RouteData route, NameValueCollection queryString)
    	{
    	    object command = FromContext(route, queryString);
    	    return command;
    	}

        object CreateCommandFromAdditionalParameters(RouteData route, object[] additionalParameters,
    	                                                    NameValueCollection queryString)
    	{
            var command = additionalParameters.First();
            foreach (var prop in FromContext(route, queryString).GetType().GetProperties().Where(p => p.GetSetMethod(false) != null))
    			command.SetPropertyValue(prop.Name, additionalParameters.First().GetPropertyValue(prop.Name));
    		return command;
    	}

    	void InvokeUrlAndModel(RouteData route, IEnumerable<object> additionalParameters, NameValueCollection queryString, IEnumerable<MethodInfo> methods)
		{
			var args = new List<object>(new[] {FromContext(route, queryString)});
			args.AddRange(additionalParameters);

            Result = (ResourceResult)methods.First().Invoke(classUnderTest(), args.ToArray());
		}

        public Url FromContext(RouteData data, NameValueCollection queryString)
		{
			var url = Activator.CreateInstance(data.Route.GetType().GetGenericArguments()[0]);

			AssignParentUrl(url, data, queryString);

			AssignUrlProperties(data, url, queryString);
			return (Url) url;
		}


        public object Convert(object value, Type type)
		{
			if (value != null)
			{
				if (value.GetType() == type)
					return value;
				var converter1 = TypeDescriptor.GetConverter(value);
				if (converter1.CanConvertTo(type))
					return converter1.ConvertTo(value, type);
				var converter2 = TypeDescriptor.GetConverter(type);
				if (converter2.CanConvertFrom(value.GetType()))
					return converter2.ConvertFrom(value);
			}
			return null;
		}


		void AssignUrlProperties(RouteData data, object url, NameValueCollection queryString)
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

		void AssignParentUrl(object url, RouteData data, NameValueCollection queryString)
		{
		    var subUrl = url as ISubUrl;
            while (subUrl!=null)
            {
                var parentType = subUrl.GetParentUrlType();
				var parentUrl = Activator.CreateInstance(parentType);

				AssignUrlProperties(data, parentUrl, queryString);

				if(url.GetType().GetProperty("Parent") != null)
					url.SetPropertyValue("Parent", parentUrl);

                subUrl = parentUrl as ISubUrl;
			}
		}

		NameValueCollection GetQueryString(string uri) { return HttpUtility.ParseQueryString(new Uri("http://local.com/" + uri).Query); }

		RouteData GetRouteData(string uri)
		{
            if (routeData != null)
                return routeData;

			var path = new Uri("http://local.com/" + uri).AbsolutePath;
			var qs = new Uri("http://local.com/" + uri).Query;
		    var context = new FakeHttpContext(new FakeHttpRequest(path) { _queryString = HttpUtility.ParseQueryString(qs),
                                                                          _cookies = RequestCookies,
                                                                          _headers = RequestHeaders
            });

		    foreach (var route in RouteTable.Routes)
		    {
		        Console.WriteLine(route);
		    }
            

			routeData = RouteTable.Routes.GetRouteData(context);

			if (routeData == null)
				throw new InvalidOperationException("No route for " + uri);
			return routeData;
		}

		void FauxHttp(string method, string uri, params object[] @params)
		{
			if (uri.StartsWith("/"))
				throw new ApplicationException("Uris in specs should not start with /");

			lasturi = uri;

			InvokeAction(method, routeData ?? GetRouteData(uri), @params, GetQueryString(uri));
		}

		private void CallValidatorsOn(ControllerContext controllerContext, object[] @params)
		{
			if (!@params.Any()) return;
			var metadata = ModelMetadataProviders.Current.GetMetadataForType(() => @params[0], @params[0].GetType());

			var validators = ModelValidatorProviders.Providers.GetValidators(metadata, controllerContext);

			foreach (var error in
								  from p in @params.Where(p => p != null)
								  from validator in validators
								  from error in validator.Validate(p)
								  select error)
			{
                classUnderTest().ModelState.AddModelError(error.MemberName, error.Message);
			}
		}
    

        public void get(string uri, params object[] @params) { FauxHttp("GET", uri, @params); }

        public void copy(string uri, params object[] @params) { FauxHttp("COPY", uri, @params); }

        public void post(string uri, params object[] @params) { FauxHttp("POST", uri, @params); }

        public void put(string uri, params object[] @params) { FauxHttp("PUT", uri, @params); }

        public void delete(string uri, params object[] @params) { FauxHttp("DELETE", uri, @params); }

        public void patch(string uri, params object[] @params) { FauxHttp("PATCH", uri, @params); }




        public JObject conneg_json(string accept = "application/json", ControllerContext controllerContext = null)
		{
            var httpContext = Render(accept, controllerContext);

            return JObject.Parse(httpContext._response.ResponseOutput);
		}

        public HtmlDocument conneg_html(string accept = "text/html", ControllerContext controllerContext = null)
		{
            var httpContext = Render(accept, controllerContext);

			var doc = new HtmlDocument();

            doc.LoadHtml(httpContext._response.ResponseOutput);
			return doc;
		}

        public void markup_is_valid_html5(string accept = "text/html", ControllerContext controllerContext = null)
        {
            var httpContext = Render(accept, controllerContext);
            using (var reader = new StringReader(httpContext._response.ResponseOutput))
            {
                var parser = new SimpleHtmlParser();
                parser.ParseString(reader.ReadToEnd());
            }


        }


        public void markup_is_valid_according_to_dtd(string accept = "text/html", ControllerContext controllerContext = null)
		{

			var items = new List<string>();
			var settings = new XmlReaderSettings();
			settings.ValidationEventHandler += (s, e) => items.Add(e.Message);
			settings.ValidationType = ValidationType.DTD;
			settings.DtdProcessing = DtdProcessing.Parse;
			settings.XmlResolver = new XmlPreloadedResolver();

			var httpContext = Render(accept, controllerContext);

			using (var reader = XmlReader.Create(new StringReader(httpContext._response.ResponseOutput),settings))
			{
				while (reader.Read()) {}
			}

			if(items.Any())
			{
			    throw createException("Markup is invalid\r\n" + items
			                                                                  .Aggregate(new StringBuilder(),
			                                                                             (s, r) =>
			                                                                                 {
			                                                                                     s.Append(r);
			                                                                                     s.Append("\r\n");
			                                                                                     return s;
			                                                                                 },
			                                                                             s => s.ToString()));
			}
                

		}
	
		FakeHttpContext Render(string accept, ControllerContext controllerContext)
		{
			controllerContext = (controllerContext) ?? ControllerContext(accept);

			Result.ExecuteResult(controllerContext);
            controllerContext.HttpContext.Response.Flush();
			controllerContext.HttpContext.Response.OutputStream.Seek(0, SeekOrigin.Begin);
			return (FakeHttpContext) controllerContext.HttpContext;
		}

    	ControllerContext ControllerContext(string accept = "*/*")
    	{
            var httpContext = new FakeHttpContext(new FakeHttpRequest(lasturi)
            {
                _acceptTypes = new[] { accept },
                _cookies = RequestCookies,
                _headers = RequestHeaders
            });
    		return  new ControllerContext(new RequestContext(httpContext, routeData ?? GetRouteData(lasturi)), GetController());
    	}
    }


	


}
