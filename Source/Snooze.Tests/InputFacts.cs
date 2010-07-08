using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Snooze
{
    public class InputFacts
    {
        [Fact]
        public void New_Input_is_valid()
        {
            Assert.True(new Input<int>().IsValid);
        }

        [Fact]
        public void Assigning_foo_string_to_int_Input_makes_it_invalid()
        {
            var intInput = new Input<int>();
            intInput.RawValue = "foo";
            Assert.False(intInput.IsValid);
            Assert.Equal("Invalid data.", intInput.ErrorMessages.First());
        }

        [Fact]
        public void Assigning_42_string_to_int_Input_means_Value_is_42()
        {
            var input = new Input<int>();
            input.RawValue = "42";
            Assert.Equal(42, input.Value);
        }

        [Fact]
        public void Can_implicitly_cast_42_to_int_Input()
        {
            Input<int> input = 42;
            Assert.Equal(42, input.Value);
        }
    }
}
