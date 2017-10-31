using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Machine.Specifications;

namespace Snooze
{

    public class TestUrl : Url
    {
        public int Param { get; set; }
    }

    public class TestSubUrl : SubUrl<TestUrl>
    {
        public int Param2 { get; set; }
    }

    [Subject("Xml resource serialization")]
    public class XmlResourceSerializationSpecs
    {
        public class When_serializing_a_resource_with_a_url
        {
            static StringBuilder str;
            static XmlSerializer serializer;

            public class TestResource
            {
                public TestUrl Url { get; set; }
            }

            Establish context = () => serializer = new XmlSerializer(typeof (TestResource));

            Because of = () => serializer.Serialize(new StringWriter(str = new StringBuilder()), new TestResource {Url = new TestUrl()});

            It Has_serialized = () => str.ToString().ShouldNotBeEmpty();
        }

        public class When_serializing_a_resource_with_a_sub_url
        {
            static StringBuilder str;
            static XmlSerializer serializer;

            public class TestResource
            {
                public TestSubUrl Url { get; set; }
            }

            Establish context = () => serializer = new XmlSerializer(typeof(TestResource));

            Because of = () => serializer.Serialize(new StringWriter(str = new StringBuilder()), new TestResource { Url = new TestSubUrl{Parent = new TestUrl()} });

            It Has_serialized = () => str.ToString().ShouldNotBeEmpty();
        }

    }
}
