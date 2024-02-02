using Yootek.Organizations.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Yootek.QueriesExtension
{
    public static class BuildingUrbanQueryExtension
    {
        public static IQueryable<TEntity> WhereByBuildingOrUrban<TEntity>(this IQueryable<TEntity> query, List<long> ids) where TEntity : class
        {
            bool mustHaveBuilding = typeof(IMustHaveBuilding).IsAssignableFrom(typeof(TEntity));
            bool mustHaveUrban = typeof(IMustHaveUrban).IsAssignableFrom(typeof(TEntity));
            bool mayHaveBuilding = typeof(IMayHaveBuilding).IsAssignableFrom(typeof(TEntity));
            bool mayHaveUrban = typeof(IMayHaveUrban).IsAssignableFrom(typeof(TEntity));
            Expression<Func<TEntity, bool>> expression = null;

            if (mustHaveBuilding && mustHaveUrban)
            {
                expression = e => ids.Contains(((IMustHaveBuilding)e).BuildingId) || ids.Contains(((IMustHaveUrban)e).UrbanId);
            } 
          
            if (mayHaveBuilding && mayHaveUrban)
            {
                expression = e => (((IMayHaveBuilding)e).BuildingId.HasValue && ids.Contains(((IMayHaveBuilding)e).BuildingId.Value))
                || (((IMayHaveUrban)e).UrbanId.HasValue && ids.Contains(((IMayHaveUrban)e).UrbanId.Value));
            }

            return expression != null ?  query.Where(expression) : query;
        }

        public static IQueryable<TEntity> WhereByBuilding<TEntity>(this IQueryable<TEntity> query, List<long> ids) where TEntity : class
        {
            bool mustHaveBuilding = typeof(IMustHaveBuilding).IsAssignableFrom(typeof(TEntity));
            bool mayHaveBuilding = typeof(IMayHaveBuilding).IsAssignableFrom(typeof(TEntity));
            Expression<Func<TEntity, bool>> expression = null;
            if (mustHaveBuilding)
            {
                expression = e => ids.Contains(((IMustHaveBuilding)e).BuildingId);
            }

            if (mayHaveBuilding)
            {
                expression = e => ((IMayHaveBuilding)e).BuildingId.HasValue && ids.Contains(((IMayHaveBuilding)e).BuildingId.Value);
            }

            return expression != null ? query.Where(expression) : query;
        }

        public static IQueryable<TEntity> WhereByUrban<TEntity>(this IQueryable<TEntity> query, List<long> ids) where TEntity : class
        {
            bool mustHaveUrban = typeof(IMustHaveUrban).IsAssignableFrom(typeof(TEntity));
            bool mayHaveUrban = typeof(IMayHaveUrban).IsAssignableFrom(typeof(TEntity));
            Expression<Func<TEntity, bool>> expression = null;

            if (mustHaveUrban)
            {
                expression = e => ids.Contains(((IMustHaveUrban)e).UrbanId);
            }

            if (mayHaveUrban)
            {
                expression = e => ((IMayHaveUrban)e).UrbanId.HasValue && ids.Contains(((IMayHaveUrban)e).UrbanId.Value);
            }

            return expression != null ? query.Where(expression) : query;
        }

        public static IQueryable<TEntity> WhereByBuildingOrUrbanIf<TEntity>(this IQueryable<TEntity> query, bool condition, List<long> ids) where TEntity : class
        {
            bool mustHaveBuilding = typeof(IMustHaveBuilding).IsAssignableFrom(typeof(TEntity));
            bool mustHaveUrban = typeof(IMustHaveUrban).IsAssignableFrom(typeof(TEntity));
            bool mayHaveBuilding = typeof(IMayHaveBuilding).IsAssignableFrom(typeof(TEntity));
            bool mayHaveUrban = typeof(IMayHaveUrban).IsAssignableFrom(typeof(TEntity));
            Expression<Func<TEntity, bool>> expression = null;

            if (mustHaveBuilding && mustHaveUrban)
            {
                expression = e => ids.Contains(((IMustHaveBuilding)e).BuildingId) || ids.Contains(((IMustHaveUrban)e).UrbanId);
            }

            if (mayHaveBuilding && mayHaveUrban)
            {
                expression = e => (((IMayHaveBuilding)e).BuildingId.HasValue && ids.Contains(((IMayHaveBuilding)e).BuildingId.Value))
                || (((IMayHaveUrban)e).UrbanId.HasValue && ids.Contains(((IMayHaveUrban)e).UrbanId.Value));
            }

            return (expression != null && condition) ? query.Where(expression) : query;
        }
        public static IQueryable<TEntity> WhereByBuildingIf<TEntity>(this IQueryable<TEntity> query, bool condition, List<long> ids) where TEntity : class
        {
            bool mustHaveBuilding = typeof(IMustHaveBuilding).IsAssignableFrom(typeof(TEntity));
            bool mayHaveBuilding = typeof(IMayHaveBuilding).IsAssignableFrom(typeof(TEntity));
            Expression<Func<TEntity, bool>> expression = null;
            if (mustHaveBuilding)
            {
                expression = e => ids.Contains(((IMustHaveBuilding)e).BuildingId);
            }

            if (mayHaveBuilding)
            {
                expression = e => ((IMayHaveBuilding)e).BuildingId.HasValue && ids.Contains(((IMayHaveBuilding)e).BuildingId.Value);
            }

            return (expression != null && condition) ? query.Where(expression) : query;
        }

        public static IQueryable<TEntity> WhereByUrbanIf<TEntity>(this IQueryable<TEntity> query, bool condition, List<long> ids) where TEntity : class
        {
            bool mustHaveUrban = typeof(IMustHaveUrban).IsAssignableFrom(typeof(TEntity));
            bool mayHaveUrban = typeof(IMayHaveUrban).IsAssignableFrom(typeof(TEntity));
            Expression<Func<TEntity, bool>> expression = null;

            if (mustHaveUrban)
            {
                expression = e => ids.Contains(((IMustHaveUrban)e).UrbanId);
            }

            if (mayHaveUrban)
            {
                expression = e => ((IMayHaveUrban)e).UrbanId.HasValue && ids.Contains(((IMayHaveUrban)e).UrbanId.Value);
            }

            return (expression != null && condition) ? query.Where(expression) : query;
        }

    }
}
