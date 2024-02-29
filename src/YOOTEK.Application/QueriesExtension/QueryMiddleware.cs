#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using static Yootek.Common.Enum.CommonENum;
using static Yootek.YootekServiceBase;

namespace Yootek.Application
{
    public static class QueryMiddleware
    {
        public static IQueryable<T> ApplySort<T>(this IQueryable<T> query, Enum? enumParameter, SortBy sortBy = SortBy.ASC)
        {
            if (enumParameter != null && Enum.IsDefined(enumParameter.GetType(), enumParameter))
            {
                string orderByField = GetEnumFieldName(enumParameter);
                ParameterExpression param = Expression.Parameter(typeof(T), "x");
                MemberExpression prop = Expression.Property(param, orderByField);
                LambdaExpression exp = Expression.Lambda(prop, param);

                string method;
                if (HasOrderByOrOrderByDescending(query))
                {
                    method = sortBy != SortBy.DESC ? "ThenBy" : "ThenByDescending";
                }
                else
                {
                    method = sortBy != SortBy.DESC ? "OrderBy" : "OrderByDescending";
                }
                Type[] types = new Type[] { query.ElementType, exp.Body.Type };
                MethodCallExpression methodCallExpression = Expression.Call(typeof(Queryable), method, types, query.Expression, exp);
                return query.Provider.CreateQuery<T>(methodCallExpression);
            }
            return query;
        }

        public static IQueryable<T> ApplySearchFilter<T>(this IQueryable<T> query, string? keyword, params Expression<Func<T, string>>[] fields)
        {
            if (string.IsNullOrWhiteSpace(keyword) || fields == null || fields.Length == 0)
            {
                return query;
            }

            char[] separators = { '+', ' ' };
            string[] keywords = keyword.ToLower().Trim().Split(separators, StringSplitOptions.RemoveEmptyEntries);

            if (keywords.Any())
            {
                ParameterExpression parameter = Expression.Parameter(typeof(T), "x");
                Expression body = Expression.Constant(false);
                MethodInfo containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) })!;
                MethodInfo toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes)!;

                foreach (var field in fields)
                {
                    InvocationExpression property = Expression.Invoke(Expression.Constant(field), parameter);
                    MethodCallExpression toLowerExpression = Expression.Call(property, toLowerMethod);
                    MethodCallExpression searchExpression = Expression.Call(toLowerExpression, containsMethod, Expression.Constant(keyword.ToLower()));
                    body = Expression.OrElse(body, searchExpression);
                }

                Expression<Func<T, bool>> lambdaExpression = Expression.Lambda<Func<T, bool>>(body, parameter);
                query = query.Where(lambdaExpression);
            }
            return query;
        }

        #region method helpers 
        private static string GetEnumFieldName(Enum value)
        {
            FieldInfo? fieldInfo = value.GetType().GetField(value.ToString());

            if (fieldInfo != null)
            {
                FieldNameAttribute? attr = fieldInfo.GetCustomAttribute<FieldNameAttribute>();

                if (attr != null)
                {
                    return attr.FieldNameString;
                }
            }
            return value.ToString();
        }
        public static string GetEnumDesctiption(Enum value)
        {
            FieldInfo? fieldInfo = value.GetType().GetField(value.ToString());

            if (fieldInfo != null)
            {
                DescriptionAttribute? descriptionAttribute = fieldInfo.GetCustomAttribute<DescriptionAttribute>();
                if (descriptionAttribute != null)
                {
                    return descriptionAttribute.Description;
                }
            }
            return value.ToString();
        }

        public static List<EnumListItem> GetEnumList<T>() where T : Enum
        {
            Type enumType = typeof(T);

            if (!enumType.IsEnum)
            {
                throw new ArgumentException("T must be an enumerated type.");
            }

            List<EnumListItem> enumList = new List<EnumListItem>();

            foreach (T value in Enum.GetValues(enumType))
            {
                FieldInfo fieldInfo = enumType.GetField(value.ToString());

                if (fieldInfo != null)
                {
                    DescriptionAttribute descriptionAttribute = fieldInfo.GetCustomAttribute<DescriptionAttribute>();

                    if (descriptionAttribute != null)
                    {
                        EnumListItem item = new EnumListItem
                        {
                            Value = Convert.ToInt32(value),
                            Label = descriptionAttribute.Description
                        };

                        enumList.Add(item);
                    }
                }
            }

            return enumList;
        }

        public class EnumListItem
        {
            public int Value { get; set; }
            public string Label { get; set; }
        }

        private static bool HasOrderByOrOrderByDescending<T>(IQueryable<T> query)
        {
            // Kiểm tra xem truy vấn đã gọi OrderBy hoặc OrderByDescending chưa
            return query.Expression.ToString().Contains("OrderBy") || query.Expression.ToString().Contains("OrderByDescending");
        }
        #endregion
    }
}
