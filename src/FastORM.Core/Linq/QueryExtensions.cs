using System.Linq.Expressions;

namespace FastORM;

public static class QueryExtensions
{
    public static CompilableQuery<T> AsCompilable<T>(this IQueryable<T> source)
    {
        if (source is FastOrmQueryable<T> foq) return new CompilableQuery<T>(foq.Context, foq.TableName);
        throw new NotSupportedException("FastORM: AsCompilable requires FastORM IQueryable root");
    }

    public static bool Any<T>(this IQueryable<T> source)
    {
        throw new NotSupportedException("FastORM: Any must be intercepted by source generator");
    }

    public static bool Any<T>(this CompilableQuery<T> source)
    {
        throw new NotSupportedException("FastORM: Any must be intercepted by source generator");
    }

    public static bool Any<T>(this IQueryable<T> source, Expression<Func<T, bool>> predicate)
    {
        throw new NotSupportedException("FastORM: Any must be intercepted by source generator");
    }

    public static bool Any<T>(this CompilableQuery<T> source, Expression<Func<T, bool>> predicate)
    {
        throw new NotSupportedException("FastORM: Any must be intercepted by source generator");
    }

    public static Task<bool> AnyAsync<T>(this IQueryable<T> source)
    {
        throw new NotSupportedException("FastORM: AnyAsync must be intercepted by source generator");
    }

    public static Task<bool> AnyAsync<T>(this CompilableQuery<T> source)
    {
        throw new NotSupportedException("FastORM: AnyAsync must be intercepted by source generator");
    }

    public static Task<bool> AnyAsync<T>(this IQueryable<T> source, Expression<Func<T, bool>> predicate)
    {
        throw new NotSupportedException("FastORM: AnyAsync must be intercepted by source generator");
    }

    public static Task<bool> AnyAsync<T>(this CompilableQuery<T> source, Expression<Func<T, bool>> predicate)
    {
        throw new NotSupportedException("FastORM: AnyAsync must be intercepted by source generator");
    }

    public static bool All<T>(this IQueryable<T> source, Expression<Func<T, bool>> predicate)
    {
        throw new NotSupportedException("FastORM: All must be intercepted by source generator");
    }

    public static bool All<T>(this CompilableQuery<T> source, Expression<Func<T, bool>> predicate)
    {
        throw new NotSupportedException("FastORM: All must be intercepted by source generator");
    }

    public static Task<bool> AllAsync<T>(this IQueryable<T> source, Expression<Func<T, bool>> predicate)
    {
        throw new NotSupportedException("FastORM: AllAsync must be intercepted by source generator");
    }

    public static Task<bool> AllAsync<T>(this CompilableQuery<T> source, Expression<Func<T, bool>> predicate)
    {
        throw new NotSupportedException("FastORM: AllAsync must be intercepted by source generator");
    }

    public static CompilableQuery<T> AsCompilable<T>(this CompilableQuery<T> source)
    {
        return source;
    }

    // REMOVED: Custom Where/OrderBy implementations that accept Func<T, bool>
    // We now rely on System.Linq.Queryable extensions which build Expression Trees.
    // This allows ValueExtractor to work correctly at runtime.

    public static CompilableQuery<T> Where<T>(this CompilableQuery<T> source, Func<T, bool> predicate)
    {
        return source;
    }

    public static CompilableQuery<T> OrderBy<T, TKey>(this CompilableQuery<T> source, Func<T, TKey> keySelector)
    {
        return source;
    }

    public static CompilableQuery<T> OrderByDescending<T, TKey>(this CompilableQuery<T> source, Func<T, TKey> keySelector)
    {
        return source;
    }

    // REMOVED: Select/Take/Skip/Distinct for IQueryable to support Expression Tree building via System.Linq.Queryable

    public static CompilableQuery<TResult> Select<T, TResult>(this CompilableQuery<T> source, Func<T, TResult> selector)
    {
        return new CompilableQuery<TResult>(source.Context, source.TableName);
    }

