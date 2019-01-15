using System;
using System.Linq.Expressions;
using System.Reflection;

namespace NeinLinq
{
    internal class RewriteQueryCleaner : ExpressionVisitor
    {
        protected override Expression VisitMember(MemberExpression node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            if (node.Expression is ConstantExpression expression)
            {
                var value = GetValue(expression, node.Member);

                while (value is RewriteQueryable query)
                {
                    value = query.Provider.RewriteQuery(query.Expression);
                }

                return Expression.Constant(value, node.Type);
            }

            return base.VisitMember(node);
        }

        private static object GetValue(ConstantExpression target, MemberInfo member)
        {
            if (member is PropertyInfo p)
            {
                return p.GetValue(target.Value, null);
            }

            if (member is FieldInfo f)
            {
                return f.GetValue(target.Value);
            }

            return null;
        }
    }
}