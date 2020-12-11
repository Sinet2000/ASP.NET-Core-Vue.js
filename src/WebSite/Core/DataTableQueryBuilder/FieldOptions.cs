using System;
using System.Linq.Expressions;

namespace DataTableQueryBuilder
{
    using ValueMatchers;

    public class FieldOptions<T>
    {
        /// <summary>
        /// Gets the Entity's property access expression that is used when searching and sorting.
        /// </summary>
        internal Expression? EntityProperty { get; private set; }

        /// <summary>
        /// Gets the value match method that is used when searching. Default is String.Contains() for strings and integers, and equals for other value types.
        /// </summary>
        internal ValueMatchMethod ValueMatchMethod { get; private set; }

        /// <summary>
        /// Checks if global search is enabled on this field.
        /// </summary>
        internal bool IsGlobalSearchEnabled { get; private set; }

        /// <summary>
        /// Gets the search expression that is used when searching.
        /// </summary>
        internal Expression<Func<T, string, bool>>? SearchExpression { get; private set; }

        /// <summary>
        /// Gets the sort expression that is used when sorting.
        /// </summary>
        internal Expression<Func<T, object>>? SortExpression { get; private set; }

        public FieldOptions(Expression? entityPropertyAccessExp)
        {
            EntityProperty = entityPropertyAccessExp;
            ValueMatchMethod = ValueMatchMethod.StringContains;
        }

        /// <summary>
        /// Explicitly sets the Entity's property to use when searching and sorting.
        /// </summary>
        /// <typeparam name="TMember"></typeparam>
        /// <param name="property"></param>
        public void SetEntityProperty<TMember>(Expression<Func<T, TMember>> property)
        {
            EntityProperty = property.Body;
        }

        /// <summary>
        /// Enables a global search on this field.
        /// </summary>
        public void EnableGlobalSearch()
        {
            IsGlobalSearchEnabled = true;
        }

        /// <summary>
        /// Explicitly sets the search expression to be used during searching.
        /// </summary>
        /// <param name="expression"></param>
        public void SetSearchExp(Expression<Func<T, string, bool>> expression)
        {
            SearchExpression = expression;
        }

        /// <summary>
        /// Explicitly sets the sort expression to be used during sorting.
        /// </summary>
        /// <param name="expression"></param>
        public void SetSortExp(Expression<Func<T, object>> expression)
        {
            SortExpression = expression;
        }

        /// <summary>
        /// Explicitly sets the value match method to be used during searching.
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public void SetValueMatchMethod(ValueMatchMethod method)
        {
            ValueMatchMethod = method;
        }
    }
}
