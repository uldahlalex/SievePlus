using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Extensions.Options;
using Sieve.Plus.Extensions;
using Sieve.Plus.Attributes;
using Sieve.Plus.Exceptions;
using Sieve.Plus.Models;

namespace Sieve.Plus.Services
{
    public class SievePlusProcessor : SievePlusProcessor<SievePlusModel, FilterTerm, SortTerm>, ISievePlusProcessor
    {
        public SievePlusProcessor(IOptions<SievePlusOptions> options)
            : base(options)
        {
        }

        public SievePlusProcessor(IOptions<SievePlusOptions> options, ISievePlusCustomSortMethods plusCustomSortMethods)
            : base(options, plusCustomSortMethods)
        {
        }

        public SievePlusProcessor(IOptions<SievePlusOptions> options, ISievePlusCustomFilterMethods plusCustomFilterMethods)
            : base(options, plusCustomFilterMethods)
        {
        }

        public SievePlusProcessor(IOptions<SievePlusOptions> options, ISievePlusCustomSortMethods plusCustomSortMethods,
            ISievePlusCustomFilterMethods plusCustomFilterMethods) : base(options, plusCustomSortMethods, plusCustomFilterMethods)
        {
        }
    }

    public class SievePlusProcessor<TFilterTerm, TSortTerm> :
        SievePlusProcessor<SievePlusModel<TFilterTerm, TSortTerm>, TFilterTerm, TSortTerm>, ISievePlusProcessor<TFilterTerm, TSortTerm>
        where TFilterTerm : IFilterTerm, new()
        where TSortTerm : ISortTerm, new()
    {
        public SievePlusProcessor(IOptions<SievePlusOptions> options)
            : base(options)
        {
        }

        public SievePlusProcessor(IOptions<SievePlusOptions> options, ISievePlusCustomSortMethods plusCustomSortMethods)
            : base(options, plusCustomSortMethods)
        {
        }

        public SievePlusProcessor(IOptions<SievePlusOptions> options, ISievePlusCustomFilterMethods plusCustomFilterMethods)
            : base(options, plusCustomFilterMethods)
        {
        }

        public SievePlusProcessor(IOptions<SievePlusOptions> options, ISievePlusCustomSortMethods plusCustomSortMethods,
            ISievePlusCustomFilterMethods plusCustomFilterMethods)
            : base(options, plusCustomSortMethods, plusCustomFilterMethods)
        {
        }
    }

