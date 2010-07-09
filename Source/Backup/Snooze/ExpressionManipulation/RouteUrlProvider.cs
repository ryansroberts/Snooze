using System;
using System.Linq.Expressions;
using Snooze.Routing;

namespace Snooze.ExpressionManipulation
{
    class RouteUrlProvider<T> : ExpressionVisitor
    {
        bool _hasCatchAll;
        readonly string _url;

        public RouteUrlProvider(Expression<Func<T, string>> expr)
        {
            _url = Expression.Lambda<Func<string>>(Visit(expr.Body)).Compile().Invoke();
        }

        public string Url { get { return _url; } }
        
        protected override Expression VisitUnary(UnaryExpression u)
        {
            var member = u.Operand as MemberExpression;
            if (member != null)
            {
                return Expression.Constant("{" + member.Member.Name + "}", typeof(string));
            }
            return base.VisitUnary(u);
        }

        protected override Expression VisitMemberAccess(MemberExpression member)
        {
            if (member != null && typeof(Url).IsAssignableFrom(member.Member.DeclaringType))
            {
                return Expression.Constant("{" + (_hasCatchAll ? "*" : "") + member.Member.Name + "}", typeof(string));
            }
            return base.VisitMemberAccess(member);
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (m.Method.DeclaringType.Equals(typeof(RouteExpressionExtensions)))
            {
                var body = m.Arguments[0];
                if (m.Method.Name == "CatchAll")
                {
                    _hasCatchAll = true;
                }
                return Visit(body);
            }
            else if (m.Method.Name == "ToString")
            {
                return Visit(m.Object);
            }
            throw new InvalidOperationException("Method calls not allow in route expression.");
        }
    }
}
