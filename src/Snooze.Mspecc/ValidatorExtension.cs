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
}