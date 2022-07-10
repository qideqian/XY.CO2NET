using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace XY.CO2NET.Helpers
{
    /// <summary>
    /// 支持关联表的正则表达式查询条件拼装
    /// 
    /// 类型如：
    ///   var seh = new XYExpressionHelper<T>();
    ///   seh.ValueCompare
    ///       .AndAlso(true, i => !i.IsDeleted && i.MPAccountId == MPAccount.Id)
    ///       .AndAlso(!string.IsNullOrEmpty(SearchText), i => i.RuleName.Contains(SearchText) || i.MPKeywordList.Count(c => c.Content.Contains(SearchText)) > 0);
    ///   var where = seh.BuildWhereExpression();
    ///   
    /// 其中关联表的Count统计，在XYExpressionHelper下，会吧变量c当作主表属性而报错
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class XYExpressionHelper<TEntity> where TEntity : class
    {
        public XYValueCompare<TEntity> ValueCompare { get; set; }
        public XYExpressionHelper()
        {
            ValueCompare = new XYValueCompare<TEntity>();
        }

        /// <summary>
        /// 生成where表达式
        /// </summary>
        /// <returns></returns>
        public Expression<Func<TEntity, bool>> BuildWhereExpression()
        {
            if (ValueCompare.ExpressionBody == null)
            {
                //不需要查询
                Expression<Func<TEntity, bool>> returnTrue = z => true;
                return returnTrue;
            }
            var where = ValueCompare.ExpressionBody;
            return where;
        }
    }
    public class XYValueCompare<TEntity>
    {
        public Expression<Func<TEntity, bool>> ExpressionBody { get; set; }
        public XYValueCompare()
        {
        }
        /// <summary>
        /// Combines the first predicate with the second using the logical "and".
        /// </summary>
        public XYValueCompare<TEntity> AndAlso(bool combineWhenTrue, Expression<Func<TEntity, bool>> filter)
        {
            return Combine(combineWhenTrue, filter, Expression.AndAlso);
        }
        /// <summary>
        /// Combines the first predicate with the second using the logical "or".
        /// </summary>
        public XYValueCompare<TEntity> OrElse(bool combineWhenTrue, Expression<Func<TEntity, bool>> filter)
        {
            return Combine(combineWhenTrue, filter, Expression.OrElse);
        }

        /// <summary>
        /// Negates the predicate.
        /// </summary>
        public XYValueCompare<TEntity> Not()
        {
            var negated = Expression.Not(ExpressionBody.Body);
            ExpressionBody = Expression.Lambda<Func<TEntity, bool>>(negated, ExpressionBody.Parameters);
            return this;
        }
        public XYValueCompare<TEntity> Combine(Expression<Func<TEntity, bool>> filter, Func<Expression, Expression, Expression> merge)
        {
            if (ExpressionBody == null)
            {
                ExpressionBody = XYPredicateBuilder.Create(filter);
                return this;
            }
            ExpressionBody = ExpressionBody.XYCompose(filter, merge);

            return this;
        }
        public XYValueCompare<TEntity> Combine(bool combineWhenTrue, Expression<Func<TEntity, bool>> filter, Func<Expression, Expression, Expression> merge)
        {
            if (!combineWhenTrue)
            {
                return this;
            }
            return Combine(filter, merge);
        }
    }
    public static class XYPredicateBuilder
    {
        /// <summary>
        /// Creates a predicate expression from the specified lambda expression.
        /// </summary>
        public static Expression<Func<T, bool>> Create<T>(Expression<Func<T, bool>> predicate) { return predicate; }

        /// <summary>
        /// Combines the first expression with the second using the specified merge function.
        /// </summary>
        public static Expression<T> XYCompose<T>(this Expression<T> first, Expression<T> second, Func<Expression, Expression, Expression> merge)
        {
            // zip parameters (map from parameters of second to parameters of first)
            var map = first.Parameters
                .Select((f, i) => new { f, s = second.Parameters[i] })
                .ToDictionary(p => p.s, p => p.f);

            // replace parameters in the second lambda expression with the parameters in the first
            var secondBody = XYParameterRebinder.ReplaceParameters(map, second.Body);

            // create a merged lambda expression with parameters from the first expression
            return Expression.Lambda<T>(merge(first.Body, secondBody), first.Parameters);
        }

        /// <summary>
        /// ParameterRebinder
        /// </summary>
        class XYParameterRebinder : ExpressionVisitor
        {
            /// <summary>
            /// The ParameterExpression map
            /// </summary>
            readonly Dictionary<ParameterExpression, ParameterExpression> map;

            /// <summary>
            /// Initializes a new instance of the <see cref="XYParameterRebinder"/> class.
            /// </summary>
            /// <param name="map">The map.</param>
            XYParameterRebinder(Dictionary<ParameterExpression, ParameterExpression> map)
            {
                this.map = map ?? new Dictionary<ParameterExpression, ParameterExpression>();
            }

            /// <summary>
            /// Replaces the parameters.
            /// </summary>
            /// <param name="map">The map.</param>
            /// <param name="exp">The exp.</param>
            /// <returns>Expression</returns>
            public static Expression ReplaceParameters(Dictionary<ParameterExpression, ParameterExpression> map, Expression exp)
            {
                return new XYParameterRebinder(map).Visit(exp);
            }

            /// <summary>
            /// Visits the parameter.
            /// </summary>
            /// <param name="p">The p.</param>
            /// <returns>Expression</returns>
            protected override Expression VisitParameter(ParameterExpression p)
            {
                ParameterExpression replacement;

                if (map.TryGetValue(p, out replacement))
                {
                    p = replacement;
                }

                return base.VisitParameter(p);
            }
        }
    }
}
