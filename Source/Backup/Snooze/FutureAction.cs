using System;
using System.Linq.Expressions;
using System.Web.Mvc;

namespace Snooze
{
    public class FutureAction
    {
        public FutureAction(string method, Url url, object entity)
        {
            Method = method;
            Url = url;
            Entity = entity;
        }

        public FutureAction(Expression<Func<object>> actionMethod)
            : this(actionMethod.Body as MethodCallExpression)
        {
        }

        public FutureAction(Expression<Action> actionMethod)
            : this(actionMethod.Body as MethodCallExpression)
        {
        }

        protected FutureAction(MethodCallExpression methodCall)
        {
            Method = methodCall.Method.Name.ToLowerInvariant();
            Url = (Url)Expression.Lambda(methodCall.Arguments[0]).Compile().DynamicInvoke();
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

    }

    public class FutureAction<T> : FutureAction
    {
        public FutureAction(string method, Url url, T entity) : base(method, url, entity)
        {

        }

        public FutureAction(Expression<Func<T, object>> actionMethod)
            : base(actionMethod.Body as MethodCallExpression)
        {
        }

        public FutureAction(Expression<Func<ActionResult>> actionMethod)
            : base(actionMethod.Body as MethodCallExpression)
        {
        }

        public FutureAction(Expression<Action<T>> actionMethod)
            : base(actionMethod.Body as MethodCallExpression)
        {
        }

        public new T Entity
        {
            get { return (T)base.Entity; }
            set { base.Entity = value; }
        }
    }
}
