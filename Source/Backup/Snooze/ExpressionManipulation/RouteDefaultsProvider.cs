using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Snooze.Routing;

namespace Snooze.ExpressionManipulation
{
    class RouteDefaultsProvider<T> : ExpressionVisitor
    {
        readonly Dictionary<string, object> _defaults = new Dictionary<string, object>();
        string _recentPropertyName;

        public RouteDefaultsProvider(Expression<Func<T, string>> routeExpression)
        {
            Visit(routeExpression);
        }

        public IDictionary<string, object> Defaults
        {
            get { return _defaults; }
        }

        protected override Expression VisitMemberAccess(MemberExpression m)
        {
            var expr = base.VisitMemberAccess(m);
            _recentPropertyName = m.Member.Name;
            return expr;
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (m.Method.DeclaringType.Equals(typeof(RouteExpressionExtensions)))
            {
                var body = m.Arguments[0];
                var expr = Visit(body);
                if (m.Method.Name == "Default")
                {
                    var value = m.Arguments[1];
                    if (value.NodeType == ExpressionType.Constant)
                    {
                        _defaults.Add(_recentPropertyName, ((ConstantExpression)value).Value);
                    }
                    else
                    {
                        _defaults.Add(_recentPropertyName, Expression.Lambda(value).Compile().DynamicInvoke());
                    }
                }
                return m;
            }
            else if (m.Method.Name == "ToString")
            {
                return base.VisitMethodCall(m);
            }
            throw new InvalidOperationException("Method calls not allow in route expression.");
        }
    }
}
