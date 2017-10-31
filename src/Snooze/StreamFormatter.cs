﻿using System;
using System.IO;
using System.Web.Mvc;

namespace Snooze
{
	public class StreamFormatter : IResourceFormatter
	{
		public bool CanFormat(ControllerContext context, object resource, string mimeType)
		{
			return (resource != null && resource is Stream);
		}

		public void Output(ControllerContext context, object resource, string contentType)
		{
			context.HttpContext.Response.ContentType = contentType;
			((Stream)resource).CopyTo(context.HttpContext.Response.OutputStream);
			((Stream)resource).Close();
		}

        public int CompareTo(object obj)
        {
            if (obj == null)
                return 1;

            return obj.GetType() == typeof(StreamFormatter) ? 0 : -1;
        }
	}
}