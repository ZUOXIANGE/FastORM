using System.Collections;

namespace FastORM;

public class Group<TSource, TKey> : IGrouping<TKey, TSource>
{
    public TKey Key { get; }

    public Group(TKey key)
    {
        Key = key;
    }

    public IEnumerator<TSource> GetEnumerator()
    {
        throw new NotImplementedException("FastORM: Group is for query generation only");
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
