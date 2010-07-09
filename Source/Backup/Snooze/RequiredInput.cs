using System;
using System.Collections.Generic;
using System.Linq;

namespace Snooze
{
    public class RequiredInput<T> : Input<T>
    {
        public override bool IsValid
        {
            get
            {
                return HasValue && base.IsValid;
            }
            protected set
            {
                base.IsValid = value;
            }
        }

        public override IEnumerable<string> ErrorMessages
        {
            get
            {
                if (!HasValue)
                {
                    return Enumerable.Concat(new[] { "Required." }, base.ErrorMessages);
                }
                else
                {
                    return base.ErrorMessages;
                }
            }
        }
    }
}
