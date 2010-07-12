#region

using System;

#endregion

namespace Snooze.Routing
{
    public static class RouteExpressionExtensions
    {
        public static string CatchAll<T>(this T parameter)
        {
            throw new InvalidOperationException("This is a marker method, do not call.");
        }

        public static string Default<T, V>(this T parameter, V value)
        {
            throw new InvalidOperationException("This is a marker method, do not call.");
        }
    }
}