namespace Snooze
{
    public static class UrlExtensions
    {
        public static T Concat<P, T>(this P parent, T child)
            where T : SubUrl<P>
            where P : Url
        {
            child.Parent = parent;
            return child;
        }
    }
}