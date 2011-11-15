using Machine.Specifications;
using SampleApplication.Controllers;
using Snooze.MSpec;
using Snooze.Routing;

namespace Snooze
{
	public class RoutableHandler : Handler
	{
		Register route = r => r.Map<Command>(u => "commandhandler");

		public class Command : Url {}

		public ResourceResult Get(Command cmd) { return OK(cmd); }
	}

	public class handlers_are_routable : with_controller<RoutableHandler.Command, RoutableHandler>
	{
		Because of = () => get("commandhandler");


		It is_routable = is_200;
	}


	public class RenderingViews : with_controller<HomeViewModel,HomeController>
	{
		Establish view_location = () => application_under_test_is_here("../SampleApplication");

		Because of = () => get("");

		It content_negotiates_texthtml = () => conneg_html()
			.DocumentNode
			.ShouldNotBeNull();
	}
}