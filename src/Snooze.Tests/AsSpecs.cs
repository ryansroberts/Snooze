using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;

namespace Snooze
{
    public class AsSpecs
    {

        public class mime_lookup
        {
            static string mime;

            Because of = () => { mime = MimeTypes.GetMimeTypeForFilename("somefile.xslx"); };

            It should_return_mime_type =
                () => mime.ShouldEqual("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        }


        [Subject(typeof(ResourceFormatters))]
        public class as_text_with_minimal : with_minimal
        {
            Establish context = () =>
            {
                (new ResourceResult<MyResource>(200, new MyResource())).AsText();
                (new ResourceResult<MyResource>(200, new MyResource())).AsText();
            };

            Because of = () => GetFormatters();

            It should_have_the_required_formatter = () => allFormattersFor.Any(f => f.CompareTo(new StringFormatter()) == 0).ShouldBeTrue();

            It should_only_add_one_specific_formatter = () => specificFormattersFor.Count().ShouldEqual(1);

            It should_add_a_specific_formatter = () => specificFormattersFor.Any(f => f.CompareTo(new StringFormatter()) == 0).ShouldBeTrue();

        }


        [Subject(typeof(ResourceFormatters))]
        public class as_xml_with_minimal : with_minimal
        {
            Establish context = () =>
            {
                (new ResourceResult<MyResource>(200, new MyResource())).AsXml();
                (new ResourceResult<MyResource>(200, new MyResource())).AsXml();
            };

            Because of = () => GetFormatters();

            It should_have_the_required_formatter = () => allFormattersFor.Any(f => f.CompareTo(new XmlFormatter()) == 0).ShouldBeTrue();

            It should_only_add_one_specific_formatter = () => specificFormattersFor.Count().ShouldEqual(1);

            It should_add_a_specific_formatter = () => specificFormattersFor.Any(f => f.CompareTo(new XmlFormatter()) == 0).ShouldBeTrue();

        }

        [Subject(typeof(ResourceFormatters))]
        public class as_json_with_minimal : with_minimal
        {
            Establish context = () =>
            {
                (new ResourceResult<MyResource>(200, new MyResource())).AsJson();
                (new ResourceResult<MyResource>(200, new MyResource())).AsJson();
            };

            Because of = () => GetFormatters();

            It should_have_the_required_formatter = () => allFormattersFor.Any(f => f.CompareTo(new JsonFormatter()) == 0).ShouldBeTrue();

            It should_only_add_one_specific_formatter = () => specificFormattersFor.Count().ShouldEqual(1);

            It should_add_a_specific_formatter = () => specificFormattersFor.Any(f => f.CompareTo(new JsonFormatter()) == 0).ShouldBeTrue();

        }




        [Subject(typeof(ResourceFormatters))]
        public class as_xhtml_with_defaults : with_defaults
        {
            Establish context = () => (new ResourceResult<MyResource>(200, new MyResource())).AsXhtml();

            Because of = () => GetFormatters();

            It should_have_the_required_formatter = () => allFormattersFor.Any(f => f.CompareTo(new ResourceTypeConventionViewFormatter("application/xhtml+xml")) == 0).ShouldBeTrue();

            It should_not_add_any_specific_formatters = () => specificFormattersFor.Count().ShouldEqual(0);

            It should_not_add_a_specific_formatter = () => specificFormattersFor.Any(f => f.CompareTo(new ResourceTypeConventionViewFormatter("application/xhtml+xml")) == 0).ShouldBeFalse();

        }


        [Subject(typeof(ResourceFormatters))]
        public class as_html_with_defaults : with_defaults
        {
            Establish context = () => (new ResourceResult<MyResource>(200, new MyResource())).AsHtml();

            Because of = () => GetFormatters();

            It should_have_the_required_formatter = () => allFormattersFor.Any(f => f.CompareTo(new ResourceTypeConventionViewFormatter("text/html")) == 0).ShouldBeTrue();

            It should_not_add_any_specific_formatters = () => specificFormattersFor.Count().ShouldEqual(0);

            It should_not_add_a_specific_formatter = () => specificFormattersFor.Any(f => f.CompareTo(new ResourceTypeConventionViewFormatter("text/html")) == 0).ShouldBeFalse();

        }

        [Subject(typeof(ResourceFormatters))]
        public class as_text_with_defaults : with_defaults
        {
            Establish context = () => (new ResourceResult<MyResource>(200, new MyResource())).AsText();

            Because of = () => GetFormatters();

            It should_have_the_required_formatter = () => allFormattersFor.Any(f => f.CompareTo(new StringFormatter()) == 0).ShouldBeTrue();

            It should_not_add_any_specific_formatters = () => specificFormattersFor.Count().ShouldEqual(0);

            It should_not_add_a_specific_formatter = () => specificFormattersFor.Any(f => f.CompareTo(new StringFormatter()) == 0).ShouldBeFalse();

        }


        [Subject(typeof(ResourceFormatters))]
        public class as_xml_with_defaults : with_defaults
        {
            Establish context = () => (new ResourceResult<MyResource>(200, new MyResource())).AsXml();

            Because of = () => GetFormatters();

            It should_have_the_required_formatter = () => allFormattersFor.Any(f => f.CompareTo(new XmlFormatter()) == 0).ShouldBeTrue();

            It should_not_add_any_specific_formatters = () => specificFormattersFor.Count().ShouldEqual(0);

            It should_not_add_a_specific_formatter = () => specificFormattersFor.Any(f => f.CompareTo(new XmlFormatter()) == 0).ShouldBeFalse();

        }

        [Subject(typeof(ResourceFormatters))]
        public class as_json_with_defaults : with_defaults
        {
            Establish context = () => (new ResourceResult<MyResource>(200, new MyResource())).AsJson();

            Because of = () => GetFormatters();

            It should_have_the_required_formatter = () => allFormattersFor.Any(f => f.CompareTo(new JsonFormatter()) == 0).ShouldBeTrue();

            It should_not_add_any_specific_formatters = () => specificFormattersFor.Count().ShouldEqual(0);

            It should_not_add_a_specific_formatter = () => specificFormattersFor.Any(f => f.CompareTo(new JsonFormatter()) == 0).ShouldBeFalse();

        }
         


        public class with_defaults
        {
            protected static IEnumerable<IResourceFormatter> allFormattersFor;
            protected static IEnumerable<IResourceFormatter> specificFormattersFor;

            Establish context = () => ResourceFormatters.Defaults();

            protected static void GetFormatters()
            {
                allFormattersFor = ResourceFormatters.FormattersFor(typeof(MyResource));
                specificFormattersFor = ResourceFormatters.ResourceSpecificFormattersFor(typeof(MyResource));
            }

            Cleanup tearDown = () =>
                                   {
                                        ResourceFormatters.ClearSpecificFormatters(typeof(MyResource));
                                        ResourceFormatters.Defaults();
                                   };

        }



        public class with_minimal
        {
            protected static IEnumerable<IResourceFormatter> allFormattersFor;
            protected static IEnumerable<IResourceFormatter> specificFormattersFor;

            Establish context = () =>
                                    {
                                        ResourceFormatters.ClearSpecificFormatters(typeof(MyResource));
                                        ResourceFormatters.Defaults(null);
                                    };

            protected static void GetFormatters()
            {
                allFormattersFor = ResourceFormatters.FormattersFor(typeof (MyResource));
                specificFormattersFor = ResourceFormatters.ResourceSpecificFormattersFor(typeof(MyResource));
            }

            Cleanup tearDown = () =>
            {
                ResourceFormatters.ClearSpecificFormatters(typeof(MyResource));
                ResourceFormatters.Defaults();
            };

        }
        

        public class MyResource
        {
             
        }
    }
}