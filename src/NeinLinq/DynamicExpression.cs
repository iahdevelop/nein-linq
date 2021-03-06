using System;
using System.Linq;
using System.Linq.Expressions;

namespace NeinLinq
{
    /// <summary>
    /// Helps building dynamic expressions.
    /// </summary>
    public static class DynamicExpression
    {
        private static readonly ObjectCache<Type, Func<string, IFormatProvider, object>> cache = new ObjectCache<Type, Func<string, IFormatProvider, object>>();

        /// <summary>
        /// Create a dynamic comparison expression for a given property selector, comparison method and reference value.
        /// </summary>
        /// <param name="target">The parameter of the query data.</param>
        /// <param name="selector">The property selector to parse.</param>
        /// <param name="comparer">The comparison method to use.</param>
        /// <param name="value">The reference value to compare with.</param>
        /// <param name="provider">The culture-specific formatting information.</param>
        /// <returns>The dynamic comparison expression.</returns>
        public static Expression CreateComparison(ParameterExpression target, string selector, DynamicCompare comparer, string value, IFormatProvider provider = null)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            if (string.IsNullOrEmpty(selector))
                throw new ArgumentNullException(nameof(selector));
            if (!Enum.IsDefined(typeof(DynamicCompare), comparer))
                throw new ArgumentOutOfRangeException(nameof(comparer));

            var memberAccess = CreateMemberAccess(target, selector);
            var actualValue = CreateConstant(target, memberAccess, value, provider);

            switch (comparer)
            {
                case DynamicCompare.Equal:
                    return Expression.Equal(memberAccess, actualValue);

                case DynamicCompare.NotEqual:
                    return Expression.NotEqual(memberAccess, actualValue);

                case DynamicCompare.GreaterThan:
                    return Expression.GreaterThan(memberAccess, actualValue);

                case DynamicCompare.GreaterThanOrEqual:
                    return Expression.GreaterThanOrEqual(memberAccess, actualValue);

                case DynamicCompare.LessThan:
                    return Expression.LessThan(memberAccess, actualValue);

                case DynamicCompare.LessThanOrEqual:
                    return Expression.LessThanOrEqual(memberAccess, actualValue);

                default:
                    return Expression.Constant(false);
            }
        }

        
        /// <summary>
        /// Create a dynamic comparison expression for a given property selector, comparison method and reference value.
        /// </summary>
        /// <param name="target">The parameter of the query data.</param>
        /// <param name="selector">The property selector to parse.</param>
        /// <param name="comparer">The comparison method to use.</param>
        /// <param name="value">The reference value to compare with.</param>
        /// <param name="provider">The culture-specific formatting information.</param>
        /// <returns>The dynamic comparison expression.</returns>
        public static Expression CreateComparison(ParameterExpression target, string selector, string comparer, string value, IFormatProvider provider = null)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            if (string.IsNullOrEmpty(selector))
                throw new ArgumentNullException(nameof(selector));
            if (string.IsNullOrEmpty(comparer))
                throw new ArgumentNullException(nameof(comparer));

            var memberAccess = CreateMemberAccess(target, selector);
            var actualValue = CreateConstant(target, memberAccess, value, provider);

            return Expression.Call(memberAccess, comparer, null, actualValue);
        }

        /// <summary>
        /// Creates a dynamic member access expression.
        /// </summary>
        /// <param name="target">The parameter of the query data.</param>
        /// <param name="selector">The property selector to parse.</param>
        /// <returns>The dynamic member access expression.</returns>
        public static Expression CreateMemberAccess(Expression target, string selector)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            if (string.IsNullOrEmpty(selector))
                throw new ArgumentNullException(nameof(selector));

            return selector.Split('.').Aggregate(target, Expression.PropertyOrField);
        }

        private static Expression CreateConstant(ParameterExpression target, Expression selector, string value, IFormatProvider provider)
        {
            var type = Expression.Lambda(selector, target).ReturnType;

            if (string.IsNullOrEmpty(value))
                return Expression.Default(type);

            var converter = cache.GetOrAdd(type, CreateConverter);
            var convertedValue = converter(value, provider);

            return Expression.Constant(convertedValue, type);
        }

        private static Func<string, IFormatProvider, object> CreateConverter(Type type)
        {
            var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

            var target = Expression.Parameter(typeof(string));
            var format = Expression.Parameter(typeof(IFormatProvider));

            var expression = (Expression)target;

            var ordinalParse = underlyingType.GetMethod("Parse", new[] { typeof(string) });
            if (ordinalParse != null)
                expression = Expression.Call(ordinalParse, target);

            var cultureParse = underlyingType.GetMethod("Parse", new[] { typeof(string), typeof(IFormatProvider) });
            if (cultureParse != null)
                expression = Expression.Call(cultureParse, target, format);

            return Expression.Lambda<Func<string, IFormatProvider, object>>(
                Expression.Convert(expression, typeof(object)), target, format).Compile();
        }
    }
}
