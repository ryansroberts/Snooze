#region

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

#endregion

namespace Snooze
{

    public class AdditionalFormatter
    {
        public static FormatterAdder<T> For<T>(string mediatype)
        {
            return new FormatterAdder<T>(mediatype);

        }
    }

    public class FormatterAdder<T>
    {
        private readonly string mediatype;

        public FormatterAdder(string mediatype)
        {
            this.mediatype = mediatype;
        }

        public void FormattedBy(string viewname)
        {
            ResourceFormatters.AddViewFormatter(typeof (T), mediatype, viewname);
        }
    }

    public static class ResourceFormatterDefaults
    {
        public static string ViewFormatter { get; set; }

        static ResourceFormatterDefaults()
        {
           ViewFormatter = ConfigurationManager.AppSettings["snooze:viewformatter"] ?? "html";
        }


        public static IEnumerable<IResourceFormatter> GetViewFormatters()
        {
            switch (ViewFormatter)
            {
                case "legacy":
                    yield return new ResourceTypeConventionViewFormatter("text/html");
                    yield return new ResourceTypeConventionViewFormatter("application/xhtml+xml");
                    yield return new ResourceTypeConventionViewFormatter("text/xml");
                    yield return new ResourceTypeConventionViewFormatter("application/rss+xml");
                    yield return new ResourceTypeConventionViewFormatter("*/*");
                    yield break;
                case "xhtml":
                    yield return new ResourceTypeConventionViewFormatter("application/xhtml+xml", "text/html", "*/*");
                    yield return new ResourceTypeConventionViewFormatter("text/xml");
                    yield return new ResourceTypeConventionViewFormatter("application/rss+xml");
                    yield break;
                default:
                    yield return new ResourceTypeConventionViewFormatter("text/html", "application/xhtml+xml", "*/*");
                    yield return new ResourceTypeConventionViewFormatter("text/xml");
                    yield return new ResourceTypeConventionViewFormatter("application/rss+xml");
                    yield break;
            }
        }
        
    }

    public static class ResourceFormatters
    {
        static readonly IList<IResourceFormatter> defaultViewFormatters = new List<IResourceFormatter>();
        static readonly IList<IResourceFormatter> defaultSerialisationFormatters = new List<IResourceFormatter>();
        static readonly ConcurrentDictionary<Type, IList<IResourceFormatter>> resourceSpecificFormatters = new ConcurrentDictionary<Type, IList<IResourceFormatter>>();


        public static void Defaults()
        {
            Defaults(typeof(JsonFormatter), typeof(XmlFormatter), typeof(StreamFormatter), typeof(ByteArrayFormatter), typeof(StringFormatter));
        }

		public static void Defaults(params Type[] types)
		{
            defaultViewFormatters.Clear();
            // The order of formatters matters.
            // Browsers like Chrome will ask for "text/xml, application/xhtml+xml, ..."
            // But we don't want to use the XML formatter by default 
            // - which would happen since "text/xml" appears first in the list.
            // So we add an explicitly typed ViewFormatter first.
            foreach (var resourceSpecificFormatter in ResourceFormatterDefaults.GetViewFormatters())
            {
                defaultViewFormatters.Add(resourceSpecificFormatter);
            }

			defaultSerialisationFormatters.Clear();
            if (types == null || types.Length == 0)
                return;

			foreach (var type in types)
			{
				if (!typeof(IResourceFormatter).IsAssignableFrom(type))
					throw new InvalidOperationException("Type " + type + " is not an IResourceFormatter");

				defaultSerialisationFormatters.Add((IResourceFormatter) Activator.CreateInstance(type,new object[]{}));
			}
		}

        static ResourceFormatters()
        {
			Defaults();
        }

        public static IEnumerable<IResourceFormatter> FormattersFor(Type resourceType)
        {
            return Enumerable.Concat(defaultViewFormatters, Enumerable.Concat(ResourceSpecificFormattersFor(resourceType), defaultSerialisationFormatters));
        }

        public static IEnumerable<IResourceFormatter> ResourceSpecificFormattersFor(Type resourceType)
        {
            return resourceSpecificFormatters.ContainsKey(resourceType)
                       ? resourceSpecificFormatters[resourceType]
                       : new IResourceFormatter[] {};
        }

        public static void AddViewFormatter(Type type, string mediatype, string viewname)
        {
            AddResourceFormatter(type, new ExplicitNameViewFormatter(mediatype, viewname));
        }

        public static void ClearSpecificFormatters(Type resource)
        {
            IList<IResourceFormatter> formatters;
            resourceSpecificFormatters.TryRemove(resource, out formatters);
        }

        public static void AddResourceFormatter(Type resource, IResourceFormatter formatter)
        {
            if(formatter==null)
                return;

            IList<IResourceFormatter> list;
            if (resourceSpecificFormatters.TryGetValue(resource, out list))
            {
                if (list.Any(f => f.CompareTo(formatter) == 0))
                    return;
            }


            if(defaultSerialisationFormatters.Any(f=>f.CompareTo(formatter)==0) || defaultViewFormatters.Any(f=>f.CompareTo(formatter)==0))
                return;

            
            if (list!=null)
            {
                list.Add(formatter);
                return;
            }

            resourceSpecificFormatters.TryAdd(resource, new List<IResourceFormatter>{ formatter });

        }
    }
}