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
        [DebuggerTypeProxy(typeof(DictionaryValueCollectionDebugView<,>))]
        [DebuggerDisplay("Count = {Count}")]
        public sealed class ValueCollection : ICollection<TValue>, ICollection, IReadOnlyCollection<TValue>
        {
            private readonly OrderedDictionary<TKey, TValue> _dictionary;

            public ValueCollection(OrderedDictionary<TKey, TValue> dictionary)
            {
                if (dictionary == null)
                {
                    throw new ArgumentNullException(nameof(dictionary));
                }

                _dictionary = dictionary;
            }

            public Enumerator GetEnumerator() => new Enumerator(_dictionary);

            public void CopyTo(TValue[] array, int index)
            {
                if (array == null)
                {
                    throw new ArgumentNullException(nameof(array));
                }

                if ((uint)index > array.Length)
                {
                    throw new IndexOutOfRangeException($"Index {(uint)index} > array Length {(uint)array.Length}");
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
                    array[index++] = entry.value;
                    entry = entries![entry.orderNext - 1];
                    cnt++;
                }

                //SanityCheck
                Debug.Assert(cnt == _dictionary.Count, "The linked list of entries does not contain all entries.");
            }

            public int Count => _dictionary.Count;

            bool ICollection<TValue>.IsReadOnly => true;

            void ICollection<TValue>.Add(TValue item) => throw new NotSupportedException("Not Supported: ValueCollectionSet");

            bool ICollection<TValue>.Remove(TValue item)
            {
                throw new NotSupportedException("Not Supported: ValueCollectionSet");
            }

            void ICollection<TValue>.Clear() => throw new NotSupportedException("Not Supported: ValueCollectionSet");

            bool ICollection<TValue>.Contains(TValue item) => _dictionary.ContainsValue(item);

            IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator() =>
                Count == 0 ? SZGenericArrayEnumerator<TValue>.Empty :
                GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<TValue>)this).GetEnumerator();

            void ICollection.CopyTo(Array array, int index)
            {
                if (array == null)
                {
                    throw new ArgumentNullException(nameof(array)); ;
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

                if (array is TValue[] values)
                {
                    CopyTo(values, index);
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
                            objects[index++] = entry.value!;
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

            public struct Enumerator : IEnumerator<TValue>, IEnumerator
            {
                private readonly OrderedDictionary<TKey, TValue> _dictionary;
                private int _index;
                private readonly int _version;
                private TValue? _currentValue;

                internal Enumerator(OrderedDictionary<TKey, TValue> dictionary)
                {
                    _dictionary = dictionary;
                    _version = dictionary._version;
                    _index = _dictionary._first;
                    _currentValue = default;
                }

                public void Dispose() { }

                public bool MoveNext()
                {
                    if (_version != _dictionary._version)
                    {
                        throw new InvalidOperationException("Enum Version failed.");
                    }

                    while (_index > -1)
                    {
                        ref Entry entry = ref _dictionary._entries![_index];

                        _currentValue = entry.value;
                        _index = (int)(entry.orderNext - 1);
                        return true;
                    }

                    _index = -2;
                    _currentValue = default;
                    return false;
                }

                public TValue Current => _currentValue!;

                object? IEnumerator.Current
                {
                    get
                    {
                        if (_index == -2 /* end */ || _index == _dictionary._first)
                        {
                            throw new InvalidOperationException("Enum Operation Cannot Happen.");
                        }

                        return _currentValue;
                    }
                }

                void IEnumerator.Reset()
                {
                    if (_version != _dictionary._version)
                    {
                        throw new InvalidOperationException("Enum Version failed.");
                    }

                    _index = _dictionary._first;
                    _currentValue = default;
                }
            }


        }
    }
}
