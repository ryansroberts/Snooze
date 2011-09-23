#region

using System;
using System.Collections.Generic;
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


    public static class ResourceFormatters
    {
        static readonly IList<IResourceFormatter> defaultViewFormatters = new List<IResourceFormatter>();

        static readonly IList<IResourceFormatter> defaultSerialisationFormatters = new List<IResourceFormatter>();
        static readonly IDictionary<Type,IResourceFormatter> resourceSpecificFormatters = new Dictionary<Type, IResourceFormatter>();

		public static void Defaults(params Type[] types)
		{
			defaultSerialisationFormatters.Clear();
			foreach (var type in types)
			{
				if (!typeof(IResourceFormatter).IsAssignableFrom(type))
					throw new InvalidOperationException("Type " + type + " is not an IResourceFormatter");

				defaultSerialisationFormatters.Add((IResourceFormatter) Activator.CreateInstance(type,new object[]{}));
			}
		}

        static ResourceFormatters()
        {
            // The order of formatters matters.
            // Browsers like Chrome will ask for "text/xml, application/xhtml+xml, ..."
            // But we don't want to use the XML formatter by default 
            // - which would happen since "text/xml" appears first in the list.
            // So we add an explicitly typed ViewFormatter first.
            defaultViewFormatters.Add(new ResourceTypeConventionViewFormatter("text/html"));
            defaultViewFormatters.Add(new ResourceTypeConventionViewFormatter("application/xhtml+xml"));
            defaultViewFormatters.Add(new ResourceTypeConventionViewFormatter("text/xml"));
            defaultViewFormatters.Add(new ResourceTypeConventionViewFormatter("application/rss+xml"));
            defaultViewFormatters.Add(new ResourceTypeConventionViewFormatter("*/*")); // similar reason for this.

			Defaults(typeof(JsonFormatter), typeof(XmlFormatter), typeof(StreamFormatter),typeof(ByteArrayFormatter),typeof(StringFormatter));
        }

        public static IEnumerable<IResourceFormatter> FormattersFor(Type resourceType)
        {
            return Enumerable.Concat(
                              defaultViewFormatters,
                        Enumerable.Concat(ResourceSpecificFormattersFor(resourceType),
                              defaultSerialisationFormatters));
        }

        private static IEnumerable<IResourceFormatter> ResourceSpecificFormattersFor(Type resourceType)
        {
            return resourceSpecificFormatters.ContainsKey(resourceType)
                       ? new[] {resourceSpecificFormatters[resourceType]}
                       : new IResourceFormatter[] {};
        }

        public static void AddViewFormatter(Type type, string mediatype, string viewname)
        {
            resourceSpecificFormatters[type] = new ExplicitNameViewFormatter(mediatype, viewname);
        }
    }
}