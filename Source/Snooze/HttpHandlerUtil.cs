#region

using System;
using System.Web;
using System.Web.UI;

#endregion

namespace Snooze
{
    internal static class HttpHandlerUtil
    {
        // Since Server.Execute() doesn't propagate HttpExceptions where the status code is
        // anything other than 500, we need to wrap these exceptions ourselves.
        public static IHttpHandler WrapForServerExecute(IHttpHandler httpHandler)
        {
            var asyncHandler = httpHandler as IHttpAsyncHandler;
            return (asyncHandler != null)
                       ? new ServerExecuteHttpHandlerAsyncWrapper(asyncHandler)
                       : new ServerExecuteHttpHandlerWrapper(httpHandler);
        }

        // Server.Execute() requires that the provided IHttpHandler subclass Page.

        #region Nested type: ServerExecuteHttpHandlerAsyncWrapper

        sealed class ServerExecuteHttpHandlerAsyncWrapper : ServerExecuteHttpHandlerWrapper, IHttpAsyncHandler
        {
            readonly IHttpAsyncHandler _httpHandler;

            public ServerExecuteHttpHandlerAsyncWrapper(IHttpAsyncHandler httpHandler)
                : base(httpHandler)
            {
                _httpHandler = httpHandler;
            }

            #region IHttpAsyncHandler Members

            public IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback cb, object extraData)
            {
                return Wrap(() => _httpHandler.BeginProcessRequest(context, cb, extraData));
            }

            public void EndProcessRequest(IAsyncResult result)
            {
                Wrap(() => _httpHandler.EndProcessRequest(result));
            }

            #endregion
        }

        #endregion

        #region Nested type: ServerExecuteHttpHandlerWrapper

        internal class ServerExecuteHttpHandlerWrapper : Page
        {
            readonly IHttpHandler _httpHandler;

            public ServerExecuteHttpHandlerWrapper(IHttpHandler httpHandler)
            {
                _httpHandler = httpHandler;
            }

            internal IHttpHandler InnerHandler
            {
                get { return _httpHandler; }
            }

            public override void ProcessRequest(HttpContext context)
            {
                Wrap(() => _httpHandler.ProcessRequest(context));
            }

            protected static void Wrap(Action action)
            {
                Wrap(delegate
                    {
                        action();
                        return (object) null;
                    });
            }

            protected static TResult Wrap<TResult>(Func<TResult> func)
            {
                try
                {
                    return func();
                }
                catch (HttpException he)
                {
                    if (he.GetHttpCode() == 500)
                    {
                        throw; // doesn't need to be wrapped
                    }
                    throw new HttpException(500,
                                            "An exception occured on the execution of a partial request, see inner exception for details",
                                            he);
                }
            }
        }

        #endregion
    }
}