    public static CompilableQuery<T> Take<T>(this CompilableQuery<T> source, int count)
    {
        return source;
    }

    public static CompilableQuery<T> Skip<T>(this CompilableQuery<T> source, int count)
    {
        return source;
    }

    public static CompilableQuery<T> Distinct<T>(this CompilableQuery<T> source)
    {
        return source;
    }

    public static List<T> ToList<T>(this CompilableQuery<T> source)
    {
        throw new NotSupportedException("FastORM: ToList must be intercepted by source generator");
    }

    public static List<T> ToList<T>(this IQueryable<T> source)
    {
        throw new NotSupportedException("FastORM: ToList must be intercepted by source generator");
    }

    public static Task<List<T>> ToListAsync<T>(this CompilableQuery<T> source)
    {
        throw new NotSupportedException("FastORM: ToListAsync must be intercepted by source generator");
    }

    public static Task<List<T>> ToListAsync<T>(this IQueryable<T> source)
    {
        throw new NotSupportedException("FastORM: ToListAsync must be intercepted by source generator");
    }

    public static T FirstOrDefault<T>(this CompilableQuery<T> source)
    {
        throw new NotSupportedException("FastORM: FirstOrDefault must be intercepted by source generator");
    }

    public static T FirstOrDefault<T>(this IQueryable<T> source)
    {
        throw new NotSupportedException("FastORM: FirstOrDefault must be intercepted by source generator");
    }

    public static Task<T> FirstOrDefaultAsync<T>(this CompilableQuery<T> source)
    {
        throw new NotSupportedException("FastORM: FirstOrDefaultAsync must be intercepted by source generator");
    }

    public static Task<T> FirstOrDefaultAsync<T>(this IQueryable<T> source)
    {
        throw new NotSupportedException("FastORM: FirstOrDefaultAsync must be intercepted by source generator");
    }

    public static int Count<T>(this IQueryable<T> source)
    {
        throw new NotSupportedException("FastORM: Count must be intercepted by source generator");
    }

    public static Task<int> CountAsync<T>(this IQueryable<T> source)
    {
        throw new NotSupportedException("FastORM: CountAsync must be intercepted by source generator");
    }

    public static int Count<T>(this CompilableQuery<T> source)
    {
        throw new NotSupportedException("FastORM: Count must be intercepted by source generator");
    }

    public static Task<int> CountAsync<T>(this CompilableQuery<T> source)
    {
        throw new NotSupportedException("FastORM: CountAsync must be intercepted by source generator");
    }

    public static int Delete<T>(this IQueryable<T> source)
    {
        throw new NotSupportedException("FastORM: Delete must be intercepted by source generator");
    }

    public static Task<int> DeleteAsync<T>(this IQueryable<T> source)
    {
        throw new NotSupportedException("FastORM: DeleteAsync must be intercepted by source generator");
    }

    public static int Delete<T>(this CompilableQuery<T> source)
    {
        throw new NotSupportedException("FastORM: Delete must be intercepted by source generator");
    }

    public static Task<int> DeleteAsync<T>(this CompilableQuery<T> source)
    {
        throw new NotSupportedException("FastORM: DeleteAsync must be intercepted by source generator");
    }

    public static int Update<T>(this IQueryable<T> source, Action<T> updateAction)
    {
        throw new NotSupportedException("FastORM: Update must be intercepted by source generator");
    }

    public static Task<int> UpdateAsync<T>(this IQueryable<T> source, Action<T> updateAction)
    {
        throw new NotSupportedException("FastORM: UpdateAsync must be intercepted by source generator");
    }

    public static int Update<T>(this CompilableQuery<T> source, Action<T> updateAction)
    {
        throw new NotSupportedException("FastORM: Update must be intercepted by source generator");
    }

    public static Task<int> UpdateAsync<T>(this CompilableQuery<T> source, Action<T> updateAction)
    {
        throw new NotSupportedException("FastORM: UpdateAsync must be intercepted by source generator");
    }

