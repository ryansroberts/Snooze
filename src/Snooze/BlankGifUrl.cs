#region

using System.Reflection;

#endregion

namespace Snooze
{
    public class BlankGifUrl : Url
    {
        public BlankGifUrl()
        {
            SnoozeVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString(4);
        }

        public string SnoozeVersion { get; set; }
    }
}