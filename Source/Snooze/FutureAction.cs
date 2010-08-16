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

        public FutureAction(string method, Url url, object entity, FormEncodingTypes formEncodingType) 
            : this (method, url, entity)
        {
            FormEncodingType = formEncodingType;
        }

        public FutureAction(Expression<Func<object>> actionMethod)
            : this(actionMethod.Body as MethodCallExpression)
        {
        }

        public FutureAction(Expression<Func<object>> actionMethod, FormEncodingTypes formEncodingType)
            : this(actionMethod.Body as MethodCallExpression, formEncodingType)
        {
        }

        public FutureAction(Expression<Action> actionMethod)
            : this(actionMethod.Body as MethodCallExpression)
        {
        }

        public FutureAction(Expression<Action> actionMethod, FormEncodingTypes formEncodingType)
            : this(actionMethod.Body as MethodCallExpression, formEncodingType)
        {
        }

        protected FutureAction(MethodCallExpression methodCall, FormEncodingTypes formEncodingType)
            : this(methodCall)
        {
            FormEncodingType = formEncodingType;
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

        public string Method { get; set; }
        public Url Url { get; set; }
        public object Entity { get; set; }
        public FormEncodingTypes FormEncodingType { get; set; }

        public string FormEncodingString
        {
            get 
            {
                return FormEncoding.GetFormEncodingString(this.FormEncodingType);
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
            writer.WriteElementString("FormEncodingType", FormEncodingType.ToString());
            writer.WriteElementString("FormEncodingString", FormEncodingString);
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

        public FutureAction(string method, Url url, T entity, FormEncodingTypes formEncodingType)
            : base(method, url, entity, formEncodingType)
        {
        }

        public FutureAction(Expression<Func<T, object>> actionMethod)
            : base(actionMethod.Body as MethodCallExpression)
        {
        }

        public FutureAction(Expression<Func<T, object>> actionMethod, FormEncodingTypes formEncodingType)
            : base(actionMethod.Body as MethodCallExpression, formEncodingType)
        {
        }

        public FutureAction(Expression<Func<ActionResult>> actionMethod)
            : base(actionMethod.Body as MethodCallExpression)
        {
        }

        public FutureAction(Expression<Func<ActionResult>> actionMethod, FormEncodingTypes formEncodingType)
            : base(actionMethod.Body as MethodCallExpression, formEncodingType)
        {
        }


        public FutureAction(Expression<Action<T>> actionMethod)
            : base(actionMethod.Body as MethodCallExpression)
        {
        }

        public FutureAction(Expression<Action<T>> actionMethod, FormEncodingTypes formEncodingType)
            : base(actionMethod.Body as MethodCallExpression, formEncodingType)
        {
        }

        public new T Entity
        {
            get { return (T) base.Entity; }
            set { base.Entity = value; }
        }
    }
}