    public static TKey Max<T, TKey>(this IQueryable<T> source, Func<T, TKey> selector)
    {
        throw new NotSupportedException("FastORM: Max must be intercepted by source generator");
    }

    public static TKey Max<T, TKey>(this CompilableQuery<T> source, Func<T, TKey> selector)
    {
        throw new NotSupportedException("FastORM: Max must be intercepted by source generator");
    }

    public static Task<TKey> MaxAsync<T, TKey>(this IQueryable<T> source, Func<T, TKey> selector)
    {
        throw new NotSupportedException("FastORM: MaxAsync must be intercepted by source generator");
    }

    public static Task<TKey> MaxAsync<T, TKey>(this CompilableQuery<T> source, Func<T, TKey> selector)
    {
        throw new NotSupportedException("FastORM: MaxAsync must be intercepted by source generator");
    }

    public static TKey Min<T, TKey>(this IQueryable<T> source, Func<T, TKey> selector)
    {
        throw new NotSupportedException("FastORM: Min must be intercepted by source generator");
    }

    public static TKey Min<T, TKey>(this CompilableQuery<T> source, Func<T, TKey> selector)
    {
        throw new NotSupportedException("FastORM: Min must be intercepted by source generator");
    }

    public static Task<TKey> MinAsync<T, TKey>(this IQueryable<T> source, Func<T, TKey> selector)
    {
        throw new NotSupportedException("FastORM: MinAsync must be intercepted by source generator");
    }

    public static Task<TKey> MinAsync<T, TKey>(this CompilableQuery<T> source, Func<T, TKey> selector)
    {
        throw new NotSupportedException("FastORM: MinAsync must be intercepted by source generator");
    }

    public static double Average<T>(this IQueryable<T> source, Func<T, int> selector)
    {
        throw new NotSupportedException("FastORM: Average must be intercepted by source generator");
    }

    public static double Average<T>(this CompilableQuery<T> source, Func<T, int> selector)
    {
        throw new NotSupportedException("FastORM: Average must be intercepted by source generator");
    }

    public static double Average<T>(this IQueryable<T> source, Func<T, long> selector)
    {
        throw new NotSupportedException("FastORM: Average must be intercepted by source generator");
    }

    public static double Average<T>(this CompilableQuery<T> source, Func<T, long> selector)
    {
        throw new NotSupportedException("FastORM: Average must be intercepted by source generator");
    }

    public static double Average<T>(this IQueryable<T> source, Func<T, double> selector)
    {
        throw new NotSupportedException("FastORM: Average must be intercepted by source generator");
    }

    public static double Average<T>(this CompilableQuery<T> source, Func<T, double> selector)
    {
        throw new NotSupportedException("FastORM: Average must be intercepted by source generator");
    }

    public static decimal Average<T>(this IQueryable<T> source, Func<T, decimal> selector)
    {
        throw new NotSupportedException("FastORM: Average must be intercepted by source generator");
    }

    public static decimal Average<T>(this CompilableQuery<T> source, Func<T, decimal> selector)
    {
        throw new NotSupportedException("FastORM: Average must be intercepted by source generator");
    }

    public static Task<double> AverageAsync<T>(this IQueryable<T> source, Func<T, int> selector)
    {
        throw new NotSupportedException("FastORM: AverageAsync must be intercepted by source generator");
    }

    public static Task<double> AverageAsync<T>(this CompilableQuery<T> source, Func<T, int> selector)
    {
        throw new NotSupportedException("FastORM: AverageAsync must be intercepted by source generator");
    }

    public static Task<double> AverageAsync<T>(this IQueryable<T> source, Func<T, long> selector)
    {
        throw new NotSupportedException("FastORM: AverageAsync must be intercepted by source generator");
    }

