using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using Snooze.ExpressionManipulation;

namespace Snooze.Routing
{
    class SubResourceRoute<T, P> : ResourceRoute<T>
        where T : SubUrl<P>
        where P : Url
    {
        public SubResourceRoute(Expression<Func<T, string>> expr, ResourceRoute<P> parent)
            : base(ConcatUriExpressions(parent.RouteExpression, expr))
        {

        }

        private static Expression<Func<T, string>> ConcatUriExpressions(Expression<Func<P, string>> parent, Expression<Func<T, string>> child)
        {
            // parent: c => "customer/" + c.CustomerId
            // child : o => "orders/" + c.OrderId
            // +     : o => "customer/" + o.Parent.CustomerId + "/" + "orders/" + o.OrderId
            var replacer = new ParameterReplacer();
            Expression getParent = Expression.MakeMemberAccess(child.Parameters[0], typeof(T).GetProperty("Parent"));
            var p = replacer.Replace(parent.Body, parent.Parameters[0], getParent);

            var concat = typeof(string).GetMethod("Concat", new[] { typeof(object), typeof(object) });
            var addExpr = Expression.Add(
                p,
                Expression.Add(
                    Expression.Constant("/", typeof(string)),
                    child.Body,
                    concat
                ),
                concat
            );
            return Expression.Lambda<Func<T, string>>(addExpr, child.Parameters[0]);
        }
    }
}
