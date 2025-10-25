using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Sieve.Plus.Models;

namespace Sieve.Plus.Services
{
    /// <summary>
    /// Maps query model properties to entity expressions.
    /// This allows explicit definition of what can be filtered/sorted and how it maps to the entity.
    /// </summary>
    /// <typeparam name="TQueryModel">The query model</typeparam>
    /// <typeparam name="TEntity">The entity model</typeparam>
    public class SievePlusQueryMapper<TQueryModel, TEntity>
        where TQueryModel : ISievePlusQueryModel
    {
        internal Dictionary<string, QueryPropertyMapping> Mappings { get; } = new Dictionary<string, QueryPropertyMapping>();

        /// <summary>
        /// Map a query model property to an entity property expression.
        /// This is the most common mapping for simple 1:1 property mappings.
        /// </summary>
        /// <typeparam name="TProperty">The property type</typeparam>
        /// <param name="queryProperty">Expression pointing to the query model property</param>
        /// <param name="entityProperty">Expression pointing to the entity property (can be nested or calculated)</param>
        /// <returns>Fluent API to configure filter/sort capabilities</returns>
        public QueryPropertyFluentApi Property<TProperty>(
            Expression<Func<TQueryModel, TProperty>> queryProperty,
            Expression<Func<TEntity, TProperty>> entityProperty)
        {
            var queryPropertyName = GetPropertyName(queryProperty);
            var (fullEntityName, entityPropertyInfo) = GetPropertyInfo(entityProperty);

            var mapping = new QueryPropertyMapping
            {
                QueryPropertyName = queryPropertyName,
                EntityFullPropertyName = fullEntityName,
                EntityPropertyInfo = entityPropertyInfo,
                CanFilter = false,
                CanSort = false,
                IsCustomFilter = false
            };

            return new QueryPropertyFluentApi(this, mapping);
        }

        /// <summary>
        /// Define a custom filter that maps to a boolean expression on the entity.
        /// Use this for filters that aren't simple property comparisons.
        /// </summary>
        /// <param name="queryProperty">The query model boolean property</param>
        /// <param name="entityExpression">The entity boolean expression that implements the filter</param>
        /// <example>
        /// <code>
        /// mapper.CustomFilter(q => q.IsLongBook, e => e.Pages > 500);
        /// </code>
        /// </example>
        public void CustomFilter(
            Expression<Func<TQueryModel, bool?>> queryProperty,
            Expression<Func<TEntity, bool>> entityExpression)
        {
            var queryPropertyName = GetPropertyName(queryProperty);

            var mapping = new QueryPropertyMapping
            {
                QueryPropertyName = queryPropertyName,
                EntityExpression = entityExpression,
                CanFilter = true,
                CanSort = false,
                IsCustomFilter = true
            };

            Mappings[queryPropertyName] = mapping;
        }

        /// <summary>
        /// Fluent API for configuring property capabilities (filter/sort).
        /// </summary>
        public class QueryPropertyFluentApi
        {
            private readonly SievePlusQueryMapper<TQueryModel, TEntity> _mapper;
            private readonly QueryPropertyMapping _mapping;

            internal QueryPropertyFluentApi(SievePlusQueryMapper<TQueryModel, TEntity> mapper, QueryPropertyMapping mapping)
            {
                _mapper = mapper;
                _mapping = mapping;
            }

            /// <summary>
            /// Allow filtering on this property.
            /// </summary>
            public QueryPropertyFluentApi CanFilter()
            {
                _mapping.CanFilter = true;
                UpdateMapping();
                return this;
            }

            /// <summary>
            /// Allow sorting on this property.
            /// </summary>
            public QueryPropertyFluentApi CanSort()
            {
                _mapping.CanSort = true;
                UpdateMapping();
                return this;
            }

            private void UpdateMapping()
            {
                _mapper.Mappings[_mapping.QueryPropertyName] = _mapping;
            }
        }

        /// <summary>
        /// Internal structure holding mapping information.
        /// </summary>
        internal class QueryPropertyMapping
        {
            public string QueryPropertyName { get; set; }
            public string EntityFullPropertyName { get; set; }
            public PropertyInfo EntityPropertyInfo { get; set; }
            public LambdaExpression EntityExpression { get; set; }
            public bool CanFilter { get; set; }
            public bool CanSort { get; set; }
            public bool IsCustomFilter { get; set; }
        }

        private static string GetPropertyName<T>(Expression<Func<TQueryModel, T>> expression)
        {
            if (expression.Body is MemberExpression memberExpression)
            {
                return memberExpression.Member.Name;
            }

            if (expression.Body is UnaryExpression unaryExpression &&
                unaryExpression.Operand is MemberExpression operandMember)
            {
                return operandMember.Member.Name;
            }

            throw new ArgumentException($"Expression '{expression}' does not refer to a property.");
        }

        private static (string fullName, PropertyInfo propertyInfo) GetPropertyInfo<T>(Expression<Func<TEntity, T>> expression)
        {
            MemberExpression body = null;

            if (expression.Body is MemberExpression memberExpression)
            {
                body = memberExpression;
            }
            else if (expression.Body is UnaryExpression unaryExpression)
            {
                body = unaryExpression.Operand as MemberExpression;
            }

            if (body == null)
            {
                throw new ArgumentException($"Expression '{expression}' does not refer to a property.");
            }

            var member = body.Member as PropertyInfo;
            var stack = new Stack<string>();

            while (body != null)
            {
                stack.Push(body.Member.Name);
                body = body.Expression as MemberExpression;
            }

            return (string.Join(".", stack), member);
        }
    }
}
