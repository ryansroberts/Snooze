#region

using System.Collections.Generic;

#endregion

namespace Snooze
{
    public interface IInput
    {
        string RawValue { get; set; }
        bool IsValid { get; }
        bool HasValue { get; }
        IEnumerable<string> ErrorMessages { get; }
        void AddErrorMessage(string message);
    }
}