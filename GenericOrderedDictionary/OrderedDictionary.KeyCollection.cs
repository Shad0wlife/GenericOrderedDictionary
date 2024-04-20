using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GenericOrderedDictionary
{
    public partial class OrderedDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IDictionary, IOrderedDictionary, IReadOnlyDictionary<TKey, TValue>, ISerializable, IDeserializationCallback where TKey : notnull
    {
        [DebuggerTypeProxy(typeof(DictionaryKeyCollectionDebugView<,>))]
        [DebuggerDisplay("Count = {Count}")]
        public sealed class KeyCollection : ICollection<TKey>, ICollection, IReadOnlyCollection<TKey>
        {
            private readonly OrderedDictionary<TKey, TValue> _dictionary;

            public KeyCollection(OrderedDictionary<TKey, TValue> dictionary)
            {
                if (dictionary == null)
                {
                    throw new ArgumentNullException(nameof(dictionary));
                }

                _dictionary = dictionary;
            }

            public Enumerator GetEnumerator() => new Enumerator(_dictionary);

            public void CopyTo(TKey[] array, int index)
            {
                if (array == null)
                {
                    throw new ArgumentNullException(nameof(array));
                }

                if (index < 0 || index > array.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(index), "Index is negative or too large.");
                }

                if (array.Length - index < _dictionary.Count)
                {
                    throw new ArgumentException("ArrayPlusOffTooSmall");
                }

                int count = _dictionary._count;
                Entry[]? entries = _dictionary._entries;

                int cnt = 0;
                Entry entry = entries![_dictionary._first];
                while (entry.orderNext != 0)
                {
                    array[index++] = entry.key;
                    entry = entries![entry.orderNext - 1];
                    cnt++;
                }

                //SanityCheck
                Debug.Assert(cnt == _dictionary.Count, "The linked list of entries does not contain all entries.");
            }

            public int Count => _dictionary.Count;

            bool ICollection<TKey>.IsReadOnly => true;

            void ICollection<TKey>.Add(TKey item) => throw new NotSupportedException("Not Supported: KeyCollectionSet");


            void ICollection<TKey>.Clear() => throw new NotSupportedException("Not Supported: KeyCollectionSet");

            public bool Contains(TKey item) =>
                _dictionary.ContainsKey(item);

            bool ICollection<TKey>.Remove(TKey item)
            {
                throw new NotSupportedException("Not Supported: KeyCollectionSet");
            }

            IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator() =>
                Count == 0 ? SZGenericArrayEnumerator<TKey>.Empty :
                GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<TKey>)this).GetEnumerator();

            void ICollection.CopyTo(Array array, int index)
            {
                if (array == null)
                {
                    throw new ArgumentNullException(nameof(array));
                }

                if (array.Rank != 1)
                {
                    throw new ArgumentException("Multi Dimensional Array Not supported.");
                }

                if (array.GetLowerBound(0) != 0)
                {
                    throw new ArgumentException("Non Zero Lower Bound not supported.");
                }

                if ((uint)index > (uint)array.Length)
                {
                    throw new IndexOutOfRangeException($"Index {(uint)index} > array Length {(uint)array.Length}");
                }

                if (array.Length - index < _dictionary.Count)
                {
                    throw new ArgumentException("ArrayPlusOffTooSmall");
                }

                if (array is TKey[] keys)
                {
                    CopyTo(keys, index);
                }
                else
                {
                    object[]? objects = array as object[];
                    if (objects == null)
                    {
                        throw new ArgumentException("Incompatible Array Type!");
                    }

                    int count = _dictionary._count;
                    Entry[]? entries = _dictionary._entries;
                    try
                    {
                        int cnt = 0;
                        Entry entry = entries![_dictionary._first];
                        while (entry.orderNext != 0)
                        {
                            objects[index++] = entry.key;
                            entry = entries![entry.orderNext - 1];
                            cnt++;
                        }

                        //SanityCheck
                        Debug.Assert(cnt == _dictionary.Count, "The linked list of entries does not contain all entries.");
                    }
                    catch (ArrayTypeMismatchException)
                    {
                        throw new ArgumentException("Incompatible Array Type!");
                    }
                }
            }

            bool ICollection.IsSynchronized => false;

            object ICollection.SyncRoot => ((ICollection)_dictionary).SyncRoot;

            public struct Enumerator : IEnumerator<TKey>, IEnumerator
            {
                private readonly OrderedDictionary<TKey, TValue> _dictionary;
                private int _index;
                private readonly int _version;
                private TKey? _currentKey;

                internal Enumerator(OrderedDictionary<TKey, TValue> dictionary)
                {
                    _dictionary = dictionary;
                    _version = dictionary._version;
                    _index = _dictionary._first;
                    _currentKey = default;
                }

                public void Dispose() { }

                public bool MoveNext()
                {
                    if (_version != _dictionary._version)
                    {
                        throw new InvalidOperationException("Version Mismatch: The OrderedDictionary has been changed during enumeration.");
                    }

                    while (_index > -1)
                    {
                        ref Entry entry = ref _dictionary._entries![_index];

                        _currentKey = entry.key;
                        _index = (int)(entry.orderNext - 1);
                        return true;
                    }

                    _index = -2;
                    _currentKey = default;
                    return false;
                }

                public TKey Current => _currentKey!;

                object? IEnumerator.Current
                {
                    get
                    {
                        if (_index == -2 /* end */ || _index == _dictionary._first)
                        {
                            throw new InvalidOperationException("Enum Operation Cannot Happen.");
                        }

                        return _currentKey;
                    }
                }

                void IEnumerator.Reset()
                {
                    if (_version != _dictionary._version)
                    {
                        throw new InvalidOperationException("Version Mismatch: The OrderedDictionary has been changed during enumeration.");
                    }

                    _index = _dictionary._first;
                    _currentKey = default;
                }
            }
        }
    }
}
