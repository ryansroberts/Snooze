using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;

namespace Snooze
{

    [Subject(typeof(RequiredInput<>))]
    public class RequiredInputSpec
    {
        public class When_constructing_a_required_input
        {
            static RequiredInput<int> input;
            Establish context = () => input = new RequiredInput<int>();

            It Should_be_invalid = () => input.IsValid.ShouldBeFalse();
        }
    }
}
