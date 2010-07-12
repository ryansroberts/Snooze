using System.Reflection;

namespace Snooze
{
    public class IEPngFixUrl : Url
    {
        public IEPngFixUrl()
        {
            SnoozeVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString(4);
        }

        public string SnoozeVersion { get; set; } 
    }
}