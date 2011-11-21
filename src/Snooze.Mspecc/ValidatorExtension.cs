using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using HtmlAgilityPack;
using Machine.Specifications;

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
				return new Uri("http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd");
			else if (relativeUri == "-//W3C XHTML 1.0 Transitional//EN")
				return new Uri("http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd");
			else if (relativeUri == "-//W3C//DTD XHTML 1.0 Transitional//EN")
				return new Uri("http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd");
			else if (relativeUri == "-//W3C XHTML 1.0 Frameset//EN")
				return new Uri("http://www.w3.org/TR/xhtml1/DTD/xhtml1-frameset.dtd");
			else if (relativeUri == "-//W3C//DTD XHTML 1.1//EN")
				return new Uri("http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd");
			return base.ResolveUri(baseUri, relativeUri);
		}

		public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn)
		{
			if (!cache.ContainsKey(absoluteUri))
				GetNewStream(absoluteUri, role, ofObjectToReturn);
			return new FileStream(cache[absoluteUri], FileMode.Open, FileAccess.Read, FileShare.Read);
		}

		private void GetNewStream(Uri absoluteUri, string role, Type ofObjectToReturn)
		{
			using (Stream stream = (Stream)base.GetEntity(absoluteUri, role, ofObjectToReturn))
			{
				String filename = System.IO.Path.GetTempFileName();
				using (FileStream ms = new FileStream(filename, FileMode.Create, FileAccess.Write))
				{
					Byte[] buffer = new byte[8192];
					Int32 count = 0;
					while ((count = stream.Read(buffer, 0, buffer.Length)) > 0)
					{
						ms.Write(buffer, 0, count);
					}
					ms.Flush();
					cache.Add(absoluteUri, filename);
				}
			}
		}

		public static Dictionary<Uri, String> cache = new Dictionary<Uri, String>();

	}

}