    public static Task<double> AverageAsync<T>(this CompilableQuery<T> source, Func<T, long> selector)
    {
        throw new NotSupportedException("FastORM: AverageAsync must be intercepted by source generator");
    }

    public static Task<double> AverageAsync<T>(this IQueryable<T> source, Func<T, double> selector)
    {
        throw new NotSupportedException("FastORM: AverageAsync must be intercepted by source generator");
    }

    public static Task<double> AverageAsync<T>(this CompilableQuery<T> source, Func<T, double> selector)
    {
        throw new NotSupportedException("FastORM: AverageAsync must be intercepted by source generator");
    }

    public static Task<decimal> AverageAsync<T>(this IQueryable<T> source, Func<T, decimal> selector)
    {
        throw new NotSupportedException("FastORM: AverageAsync must be intercepted by source generator");
    }

    public static Task<decimal> AverageAsync<T>(this CompilableQuery<T> source, Func<T, decimal> selector)
    {
        throw new NotSupportedException("FastORM: AverageAsync must be intercepted by source generator");
    }

    public static int Sum<T>(this IQueryable<T> source, Func<T, int> selector)
    {
        throw new NotSupportedException("FastORM: Sum must be intercepted by source generator");
    }

    public static int Sum<T>(this CompilableQuery<T> source, Func<T, int> selector)
    {
        throw new NotSupportedException("FastORM: Sum must be intercepted by source generator");
    }

    public static long Sum<T>(this IQueryable<T> source, Func<T, long> selector)
    {
        throw new NotSupportedException("FastORM: Sum must be intercepted by source generator");
    }

    public static long Sum<T>(this CompilableQuery<T> source, Func<T, long> selector)
    {
        throw new NotSupportedException("FastORM: Sum must be intercepted by source generator");
    }

    public static double Sum<T>(this IQueryable<T> source, Func<T, double> selector)
    {
        throw new NotSupportedException("FastORM: Sum must be intercepted by source generator");
    }

    public static double Sum<T>(this CompilableQuery<T> source, Func<T, double> selector)
    {
        throw new NotSupportedException("FastORM: Sum must be intercepted by source generator");
    }

    public static decimal Sum<T>(this IQueryable<T> source, Func<T, decimal> selector)
    {
        throw new NotSupportedException("FastORM: Sum must be intercepted by source generator");
    }

    public static decimal Sum<T>(this CompilableQuery<T> source, Func<T, decimal> selector)
    {
        throw new NotSupportedException("FastORM: Sum must be intercepted by source generator");
    }

    public static Task<int> SumAsync<T>(this IQueryable<T> source, Func<T, int> selector)
    {
        throw new NotSupportedException("FastORM: SumAsync must be intercepted by source generator");
    }

    public static Task<int> SumAsync<T>(this CompilableQuery<T> source, Func<T, int> selector)
    {
        throw new NotSupportedException("FastORM: SumAsync must be intercepted by source generator");
    }

    public static Task<long> SumAsync<T>(this IQueryable<T> source, Func<T, long> selector)
    {
        throw new NotSupportedException("FastORM: SumAsync must be intercepted by source generator");
    }

    public static Task<long> SumAsync<T>(this CompilableQuery<T> source, Func<T, long> selector)
    {
        throw new NotSupportedException("FastORM: SumAsync must be intercepted by source generator");
    }

    public static Task<double> SumAsync<T>(this IQueryable<T> source, Func<T, double> selector)
    {
        throw new NotSupportedException("FastORM: SumAsync must be intercepted by source generator");
    }

    public static Task<double> SumAsync<T>(this CompilableQuery<T> source, Func<T, double> selector)
    {
        throw new NotSupportedException("FastORM: SumAsync must be intercepted by source generator");
    }

    public static Task<decimal> SumAsync<T>(this IQueryable<T> source, Func<T, decimal> selector)
    {
        throw new NotSupportedException("FastORM: SumAsync must be intercepted by source generator");
    }

