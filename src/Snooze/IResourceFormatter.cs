#region

using System;
using System.Web.Mvc;

#endregion

namespace Snooze
{
    public interface IResourceFormatter : IComparable
    {
        bool CanFormat(ControllerContext context, object resource, string mimeType);
        void Output(ControllerContext context, object resource, string contentType);
    }
}