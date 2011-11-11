using Machine.Specifications;
using SampleApplication.Controllers;
using Snooze.MSpec;

namespace Snooze
{
	public class RenderingViews : with_controller<HomeViewModel,HomeController>
	{
		Establish view_location = () => application_under_test_is_here("../SampleApplication");

		Because of = () => get("");

		It content_negotiates_texthtml = () => conneg_texthtml()
			.DocumentNode.ShouldNotBeNull();
	}
}