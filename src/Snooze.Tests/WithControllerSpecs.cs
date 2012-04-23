using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Machine.Specifications;
using Snooze.MSpec;
using Snooze.Routing;

namespace Snooze
{
    public class WithControllerSpecs
    {
        [Subject(typeof(with_controller<ACommand, AHandler>))]
        public class with_a_simple_get_request_with_querystring_and_a_command : with_controller<ACommand, AHandler>
        {
            Establish context = () => RouteTable.Routes.Map<ACommand>(u => "uri");

            Because of = () => get("uri?Property=value",new object[]{ new ACommand { Property = "value2"}});

            It should_be_ok = () => is_200();

            It should_return_a_command = () => result.Resource.ShouldBeOfType<ACommand>();

            It should_map_the_property = () => Resource.Property.ShouldEqual("value2");

            It should_pass_the_command_to_the_filter_context = () => class_under_test.OnActionExecutingCommand.ShouldBeOfType<ACommand>();

            It should_set_the_value_from_the_uri = () => ((ACommand)class_under_test.OnActionExecutingCommand).Property.ShouldEqual("value2");

            Cleanup tear_down = () =>
            {
                ModelBinders.Binders.Clear();
                RouteTable.Routes.Clear();
            };
        }


        [Subject(typeof(with_controller<ACommand, AHandler>))]
        public class with_a_simple_get_request_with_querystring : with_controller<ACommand, AHandler>
        {
            Establish context = () => RouteTable.Routes.Map<ACommand>(u => "uri");

            Because of = () => get("uri?Property=value");

            It should_be_ok = () => is_200();

            It should_return_a_command = () => result.Resource.ShouldBeOfType<ACommand>();

            It should_map_the_property = () => Resource.Property.ShouldEqual("value");

            It should_pass_the_command_to_the_filter_context = () => class_under_test.OnActionExecutingCommand.ShouldBeOfType<ACommand>();

            It should_set_the_value_from_the_uri = () => ((ACommand) class_under_test.OnActionExecutingCommand).Property.ShouldEqual("value");

            Cleanup tear_down = () =>
            {
                ModelBinders.Binders.Clear();
                RouteTable.Routes.Clear();
            };
        }

        [Subject(typeof(with_controller<ACommand,AHandler>))]
        public class with_a_simple_get_request : with_controller<ACommand,AHandler>
        {
            Establish context = () => RouteTable.Routes.Map<ACommand>(u => "uri");

            Because of = () => get("uri");

            It should_be_ok = () => is_200();

            It should_return_a_command = () => result.Resource.ShouldBeOfType<ACommand>();

            It should_pass_the_command_to_the_filter_context =()=> class_under_test.OnActionExecutingCommand.ShouldBeOfType<ACommand>();

            Cleanup tear_down = () =>
            {
                ModelBinders.Binders.Clear();
                RouteTable.Routes.Clear();
            };
        }
        

        public class AHandler : Handler
        {
            public Url OnActionExecutingCommand { get; set; }

            protected override void OnActionExecuting(System.Web.Mvc.ActionExecutingContext filterContext)
            {
                base.OnActionExecuting(filterContext);

                OnActionExecutingCommand = filterContext.ActionParameters.Values.FirstOrDefault(c => c != null && c is Url) as Url;
            }
        
            public ResourceResult Get(ACommand command)
            {
                return OK(command);
            }
        }

        public class ACommand : Url
        {
            public string Property { get; set; }
            
        }
    }
}