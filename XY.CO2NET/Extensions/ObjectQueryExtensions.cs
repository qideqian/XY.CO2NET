using System.Linq.Expressions;

namespace System.Linq
{
    public static class ObjectQueryExtensions
    {
        /// <summary>
        /// 按键选择表达式构造查询排序，根据 isAscending 选择升序或降序。
        /// </summary>
        /// <typeparam name="T">查询元素类型。</typeparam>
        /// <typeparam name="TK">排序键类型。</typeparam>
        /// <param name="obj"></param>
        /// <param name="orderBy">主排序键选择表达式；不可为 null。</param>
        /// <param name="isAscending">是否升序排列</param>
        /// <returns></returns>
        public static IOrderedQueryable<T> OrderBy<T, TK>(this IQueryable<T> obj, Expression<Func<T, TK>> orderBy, bool isAscending) where T : class
        {
            if (orderBy == null)
                throw new Exception("OrderBy can not be Null！");

            return isAscending ? obj.OrderBy(orderBy) : obj.OrderByDescending(orderBy);
        }

        /// <summary>
        /// 按键选择表达式追加次级查询排序，根据 isAscending 选择升序或降序。
        /// </summary>
        /// <typeparam name="T">查询元素类型。</typeparam>
        /// <typeparam name="TK">排序键类型。</typeparam>
        /// <param name="obj"></param>
        /// <param name="thenBy">次级排序键选择表达式；不可为 null。</param>
        /// <param name="isAscending">是否升序排列</param>
        /// <returns></returns>
        public static IOrderedQueryable<T> ThenBy<T, TK>(this IOrderedQueryable<T> obj, Expression<Func<T, TK>> thenBy, bool isAscending) where T : class
        {
            if (thenBy == null)
                throw new Exception("OrderBy can not be Null！");

            return isAscending ? obj.ThenBy(thenBy) : obj.ThenByDescending(thenBy);
        }
    }
}
