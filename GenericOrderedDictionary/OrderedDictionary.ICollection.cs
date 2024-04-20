using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GenericOrderedDictionary
{
    public partial class OrderedDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IDictionary, IOrderedDictionary, IReadOnlyDictionary<TKey, TValue>, ISerializable, IDeserializationCallback where TKey : notnull
    {
        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> keyValuePair) =>
            Add(keyValuePair.Key, keyValuePair.Value);

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> keyValuePair)
        {
            ref TValue value = ref FindValue(keyValuePair.Key);
            if (!Unsafe.IsNullRef(ref value) && EqualityComparer<TValue>.Default.Equals(value, keyValuePair.Value))
            {
                return true;
            }

            return false;
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> keyValuePair)
        {
            ref TValue value = ref FindValue(keyValuePair.Key);
            if (!Unsafe.IsNullRef(ref value) && EqualityComparer<TValue>.Default.Equals(value, keyValuePair.Value))
            {
                Remove(keyValuePair.Key);
                return true;
            }

            return false;
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int index) =>
            CopyTo(array, index);

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

            if (array.Length - index < Count)
            {
                throw new ArgumentException("ArrayPlusOffTooSmall");
            }

            if (array is KeyValuePair<TKey, TValue>[] pairs)
            {
                CopyTo(pairs, index);
            }
            else if (array is DictionaryEntry[] dictEntryArray)
            {
                Entry[]? entries = _entries;

                int cnt = 0;
                Entry entry = entries![_first];
                while (entry.orderNext != 0)
                {
                    dictEntryArray[index++] = new DictionaryEntry(entry.key, entry.value);
                    entry = entries![entry.orderNext - 1];
                    cnt++;
                }

                //SanityCheck
                Debug.Assert(cnt == Count, "The linked list of entries does not contain all entries.");
            }
            else
            {
                object[]? objects = array as object[];
                if (objects == null)
                {
                    throw new ArgumentException("Incompatible Array Type!");
                }

                try
                {
                    int count = _count;
                    Entry[]? entries = _entries;

                    int cnt = 0;
                    Entry entry = entries![_first];
                    while (entry.orderNext != 0)
                    {
                        objects[index++] = new KeyValuePair<TKey, TValue>(entry.key, entry.value);
                        entry = entries![entry.orderNext - 1];
                        cnt++;
                    }

                    //SanityCheck
                    Debug.Assert(cnt == Count, "The linked list of entries does not contain all entries.");
                }
                catch (ArrayTypeMismatchException e)
                {
                    throw new ArgumentException("Incompatible Array Type!", e);
                }
            }
        }

        bool ICollection.IsSynchronized => false;

        object ICollection.SyncRoot => this;
    }
}
