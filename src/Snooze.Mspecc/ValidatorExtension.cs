using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using HtmlAgilityPack;
using Machine.Specifications;
using Snooze.Mspecc.Properties;

namespace Snooze.MSpec
{
	public static class ValidatorExtension
	{
	
		public static void ShouldBeValidAccordingToDTD(this HtmlDocument doc)
		{
			var items = new List<string>();
			var settings = new XmlReaderSettings();
			settings.ValidationEventHandler +=(s, e) => items.Add(e.Message);
			settings.ValidationType = ValidationType.DTD;
			settings.DtdProcessing = DtdProcessing.Parse;
			settings.XmlResolver = new CachedXmlResolver();
			
			using (var reader = XmlReader.Create(new StringReader(doc.DocumentNode.OuterHtml),settings))
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
	}

	public class CachedXmlResolver : XmlUrlResolver
	{
		public override Uri ResolveUri(Uri baseUri, string relativeUri)
		{
			if (relativeUri == "-//W3C//DTD XHTML 1.0 Strict//EN")
				return new Uri("res://xhtml1-strict.dtd/");
			if (relativeUri == "-//W3C XHTML 1.0 Transitional//EN")
				return new Uri("res://xhtml1-transitional.dtd/");
			if (relativeUri == "-//W3C//DTD XHTML 1.0 Transitional//EN")
				return new Uri("res://xhtml1-transitional.dtd/");
			if (relativeUri == "-//W3C XHTML 1.0 Frameset//EN")
				return new Uri("res://xhtml1-frameset.dtd/");
			if (relativeUri == "-//W3C//DTD XHTML 1.1//EN")
				return new Uri("res://xhtml11.dtd/");
			if(relativeUri.StartsWith("res")) return new Uri(relativeUri);

			return base.ResolveUri(baseUri, relativeUri);
		}

		public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn)
		{
			if (absoluteUri.ToString().StartsWith("res://xhtml1-strict.dtd/"))
				return new MemoryStream(Resources.xhtml1_strict.GetBytes());
			if (absoluteUri.ToString().StartsWith("res://xhtml11.dtd/"))
				return new MemoryStream(Resources.xhtml11.GetBytes());
			if (absoluteUri.ToString().StartsWith("res://xhtml1-transitional.dtd/"))
				return new MemoryStream(Resources.xhtml1_transitional.GetBytes());
			if (absoluteUri.ToString().StartsWith("res://xhtml1-frameset.dtd/"))
				return new MemoryStream(Resources.xhtml1_frameset.GetBytes());

			return base.GetEntity(absoluteUri,role,ofObjectToReturn);
		}

	
	}

}