    public static Task<decimal> SumAsync<T>(this CompilableQuery<T> source, Func<T, decimal> selector)
    {
        throw new NotSupportedException("FastORM: SumAsync must be intercepted by source generator");
    }

    public static CompilableQuery<TResult> Join<TOuter, TInner, TKey, TResult>(
        this CompilableQuery<TOuter> outer,
        IQueryable<TInner> inner,
        Func<TOuter, TKey> outerKeySelector,
        Func<TInner, TKey> innerKeySelector,
        Func<TOuter, TInner, TResult> resultSelector)
    {
        return new CompilableQuery<TResult>(outer.Context, outer.TableName);
    }
    public static IQueryable<TResult> Join<TOuter, TInner, TKey, TResult>(
        this IQueryable<TOuter> outer,
        IQueryable<TInner> inner,
        Func<TOuter, TKey> outerKeySelector,
        Func<TInner, TKey> innerKeySelector,
        Func<TOuter, TInner, TResult> resultSelector)
    {
        if (outer is FastOrmQueryable<TOuter> fo) return new FastOrmQueryable<TResult>(fo.Context, fo.TableName);
        throw new NotSupportedException("FastORM: Join requires FastORM IQueryable root");
    }

    public static CompilableQuery<Group<T, TKey>> GroupBy<T, TKey>(this CompilableQuery<T> source, Func<T, TKey> keySelector)
    {
        return new CompilableQuery<Group<T, TKey>>(source.Context, source.TableName);
    }
    public static IQueryable<Group<T, TKey>> GroupBy<T, TKey>(this IQueryable<T> root, Func<T, TKey> keySelector)
    {
        if (root is FastOrmQueryable<T> fo) return new FastOrmQueryable<Group<T, TKey>>(fo.Context, fo.TableName);
        throw new NotSupportedException("FastORM: GroupBy requires FastORM IQueryable root");
    }

    public static CompilableQuery<TResult> LeftJoin<TOuter, TInner, TKey, TResult>(
        this CompilableQuery<TOuter> outer,
        IQueryable<TInner> inner,
        Func<TOuter, TKey> outerKeySelector,
        Func<TInner, TKey> innerKeySelector,
        Func<TOuter, TInner, TResult> resultSelector)
    {
        return new CompilableQuery<TResult>(outer.Context, outer.TableName);
    }
    public static IQueryable<TResult> LeftJoin<TOuter, TInner, TKey, TResult>(
        this IQueryable<TOuter> outer,
        IQueryable<TInner> inner,
        Func<TOuter, TKey> outerKeySelector,
        Func<TInner, TKey> innerKeySelector,
        Func<TOuter, TInner, TResult> resultSelector)
    {
        if (outer is FastOrmQueryable<TOuter> fo) return new FastOrmQueryable<TResult>(fo.Context, fo.TableName);
        throw new NotSupportedException("FastORM: LeftJoin requires FastORM IQueryable root");
    }

    public static CompilableQuery<TResult> RightJoin<TOuter, TInner, TKey, TResult>(
        this CompilableQuery<TOuter> outer,
        IQueryable<TInner> inner,
        Func<TOuter, TKey> outerKeySelector,
        Func<TInner, TKey> innerKeySelector,
        Func<TOuter, TInner, TResult> resultSelector)
    {
        return new CompilableQuery<TResult>(outer.Context, outer.TableName);
    }
    public static IQueryable<TResult> RightJoin<TOuter, TInner, TKey, TResult>(
        this IQueryable<TOuter> outer,
        IQueryable<TInner> inner,
        Func<TOuter, TKey> outerKeySelector,
        Func<TInner, TKey> innerKeySelector,
        Func<TOuter, TInner, TResult> resultSelector)
    {
        if (outer is FastOrmQueryable<TOuter> fo) return new FastOrmQueryable<TResult>(fo.Context, fo.TableName);
        throw new NotSupportedException("FastORM: RightJoin requires FastORM IQueryable root");
    }
}
