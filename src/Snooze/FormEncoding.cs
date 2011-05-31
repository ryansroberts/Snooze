using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Snooze
{
    public enum FormEncodingTypes
    {
        DefaultForm = 0,
        MultipartForm = 1
    }

    public static class FormEncoding
    {
        public static string GetFormEncodingString(FormEncodingTypes encodingType)
        {
            return encodingType == FormEncodingTypes.MultipartForm ? "multipart/form-data" : "application/x-www-form-urlencoded";
        }
    }
}
