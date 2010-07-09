using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Snooze
{
    public class RequiredInputFacts
    {
        [Fact]
        public void New_RequiredInput_is_invalid()
        {
            Assert.False(new RequiredInput<int>().IsValid);
        }
    }
}
