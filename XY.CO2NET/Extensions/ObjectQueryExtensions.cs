using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace XY.CO2NET.Extensions
{
    public static class ObjectQueryExtensions
    {
        /// <summary>
        /// 委托排序
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="orderBy"></param>
        /// <param name="isAscending">是否升序排列</param>
        /// <returns></returns>
        public static IOrderedQueryable<T> OrderBy<T, TK>(this IQueryable<T> obj, Expression<Func<T, TK>> orderBy, bool isAscending = true) where T : class
        {
            if (orderBy == null)
                throw new Exception("OrderBy can not be Null！");

            return isAscending ? obj.OrderBy(orderBy) : obj.OrderByDescending(orderBy);
        }

        /// <summary>
        /// 委托排序
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="ThenBy"></param>
        /// <param name="isAscending">是否升序排列</param>
        /// <returns></returns>
        public static IOrderedQueryable<T> ThenBy<T, TK>(this IOrderedQueryable<T> obj, Expression<Func<T, TK>> thenBy, bool isAscending = true) where T : class
        {
            if (thenBy == null)
                throw new Exception("OrderBy can not be Null！");

            return isAscending ? obj.ThenBy(thenBy) : obj.ThenByDescending(thenBy);
        }
    }
}
