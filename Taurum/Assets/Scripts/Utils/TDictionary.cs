using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

[Serializable]
public class Dictionary<TKey1, TKey2, TValue> : Dictionary<Tuple<TKey1, TKey2>, TValue>, IDictionary<Tuple<TKey1, TKey2>, TValue>
{
    public Dictionary()
    {

    }

    public Dictionary(SerializationInfo info, StreamingContext context) : base(info, context)
    {

    }

    public TValue this[TKey1 key1, TKey2 key2]
    {
        get { return base[new Tuple<TKey1, TKey2>(key1, key2)]; }
        set { base[new Tuple<TKey1, TKey2>(key1, key2)] = value; }
    }

    public void Add(TKey1 key1, TKey2 key2, TValue value)
    {
        base.Add(new Tuple<TKey1, TKey2>(key1, key2), value);
    }

    public bool ContainsKey(TKey1 key1, TKey2 key2)
    {
        return base.ContainsKey(new Tuple<TKey1, TKey2>(key1, key2));
    }
}