using Machine.Specifications;
using Snooze.Testing;

namespace Snooze
{
    public class FakeHttpResponseSpecs
    {

        [Subject(typeof(Testing.FakeHttpResponse))]
        public class when_reading_response_output_more_than_once
        {
            static readonly Testing.FakeHttpResponse httpResponse = new FakeHttpResponse();

            Because of = () => httpResponse.Write("test");

            It should_have_response_output_test = () => httpResponse.ResponseOutput.ShouldEqual("test");

            It should_still_have_response_output_test = () => httpResponse.ResponseOutput.ShouldEqual("test");

            It should_really_still_have_response_output_test = () => httpResponse.ResponseOutput.ShouldEqual("test");

        }
    }
}