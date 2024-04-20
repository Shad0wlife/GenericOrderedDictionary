using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GenericOrderedDictionary
{
    internal sealed class IDictionaryDebugView<TKey, TValue> where TKey : notnull
    {
        private readonly IDictionary<TKey, TValue> _dict;

        public IDictionaryDebugView(IDictionary<TKey, TValue> dictionary)
        {
            ArgumentNullException.ThrowIfNull(dictionary);

            _dict = dictionary;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public DebugViewDictionaryItem<TKey, TValue>[] Items
        {
            get
            {
                var keyValuePairs = new KeyValuePair<TKey, TValue>[_dict.Count];
                _dict.CopyTo(keyValuePairs, 0);
                var items = new DebugViewDictionaryItem<TKey, TValue>[keyValuePairs.Length];
                for (int i = 0; i < items.Length; i++)
                {
                    items[i] = new DebugViewDictionaryItem<TKey, TValue>(keyValuePairs[i]);
                }
                return items;
            }
        }
    }

    internal sealed class DictionaryKeyCollectionDebugView<TKey, TValue>
    {
        private readonly ICollection<TKey> _collection;

        public DictionaryKeyCollectionDebugView(ICollection<TKey> collection)
        {
            ArgumentNullException.ThrowIfNull(collection);

            _collection = collection;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public TKey[] Items
        {
            get
            {
                TKey[] items = new TKey[_collection.Count];
                _collection.CopyTo(items, 0);
                return items;
            }
        }
    }

    internal sealed class DictionaryValueCollectionDebugView<TKey, TValue>
    {
        private readonly ICollection<TValue> _collection;

        public DictionaryValueCollectionDebugView(ICollection<TValue> collection)
        {
            ArgumentNullException.ThrowIfNull(collection);

            _collection = collection;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public TValue[] Items
        {
            get
            {
                TValue[] items = new TValue[_collection.Count];
                _collection.CopyTo(items, 0);
                return items;
            }
        }
    }

    /// <summary>
    /// Defines a key/value pair for displaying an item of a dictionary by a debugger.
    /// </summary>
    [DebuggerDisplay("{Value}", Name = "[{Key}]")]
    internal readonly struct DebugViewDictionaryItem<TKey, TValue>
    {
        public DebugViewDictionaryItem(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }

        public DebugViewDictionaryItem(KeyValuePair<TKey, TValue> keyValue)
        {
            Key = keyValue.Key;
            Value = keyValue.Value;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
        public TKey Key { get; }

        [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
        public TValue Value { get; }
    }
}
