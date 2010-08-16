﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Snooze
{
    public class FormEncoding
    {
        public enum FormEncodingTypes
        {
            DefaultForm = 0,
            MultipartForm = 1
        }

        public static string GetFormEncodingString(FormEncodingTypes encodingType)
        {
            return encodingType == FormEncodingTypes.MultipartForm ? "multipart/form-data" : "application/x-www-form-urlencoded";
        }
    }
}
