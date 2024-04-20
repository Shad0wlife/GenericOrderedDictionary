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

        void IDictionary.Add(object key, object? value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (value == null && !(default(TValue) == null))
                throw new ArgumentException("value: Nulls are not allowed for this object.");

            try
            {
                TKey tempKey = (TKey)key;

                try
                {
                    Add(tempKey, (TValue)value!);
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

        bool IDictionary.Contains(object key)
        {
            if (IsCompatibleKey(key))
            {
                return ContainsKey((TKey)key);
            }

            return false;
        }

        IDictionaryEnumerator IDictionary.GetEnumerator() => new Enumerator(this, Enumerator.DictEntry);

        void IDictionary.Remove(object key)
        {
            if (IsCompatibleKey(key))
            {
                Remove((TKey)key);
            }
        }



        bool IDictionary.IsFixedSize => false;

        bool IDictionary.IsReadOnly => false;

        ICollection IDictionary.Keys => Keys;

        ICollection IDictionary.Values => Values;

        object? IDictionary.this[object key]
        {
            get
            {
                if (IsCompatibleKey(key))
                {
                    ref TValue value = ref FindValue((TKey)key);
                    if (!Unsafe.IsNullRef(ref value))
                    {
                        return value;
                    }
                }

                return null;
            }
            set
            {
                if (key == null)
                {
                    throw new ArgumentNullException(nameof(key));
                }

                if (value == null && !(default(TValue) == null))
                    throw new ArgumentException("value: Nulls are not allowed for this object.");

                try
                {
                    TKey tempKey = (TKey)key;
                    try
                    {
                        this[tempKey] = (TValue)value!;
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
        }
    }
}
