using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GenericOrderedDictionary
{
    public partial class OrderedDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IDictionary, IOrderedDictionary, IReadOnlyDictionary<TKey, TValue>, ISerializable, IDeserializationCallback where TKey : notnull
    {


        object? IOrderedDictionary.this[int index]
        {
            get
            {
                if(typeof(TKey) == typeof(int))
                {
                    //Hack to make this[int] work when TKey is int
                    return GetByIntegerKey((TKey)(object)index);
                }
                else
                {
                    return GetValueByIndex(index);
                }
            }
            set
            {
                throw new NotImplementedException("Undefined behaviour.");
            }
        }

        public TKey GetKeyByIndex(int index)
        {
            if (index >= Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            int arrIdx = _first;
            ref Entry entry = ref Unsafe.NullRef<Entry>();

            //Cannot bisect, since it's a linked list
            for (int cnt = 0; cnt <= index; cnt++)
            {
                entry = ref _entries![arrIdx];
                arrIdx = ((int)entry.orderNext) - 1;
            }

            return entry.key;
        }

        public TValue GetValueByIndex(int index)
        {
            return this[GetKeyByIndex(index)];
        }

        /// <summary>
        /// Used for redirecting this[int] when TKey is int
        /// </summary>
        /// <param name="intKey"></param>
        /// <returns></returns>
        public TValue GetByIntegerKey(TKey intKey)
        {
            return this[intKey];
        }



        void IOrderedDictionary.Insert(int index, object key, object? value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (value == null && !(default(TValue) == null))
                throw new ArgumentException("value: Nulls are not allowed for this object.");

            if (index > Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            try
            {
                TKey tempKey = (TKey)key;

                try
                {

                    if(index == Count)
                    {
                        //Append
                        Add(tempKey, (TValue)value!);
                    }
                    else
                    {

                        int arrIdx = _first;
                        ref Entry entry = ref Unsafe.NullRef<Entry>(); ;

                        //Cannot bisect, since it's a linked list
                        for (int cnt = 0; cnt <= index; cnt++)
                        {
                            entry = ref _entries![arrIdx];
                            arrIdx = ((int)entry.orderNext) - 1;
                        }

                        //Not At end of List
                        TryInsertBefore(tempKey, (TValue)value!, entry.key, InsertionBehavior.ThrowOnExisting);
                    }

                }
                catch (InvalidCastException)
                {
                    throw new ArgumentException($"value is of Type {value?.GetType()} but {typeof(TValue)} is needed.");
                }
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException($"key is of Type {key.GetType()} but {typeof(TKey)} is needed.");
            }
        }

        void IOrderedDictionary.RemoveAt(int index)
        {
            Remove(GetKeyByIndex(index));
        }

        IDictionaryEnumerator IOrderedDictionary.GetEnumerator() => new Enumerator(this, Enumerator.DictEntry);
    }
}
