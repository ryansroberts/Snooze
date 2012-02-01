
using System;
using System.Linq;
using System.Web.Mvc;
using Machine.Specifications;
using Snooze.FutureActionTests;

namespace Snooze
{
    public class ResourceControllerSpecs
    {
        public class RedirectingController : ResourceController
        {
            public ActionResult Get(TestUrl2 url)
            {
                return Redirect(new TestUrl1());
            }

            public ActionResult Get(TestUrl1 url)
            {
                return Redirect("www.example.com");
            }

        }

        public class FoundController: ResourceController
        {
            public ActionResult Get(TestUrl2 url)
            {
                return Found(new TestUrl1());
            }


            public ActionResult Get(TestUrl1 url)
            {
                return Found("www.example.com");
            }

        }

        [Subject(typeof(ResourceController))]
        public class When_redirecting_using_string
        {
            private static RedirectingController controller;
            private static Exception exception;
            Establish context = () => controller = new RedirectingController();

            Because of = () => exception = Catch.Exception(() => controller.Get(new TestUrl1()));

            It throws_an_invalid_operation_exception =() => exception.ShouldBeOfType(typeof (InvalidOperationException));
        }

        [Subject(typeof(ResourceController))]
        public class When_redirecting_using_snooze_url
        {
            private static RedirectingController controller;
            private static Exception exception;
            Establish context = () => controller = new RedirectingController();

            Because of = () => exception = Catch.Exception(() => controller.Get(new TestUrl2()));

            It throws_an_invalid_operation_exception = () => exception.ShouldBeOfType(typeof(InvalidOperationException));
        }




        [Subject(typeof(ResourceController))]
        public class When_FOUND_using_a_snooze_url
        {
            private static FoundController controller;
            private static ResourceResult result;
            Establish context = () => controller = new FoundController();

            Because of = () => result = controller.Get(new TestUrl2()) as ResourceResult;

            It has_a_resource_result = () => result.ShouldNotBeNull();

            It has_a_302_status_code = () => result.StatusCode.ShouldEqual(302);

            It has_a_location_header = () => result.Headers["Location"].ShouldNotBeEmpty();

            It has_the_correct_location_header =
                () => result.Headers["Location"].First().ShouldEqual(new TestUrl1().ToString());
        }


        [Subject(typeof(ResourceController))]
        public class When_FOUND_using_a_string
        {
            private static FoundController controller;
            private static ResourceResult result;
            Establish context = () => controller = new FoundController();

            Because of = () => result = controller.Get(new TestUrl1()) as ResourceResult;

            It has_a_resource_result = () => result.ShouldNotBeNull();

            It has_a_302_status_code = () => result.StatusCode.ShouldEqual(302);

            It has_a_location_header = () => result.Headers["Location"].ShouldNotBeEmpty();

            It has_the_correct_location_header =
                () => result.Headers["Location"].First().ShouldEqual("www.example.com");
        }

    }
}
