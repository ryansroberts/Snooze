using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using Fizzler.Systems.HtmlAgilityPack;
using Machine.Specifications;
using SampleApplication.Controllers;
using Snooze.MSpec;
using Snooze.Routing;

namespace Snooze
{



	public class modestatething  : Handler
	{
		public class Command : Url
		{
			readonly List<string> strings = new List<string>();

   			public string  stupid { get; set; }

			[Required]
			public string requiredButIsnt { get; set; }

			public IEnumerable<string> GetStrings() { return strings; }

			public void Add(string error)
			{
				strings.Add(error);
			}
		}

		static Register route = r => r.Map<Command>(u => "modestatething/" + u.stupid);

		protected override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			Command cmd = filterContext.ActionParameters.First().Value as Command;

			foreach (var error in ModelState["requiredButIsnt"].Errors)
				cmd.Add(error.ErrorMessage);

			base.OnActionExecuting(filterContext);
		}

		public ResourceResult Get(Command cmd)
		{

			return OK(cmd);
		}
	}

	public class modelstate_oddness : with_controller<modestatething.Command,modestatething>
	{
		Establish binder = () => { ModelValidatorProviders.Providers.Add(new DataAnnotationsModelValidatorProvider()); };

		Because of = () => get("modestatething/1", new modestatething.Command()
		{
		});

		It has_one_modelerrors = () => Resource.GetStrings().Count().ShouldEqual(1);
	}

	public class modelstate_oddness2 : with_controller<modestatething.Command, modestatething>
	{
		Because of = () => get("modestatething/1", new modestatething.Command());

		It has_one_modelerrors = () => Resource.GetStrings().Count().ShouldEqual(1);
	}

	public class RoutableHandler : Handler
	{
		static Register route = r => r.Map<Command>(u => "commandhandler/" + u.stupid);

		public class StupidType
		{
			public string Value { get; set; }
		}

		public class Command : Url
		{
			public StupidType stupid { get; set; }
		}

		public ResourceResult Get(Command cmd) { return OK(cmd); }
	}

	public class handlers_are_routable : with_controller<RoutableHandler.Command, RoutableHandler>
	{

		Because of = () => get("commandhandler/stupid");

		It is_routable = is_200;

	    It method_is_set = () => class_under_test.HttpVerb.ShouldEqual(HttpVerbs.Get);
	}

	public class handlers_preserve_command_identity : with_controller<RoutableHandler.Command, RoutableHandler>
	{
		static RoutableHandler.Command cmd;

		Because of = () => get("commandhandler/stupid", cmd = new RoutableHandler.Command());

		It is_routable = is_200;

		It command_is_passed_object = () => cmd.ShouldBeTheSameAs(Resource);
	}

    
    public class access_viewbag : with_controller<HomeViewModel, HomeController>
    {
        Establish view_location = () =>
        {
            application_under_test_is_here("../SampleApplication");
            class_under_test.ViewBag.Stuff = "stuff";
        };

        Because of = () => get("");

        It content_negotiates_texthtml = () => conneg_html()
                .DocumentNode.InnerText.ShouldContain("HELLO");

        private It has_viewbag_content =
            () => conneg_html().DocumentNode.QuerySelectorAll("html").First().InnerText.ShouldContain("stuff");

        It has_no_parse_errors = () => markup_is_valid_according_to_dtd();
    }

	public class content_negotiate_texthtml : with_controller<HomeViewModel,HomeController>
	{
		Establish view_location = () => application_under_test_is_here("../SampleApplication");

		Because of = () => get("");

		It content_negotiates_texthtml = () => conneg_html()
				.DocumentNode.InnerText.ShouldContain("HELLO");

		It has_html_element = () => conneg_html().DocumentNode.QuerySelectorAll("html").ShouldNotBeEmpty();

		It has_no_parse_errors = () => markup_is_valid_according_to_dtd();
	}


	public class content_negotiate_json : with_controller<HomeViewModel, HomeController>
	{
		Establish view_location = () => application_under_test_is_here("../SampleApplication");

		Because of = () => get("");

		It content_negotiates_json = () => conneg_json().ShouldNotBeEmpty();
	}
}