    public class SievePlusProcessor<TSieveModel, TFilterTerm, TSortTerm> : ISievePlusProcessor<TSieveModel, TFilterTerm, TSortTerm>
        where TSieveModel : class, ISievePlusModel<TFilterTerm, TSortTerm>
        where TFilterTerm : IFilterTerm, new()
        where TSortTerm : ISortTerm, new()
    {
        private const string NullFilterValue = "null";
        private const char EscapeChar = '\\';
        private readonly ISievePlusCustomSortMethods _plusCustomSortMethods;
        private readonly ISievePlusCustomFilterMethods _plusCustomFilterMethods;
        private readonly SievePlusPropertyMapper _mapper = new SievePlusPropertyMapper();

        public SievePlusProcessor(IOptions<SievePlusOptions> options,
            ISievePlusCustomSortMethods plusCustomSortMethods,
            ISievePlusCustomFilterMethods plusCustomFilterMethods)
        {
            _mapper = MapProperties(_mapper);
            Options = options;
            _plusCustomSortMethods = plusCustomSortMethods;
            _plusCustomFilterMethods = plusCustomFilterMethods;
        }

        public SievePlusProcessor(IOptions<SievePlusOptions> options,
            ISievePlusCustomSortMethods plusCustomSortMethods)
        {
            _mapper = MapProperties(_mapper);
            Options = options;
            _plusCustomSortMethods = plusCustomSortMethods;
        }

        public SievePlusProcessor(IOptions<SievePlusOptions> options,
            ISievePlusCustomFilterMethods plusCustomFilterMethods)
        {
            _mapper = MapProperties(_mapper);
            Options = options;
            _plusCustomFilterMethods = plusCustomFilterMethods;
        }

        public SievePlusProcessor(IOptions<SievePlusOptions> options)
        {
            _mapper = MapProperties(_mapper);
            Options = options;
        }

        protected IOptions<SievePlusOptions> Options { get; }

        /// <summary>
        /// Apply filtering, sorting, and pagination parameters found in `model` to `source`
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="model">An instance of ISievePlusModel</param>
        /// <param name="source">Data source</param>
        /// <param name="dataForCustomMethods">Additional data that will be passed down to custom methods</param>
        /// <param name="applyFiltering">Should the data be filtered? Defaults to true.</param>
        /// <param name="applySorting">Should the data be sorted? Defaults to true.</param>
        /// <param name="applyPagination">Should the data be paginated? Defaults to true.</param>
        /// <returns>Returns a transformed version of `source`</returns>
        public IQueryable<TEntity> Apply<TEntity>(TSieveModel model, IQueryable<TEntity> source,
            object[] dataForCustomMethods = null, bool applyFiltering = true, bool applySorting = true,
            bool applyPagination = true)
        {
            var result = source;

            if (model == null)
            {
                return result;
            }

            try
            {
                if (applyFiltering)
                {
                    result = ApplyFiltering(model, result, dataForCustomMethods);
                }

                if (applySorting)
                {
                    result = ApplySorting(model, result, dataForCustomMethods);
                }

                if (applyPagination)
                {
                    result = ApplyPagination(model, result);
                }

                return result;
            }
            catch (Exception ex)
            {
                if (!Options.Value.ThrowExceptions)
                {
                    return result;
                }

                if (ex is SievePlusException)
                {
                    throw;
                }

                throw new SievePlusException(ex.Message, ex);
            }
        }

        protected virtual IQueryable<TEntity> ApplyFiltering<TEntity>(TSieveModel model, IQueryable<TEntity> result,
            object[] dataForCustomMethods = null)
        {
            var filterGroups = model?.GetFiltersWithOrParsed();
            if (filterGroups == null)
            {
                return result;
            }

            Expression outerExpression = null;
            var parameter = Expression.Parameter(typeof(TEntity), "e");

            // Process each OR group
            foreach (var filterGroup in filterGroups)
            {
                Expression groupExpression = null;

                // Within each group, combine filters with AND logic
                foreach (var filterTerm in filterGroup)
                {
                    Expression innerExpression = null;
                    foreach (var filterTermName in filterTerm.Names)
                    {
                        var (fullPropertyName, property) = GetSieveProperty<TEntity>(false, true, filterTermName);
                        if (property != null)
                        {
                            if (filterTerm.Values == null)
                            {
                                continue;
                            }

                            var converter = TypeDescriptor.GetConverter(property.PropertyType);
                            foreach (var filterTermValue in filterTerm.Values)
                            {
                                var (propertyValue, nullCheck) =
                                    GetPropertyValueAndNullCheckExpression(parameter, fullPropertyName);

                                var isFilterTermValueNull =
                                    IsFilterTermValueNull(propertyValue, filterTerm, filterTermValue);

                                var filterValue = isFilterTermValueNull
                                    ? Expression.Constant(null, property.PropertyType)
                                    : ConvertStringValueToConstantExpression(filterTermValue, property, converter);

                                if (filterTerm.OperatorIsCaseInsensitive && !isFilterTermValueNull)
                                {
                                    propertyValue = Expression.Call(propertyValue,
                                        typeof(string).GetMethods()
                                            .First(m => m.Name == "ToUpper" && m.GetParameters().Length == 0));

                                    filterValue = Expression.Call(filterValue,
                                        typeof(string).GetMethods()
                                            .First(m => m.Name == "ToUpper" && m.GetParameters().Length == 0));
                                }

                                var expression = GetExpression(filterTerm, filterValue, propertyValue);

                                if (filterTerm.OperatorIsNegated)
                                {
                                    expression = Expression.Not(expression);
                                }

                                if (expression.NodeType != ExpressionType.NotEqual || Options.Value.IgnoreNullsOnNotEqual)
                                {
                                    var filterValueNullCheck = GetFilterValueNullCheck(parameter, fullPropertyName, isFilterTermValueNull);
                                    if (filterValueNullCheck != null)
                                    {
                                        expression = Expression.AndAlso(filterValueNullCheck, expression);
                                    }
                                }

                                innerExpression = innerExpression == null
                                    ? expression
                                    : Expression.OrElse(innerExpression, expression);
                            }
                        }
                        else
                        {
                            result = ApplyCustomMethod(result, filterTermName, _plusCustomFilterMethods,
                                new object[] {result, filterTerm.Operator, filterTerm.Values}, dataForCustomMethods);
                        }
                    }

                    // Combine filter terms within this group with AND
                    if (groupExpression == null)
                    {
                        groupExpression = innerExpression;
                    }
                    else if (innerExpression != null)
                    {
                        groupExpression = Expression.AndAlso(groupExpression, innerExpression);
                    }
                }

                // Combine groups with OR logic
                if (outerExpression == null)
                {
                    outerExpression = groupExpression;
                }
                else if (groupExpression != null)
                {
                    outerExpression = Expression.OrElse(outerExpression, groupExpression);
                }
            }

            return outerExpression == null
                ? result
                : result.Where(Expression.Lambda<Func<TEntity, bool>>(outerExpression, parameter));
        }

        private static Expression GetFilterValueNullCheck(Expression parameter, string fullPropertyName, bool isFilterTermValueNull)
        {
            var (propertyValue, nullCheck) = GetPropertyValueAndNullCheckExpression(parameter, fullPropertyName);

            if (!isFilterTermValueNull && propertyValue.Type.IsNullable())
            {
                return GenerateFilterNullCheckExpression(propertyValue, nullCheck);
            }

            return nullCheck;
        }

        private static bool IsFilterTermValueNull(Expression propertyValue, TFilterTerm filterTerm,
            string filterTermValue)
        {
            var isNotString = propertyValue.Type != typeof(string);

            var isValidStringNullOperation = filterTerm.OperatorParsed == FilterOperator.Equals ||
                                             filterTerm.OperatorParsed == FilterOperator.NotEquals;

            return filterTermValue.ToLower() == NullFilterValue && (isNotString || isValidStringNullOperation);
        }

        private static (Expression propertyValue, Expression nullCheck) GetPropertyValueAndNullCheckExpression(
            Expression parameter, string fullPropertyName)
        {
            var propertyValue = parameter;
            Expression nullCheck = null;
            var names = fullPropertyName.Split('.');
            for (var i = 0; i < names.Length; i++)
            {
                propertyValue = Expression.PropertyOrField(propertyValue, names[i]);

                if (i != names.Length - 1 && propertyValue.Type.IsNullable())
                {
                    nullCheck = GenerateFilterNullCheckExpression(propertyValue, nullCheck);
                }
            }

            return (propertyValue, nullCheck);
        }

        private static Expression GenerateFilterNullCheckExpression(Expression propertyValue,
            Expression nullCheckExpression)
        {
            return nullCheckExpression == null
                ? Expression.NotEqual(propertyValue, Expression.Default(propertyValue.Type))
                : Expression.AndAlso(nullCheckExpression,
                    Expression.NotEqual(propertyValue, Expression.Default(propertyValue.Type)));
        }

        private static Expression ConvertStringValueToConstantExpression(string value, PropertyInfo property,
            TypeConverter converter)
        {
            // to allow user to distinguish between prop==null (as null) and prop==\null (as "null"-string)
            value = value.Equals(EscapeChar + NullFilterValue, StringComparison.InvariantCultureIgnoreCase) 
                ? value.TrimStart(EscapeChar) 
                : value;
            dynamic constantVal = converter.CanConvertFrom(typeof(string))
                ? converter.ConvertFrom(value)
                : Convert.ChangeType(value, property.PropertyType);

            return GetClosureOverConstant(constantVal, property.PropertyType);
        }

        private static Expression GetExpression(TFilterTerm filterTerm, dynamic filterValue, dynamic propertyValue)
        {
            return filterTerm.OperatorParsed switch
            {
                FilterOperator.Equals => Expression.Equal(propertyValue, filterValue),
                FilterOperator.NotEquals => Expression.NotEqual(propertyValue, filterValue),
                FilterOperator.GreaterThan => Expression.GreaterThan(propertyValue, filterValue),
                FilterOperator.LessThan => Expression.LessThan(propertyValue, filterValue),
                FilterOperator.GreaterThanOrEqualTo => Expression.GreaterThanOrEqual(propertyValue, filterValue),
                FilterOperator.LessThanOrEqualTo => Expression.LessThanOrEqual(propertyValue, filterValue),
                FilterOperator.Contains => Expression.Call(propertyValue,
                    typeof(string).GetMethods().First(m => m.Name == "Contains" && m.GetParameters().Length == 1),
                    filterValue),
                FilterOperator.StartsWith => Expression.Call(propertyValue,
                    typeof(string).GetMethods().First(m => m.Name == "StartsWith" && m.GetParameters().Length == 1),
                    filterValue),
                FilterOperator.EndsWith => Expression.Call(propertyValue,
                typeof(string).GetMethods().First(m => m.Name == "EndsWith" && m.GetParameters().Length == 1),
                filterValue),
                _ => Expression.Equal(propertyValue, filterValue)
            };
        }

        // Workaround to ensure that the filter value gets passed as a parameter in generated SQL from EF Core
        private static Expression GetClosureOverConstant<T>(T constant, Type targetType)
        {
            Expression<Func<T>> hoistedConstant = () => constant;
            return Expression.Convert(hoistedConstant.Body, targetType);
        }

        protected virtual IQueryable<TEntity> ApplySorting<TEntity>(TSieveModel model, IQueryable<TEntity> result,
            object[] dataForCustomMethods = null)
        {
            if (model?.GetSortsParsed() == null)
            {
                return result;
            }

            var useThenBy = false;
            foreach (var sortTerm in model.GetSortsParsed())
            {
                var (fullName, property) = GetSieveProperty<TEntity>(true, false, sortTerm.Name);

                if (property != null)
                {
                    result = result.OrderByDynamic(fullName, property, sortTerm.Descending, useThenBy, Options.Value.DisableNullableTypeExpressionForSorting);
                }
                else
                {
                    result = ApplyCustomMethod(result, sortTerm.Name, _plusCustomSortMethods,
                        new object[] {result, useThenBy, sortTerm.Descending}, dataForCustomMethods);
                }

                useThenBy = true;
            }

            return result;
        }

        protected virtual IQueryable<TEntity> ApplyPagination<TEntity>(TSieveModel model, IQueryable<TEntity> result)
        {
            var page = model?.Page ?? 1;
            var pageSize = model?.PageSize ?? Options.Value.DefaultPageSize;
            var maxPageSize = Options.Value.MaxPageSize > 0 ? Options.Value.MaxPageSize : pageSize;

            if (pageSize <= 0)
            {
                return result;
            }

            result = result.Skip((page - 1) * pageSize);
            result = result.Take(Math.Min(pageSize, maxPageSize));

            return result;
        }

        protected virtual SievePlusPropertyMapper MapProperties(SievePlusPropertyMapper mapper)
        {
            return mapper;
        }

        private (string, PropertyInfo) GetSieveProperty<TEntity>(bool canSortRequired, bool canFilterRequired,
            string name)
        {
            var property = _mapper.FindProperty<TEntity>(canSortRequired, canFilterRequired, name,
                Options.Value.CaseSensitive);
            if (property.Item1 != null)
            {
                return property;
            }

            var prop = FindPropertyBySieveAttribute<TEntity>(canSortRequired, canFilterRequired, name,
                Options.Value.CaseSensitive);
            return (prop?.Name, prop);
        }

        private static PropertyInfo FindPropertyBySieveAttribute<TEntity>(bool canSortRequired, bool canFilterRequired,
            string name, bool isCaseSensitive)
        {
            return Array.Find(typeof(TEntity).GetProperties(),
                p => p.GetCustomAttribute(typeof(SievePlusAttribute)) is SievePlusAttribute SieveAttribute
                     && (!canSortRequired || SieveAttribute.CanSort)
                     && (!canFilterRequired || SieveAttribute.CanFilter)
                     && (SieveAttribute.Name ?? p.Name).Equals(name,
                         isCaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase));
        }

        private IQueryable<TEntity> ApplyCustomMethod<TEntity>(IQueryable<TEntity> result, string name, object parent,
            object[] parameters, object[] optionalParameters = null)
        {
            var customMethod = parent?.GetType()
                .GetMethodExt(name,
                    Options.Value.CaseSensitive
                        ? BindingFlags.Default
                        : BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance,
                    typeof(IQueryable<TEntity>));


            if (customMethod == null)
            {
                // Find generic methods `public IQueryable<T> Filter<T>(IQueryable<T> source, ...)`
                var genericCustomMethod = parent?.GetType()
                    .GetMethodExt(name,
                        Options.Value.CaseSensitive
                            ? BindingFlags.Default
                            : BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance,
                        typeof(IQueryable<>));

                if (genericCustomMethod != null &&
                    genericCustomMethod.ReturnType.IsGenericType &&
                    genericCustomMethod.ReturnType.GetGenericTypeDefinition() == typeof(IQueryable<>))
                {
                    var genericBaseType = genericCustomMethod.ReturnType.GenericTypeArguments[0];
                    var constraints = genericBaseType.GetGenericParameterConstraints();
                    if (constraints == null || constraints.Length == 0 ||
                        constraints.All((t) => t.IsAssignableFrom(typeof(TEntity))))
                    {
                        customMethod = genericCustomMethod.MakeGenericMethod(typeof(TEntity));
                    }
                }
            }

            if (customMethod != null)
            {
                try
                {
                    result = customMethod.Invoke(parent, parameters)
                        as IQueryable<TEntity>;
                }
                catch (TargetParameterCountException)
                {
                    if (optionalParameters != null)
                    {
                        result = customMethod.Invoke(parent, parameters.Concat(optionalParameters).ToArray())
                            as IQueryable<TEntity>;
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            else
            {
                var incompatibleCustomMethods =
                    parent?
                        .GetType()
                        .GetMethods(Options.Value.CaseSensitive
                            ? BindingFlags.Default
                            : BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)
                        .Where(method => string.Equals(method.Name, name,
                            Options.Value.CaseSensitive
                                ? StringComparison.InvariantCulture
                                : StringComparison.InvariantCultureIgnoreCase))
                        .ToList()
                    ?? new List<MethodInfo>();

                if (!incompatibleCustomMethods.Any())
                {
                    throw new SievePlusMethodNotFoundException(name, $"{name} not found.");
                }

                var incompatibles =
                    from incompatibleCustomMethod in incompatibleCustomMethods
                    let expected = typeof(IQueryable<TEntity>)
                    let actual = incompatibleCustomMethod.ReturnType
                    select new SievePlusIncompatibleMethodException(name, expected, actual,
                        $"{name} failed. Expected a custom method for type {expected} but only found for type {actual}");

                var aggregate = new AggregateException(incompatibles);

                throw new SievePlusIncompatibleMethodException(aggregate.Message, aggregate);
            }

            return result;
        }
    }
}
