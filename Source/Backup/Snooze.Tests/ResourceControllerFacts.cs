using System.Web.Mvc;
using Xunit;

namespace Snooze
{
    public class ResourceControllerFacts
    {
        [Fact]
        public void RestController_is_a_Controller()
        {
            Assert.True(typeof(Controller).IsAssignableFrom(typeof(ResourceController)));
        }

        [Fact]
        public void RestController_ActionInvoker_is_a_SnoozeActionInvoker()
        {
            var c = new ResourceController();
            Assert.IsAssignableFrom<ResourceActionInvoker>(c.ActionInvoker);
        }
    }
}
