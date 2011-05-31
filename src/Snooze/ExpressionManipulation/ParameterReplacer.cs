#region

using System.Linq.Expressions;

#endregion

namespace Snooze.ExpressionManipulation
{
    internal class ParameterReplacer : ExpressionVisitor
    {
        ParameterExpression _find;
        Expression _replace;

        public Expression Replace(Expression search, ParameterExpression find, Expression replace)
        {
            _find = find;
            _replace = replace;
            return Visit(search);
        }

        protected override Expression VisitParameter(ParameterExpression p)
        {
            if (p == _find) return _replace;
            return base.VisitParameter(p);
        }
    }
}