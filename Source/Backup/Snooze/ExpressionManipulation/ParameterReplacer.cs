using System.Linq.Expressions;

namespace Snooze.ExpressionManipulation
{
    class ParameterReplacer : ExpressionVisitor
    {
        public Expression Replace(Expression search, ParameterExpression find, Expression replace)
        {
            _find = find;
            _replace = replace;
            return Visit(search);
        }

        ParameterExpression _find;
        Expression _replace;

        protected override Expression VisitParameter(ParameterExpression p)
        {
            if (p == _find) return _replace;
            return base.VisitParameter(p);
        }
    }
}
