#region

using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using Newtonsoft.Json;

#endregion

namespace Snooze
{

    internal class UrlConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(Url).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("value");
            writer.WriteValue((value).ToString());

            writer.WriteEndObject();

            writer.Flush();
        }
    }
}