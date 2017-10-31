#region

using System;

#endregion

namespace Snooze
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    internal class SubUrlAttribute : Attribute
    {
    }
}