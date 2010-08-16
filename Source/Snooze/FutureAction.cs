#region

using System;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

#endregion

namespace Snooze
{
    [Serializable]
    public class FutureAction : IXmlSerializable
    {
        //Default constructor to allow xml serialization
        public FutureAction()
        { }

        public FutureAction(string method, Url url, object entity)
        {
            Method = method;
            Url = url;
            Entity = entity;
        }

        public FutureAction(string method, Url url, object entity, bool expectsMultipartEncoding) 
            : this (method, url, entity)
        {
            _expectsMultipartEncoding = expectsMultipartEncoding;
        }

        public FutureAction(Expression<Func<object>> actionMethod)
            : this(actionMethod.Body as MethodCallExpression)
        {
        }

        public FutureAction(Expression<Func<object>> actionMethod, bool expectsMultipartEncoding)
            : this(actionMethod.Body as MethodCallExpression, expectsMultipartEncoding)
        {
        }

        public FutureAction(Expression<Action> actionMethod)
            : this(actionMethod.Body as MethodCallExpression)
        {
        }

        public FutureAction(Expression<Action> actionMethod, bool expectsMultipartEncoding)
            : this(actionMethod.Body as MethodCallExpression, expectsMultipartEncoding)
        {
        }

        protected FutureAction(MethodCallExpression methodCall, bool expectsMultipartEncoding)
            : this(methodCall)
        {
            _expectsMultipartEncoding = expectsMultipartEncoding;
        }

        protected FutureAction(MethodCallExpression methodCall)
        {
            Method = methodCall.Method.Name.ToLowerInvariant();
            Url = (Url) Expression.Lambda(methodCall.Arguments[0]).Compile().DynamicInvoke();
            if (methodCall.Arguments.Count > 1)
            {
                if (methodCall.Arguments[1].NodeType == ExpressionType.Parameter)
                {
                    Entity = Activator.CreateInstance(methodCall.Arguments[1].Type);
                }
                else
                {
                    Entity = Expression.Lambda(methodCall.Arguments[1]).Compile().DynamicInvoke();
                }
            }
        }

        private readonly bool _expectsMultipartEncoding;

        public string Method { get; set; }
        public Url Url { get; set; }
        public object Entity { get; set; }

        public string FormEncoding
        {
            get 
            {
                return _expectsMultipartEncoding ? "multipart/form-data" : "application/x-www-form-urlencoded";
            }
        }

        #region Implementation of IXmlSerializable

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            throw new NotImplementedException();
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteElementString("Method", Method);
            writer.WriteElementString("Url", Url.ToString());
            writer.WriteElementString("Entity", Entity.ToString());
            writer.WriteElementString("FormEncoding", FormEncoding);
        }

        #endregion
    }

    public class FutureAction<T> : FutureAction
    {
        //Default constructor to allow xml serialization
        public FutureAction()
        { }

        public FutureAction(string method, Url url, T entity) : base(method, url, entity)
        {
        }

        public FutureAction(string method, Url url, T entity, bool expectsMultipartEncoding)
            : base(method, url, entity, expectsMultipartEncoding)
        {
        }

        public FutureAction(Expression<Func<T, object>> actionMethod)
            : base(actionMethod.Body as MethodCallExpression)
        {
        }

        public FutureAction(Expression<Func<T, object>> actionMethod, bool expectsMultipartEncoding)
            : base(actionMethod.Body as MethodCallExpression, expectsMultipartEncoding)
        {
        }

        public FutureAction(Expression<Func<ActionResult>> actionMethod)
            : base(actionMethod.Body as MethodCallExpression)
        {
        }

        public FutureAction(Expression<Func<ActionResult>> actionMethod, bool expectsMultipartEncoding)
            : base(actionMethod.Body as MethodCallExpression, expectsMultipartEncoding)
        {
        }


        public FutureAction(Expression<Action<T>> actionMethod)
            : base(actionMethod.Body as MethodCallExpression)
        {
        }

        public FutureAction(Expression<Action<T>> actionMethod, bool expectsMultipartEncoding)
            : base(actionMethod.Body as MethodCallExpression, expectsMultipartEncoding)
        {
        }

        public new T Entity
        {
            get { return (T) base.Entity; }
            set { base.Entity = value; }
        }
    }
}