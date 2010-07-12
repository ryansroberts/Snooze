using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace Snooze
{
    class UrlConverter : JavaScriptConverter
    {
        public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
        {
            return new Dictionary<string, object>
                       {
                           {"value", ((Url)obj).ToString()}
                       };
        }

        public override IEnumerable<Type> SupportedTypes
        {
            get { yield return typeof(Url); }
        }
    }
}