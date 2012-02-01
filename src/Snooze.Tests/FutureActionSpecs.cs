using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Xml.Serialization;
using Machine.Specifications;

namespace Snooze.FutureActionTests
{

    public class TestUrl1 : Url { }
    public class TestUrl2 : Url { }

    public class Test1ViewModel
    {
        public string AnyOldString { get; set; }
        public FutureAction ActionInSameController { get; set; }
        public FutureAction<TestController2> ActionInDifferentController { get; set; }
    }

    public class Test2ViewModel
    {
        public string AnyOldString { get; set; }
        public FutureAction ActionInSameController { get; set; }
    }

    public class TestController1 : ResourceController
    {
        public ResourceResult Get(TestUrl1 url)
        {
            return OK(new Test1ViewModel());
        }

        public ResourceResult Post(TestUrl1 url, Test1ViewModel postedViewModel)
        {
            return SeeOther("www.example.com");
        }
    }

    public class TestController2 : ResourceController
    {
        public ResourceResult Get(TestUrl2 url)
        {
            return OK(new Test2ViewModel());
        }

    }




    [Subject("FutureAction")]
    public class FutureActionSpecs
    {
        public class When_creating_a_future_action_that_doesnot_expect_multipart_content
        {
            private static FutureAction<TestController2> futureAction;

            Establish context = () => futureAction = new FutureAction<TestController2>(c => c.Get(new TestUrl2()));

            It Has_default_form_encoding = () => futureAction.FormEncodingString.ShouldEqual("application/x-www-form-urlencoded");
        }


        public class When_creating_a_future_action_that_expects_multipart_content
        {
            private static FutureAction<TestController1> futureAction;

            private Establish context =
                () => futureAction = new FutureAction<TestController1>(c => c.Post(new TestUrl1(), new Test1ViewModel()), FormEncodingTypes.MultipartForm);

            It Has_multipart_form_encoding = () => futureAction.FormEncodingString.ShouldEqual("multipart/form-data");
        }


        public class When_serializing_a_future_action
        {
            static StringBuilder str;
            static XmlSerializer serializer;
            static TestResource testResource;

            public class TestResource
            {
                public FutureAction TestFutureAction { get; set; }
            }

            Establish context = () =>
            {
                serializer = new XmlSerializer(typeof(TestResource));
                testResource = new TestResource { TestFutureAction = new FutureAction("post", new TestUrl1(), new TestController1()) };
            };

            Because of = () => serializer.Serialize(new StringWriter(str = new StringBuilder()), testResource);

            It Has_serialized = () => str.ToString().ShouldNotBeEmpty();
        }

        public class When_serializing_a_typed_future_action
        {
            static StringBuilder str;
            static XmlSerializer serializer;
            static TestResource testResource;

            public class TestResource
            {
                public FutureAction<TestController1> TestFutureAction { get; set; }
            }

            Establish context = () =>
            {
                serializer = new XmlSerializer(typeof(TestResource));
                testResource = new TestResource { TestFutureAction = new FutureAction<TestController1>("post", new TestUrl1(), new TestController1()) };
            };

            Because of = () => serializer.Serialize(new StringWriter(str = new StringBuilder()), testResource);

            It Has_serialized = () => str.ToString().ShouldNotBeEmpty();
        }

    }
}
