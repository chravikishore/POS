﻿#region License

// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace Lib.JSON.Utilities
{
    internal class CollectionWrapper<T> : ICollection<T>, IWrappedCollection
    {
        private readonly IList _list;
        private readonly ICollection<T> _genericCollection;
        private object _syncRoot;
        
        public CollectionWrapper(IList list)
        {
            ValidationUtils.ArgumentNotNull(list, "list");
            
            if (list is ICollection<T>)
            {
                this._genericCollection = (ICollection<T>)list;
            }
            else
            {
                this._list = list;
            }
        }
        
        public CollectionWrapper(ICollection<T> list)
        {
            ValidationUtils.ArgumentNotNull(list, "list");

            this._genericCollection = list;
        }
        
        public virtual void Add(T item)
        {
            if (this._genericCollection != null)
            {
                this._genericCollection.Add(item);
            }
            else
            {
                this._list.Add(item);
            }
        }
        
        public virtual void Clear()
        {
            if (this._genericCollection != null)
            {
                this._genericCollection.Clear();
            }
            else
            {
                this._list.Clear();
            }
        }
        
        public virtual bool Contains(T item)
        {
            if (this._genericCollection != null)
            {
                return this._genericCollection.Contains(item);
            }
            else
            {
                return this._list.Contains(item);
            }
        }
        
        public virtual void CopyTo(T[] array, int arrayIndex)
        {
            if (this._genericCollection != null)
            {
                this._genericCollection.CopyTo(array, arrayIndex);
            }
            else
            {
                this._list.CopyTo(array, arrayIndex);
            }
        }
        
        public virtual int Count
        {
            get
            {
                if (this._genericCollection != null)
                {
                    return this._genericCollection.Count;
                }
                else
                {
                    return this._list.Count;
                }
            }
        }
        
        public virtual bool IsReadOnly
        {
            get
            {
                if (this._genericCollection != null)
                {
                    return this._genericCollection.IsReadOnly;
                }
                else
                {
                    return this._list.IsReadOnly;
                }
            }
        }
        
        public virtual bool Remove(T item)
        {
            if (this._genericCollection != null)
            {
                return this._genericCollection.Remove(item);
            }
            else
            {
                bool contains = this._list.Contains(item);
                
                if (contains)
                {
                    this._list.Remove(item);
                }
                
                return contains;
            }
        }
        
        public virtual IEnumerator<T> GetEnumerator()
        {
            if (this._genericCollection != null)
            {
                return this._genericCollection.GetEnumerator();
            }

            return this._list.Cast<T>().GetEnumerator();
        }
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            if (this._genericCollection != null)
            {
                return this._genericCollection.GetEnumerator();
            }
            else
            {
                return this._list.GetEnumerator();
            }
        }
        
        int IList.Add(object value)
        {
            VerifyValueType(value);
            this.Add((T)value);

            return (this.Count - 1);
        }
        
        bool IList.Contains(object value)
        {
            if (IsCompatibleObject(value))
            {
                return this.Contains((T)value);
            }

            return false;
        }
        
        int IList.IndexOf(object value)
        {
            if (this._genericCollection != null)
            {
                throw new Exception("Wrapped ICollection<T> does not support IndexOf.");
            }
            
            if (IsCompatibleObject(value))
            {
                return this._list.IndexOf((T)value);
            }

            return -1;
        }
        
        void IList.RemoveAt(int index)
        {
            if (this._genericCollection != null)
            {
                throw new Exception("Wrapped ICollection<T> does not support RemoveAt.");
            }

            this._list.RemoveAt(index);
        }
        
        void IList.Insert(int index, object value)
        {
            if (this._genericCollection != null)
            {
                throw new Exception("Wrapped ICollection<T> does not support Insert.");
            }
            
            VerifyValueType(value);
            this._list.Insert(index, (T)value);
        }
        
        bool IList.IsFixedSize
        {
            get
            {
                if (this._genericCollection != null)
                {
                    // ICollection<T> only has IsReadOnly
                    return this._genericCollection.IsReadOnly;
                }
                else
                {
                    return this._list.IsFixedSize;
                }
            }
        }
        
        void IList.Remove(object value)
        {
            if (IsCompatibleObject(value))
            {
                this.Remove((T)value);
            }
        }
        
        object IList.this[int index]
        {
            get
            {
                if (this._genericCollection != null)
                {
                    throw new Exception("Wrapped ICollection<T> does not support indexer.");
                }
                
                return this._list[index];
            }
            set
            {
                if (this._genericCollection != null)
                {
                    throw new Exception("Wrapped ICollection<T> does not support indexer.");
                }
                
                VerifyValueType(value);
                this._list[index] = (T)value;
            }
        }
        
        void ICollection.CopyTo(Array array, int arrayIndex)
        {
            this.CopyTo((T[])array, arrayIndex);
        }
        
        bool ICollection.IsSynchronized
        {
            get
            {
                return false;
            }
        }
        
        object ICollection.SyncRoot
        {
            get
            {
                if (this._syncRoot == null)
                {
                    Interlocked.CompareExchange(ref this._syncRoot, new object(), null);
                }
                
                return this._syncRoot;
            }
        }
        
        private static void VerifyValueType(object value)
        {
            if (!IsCompatibleObject(value))
            {
                throw new ArgumentException("The value '{0}' is not of type '{1}' and cannot be used in this generic collection.".FormatWith(CultureInfo.InvariantCulture, value, typeof(T)), "value");
            }
        }
        
        private static bool IsCompatibleObject(object value)
        {
            if (!(value is T) && (value != null || (typeof(T).IsValueType && !ReflectionUtils.IsNullableType(typeof(T)))))
            {
                return false;
            }

            return true;
        }
        
        public object UnderlyingCollection
        {
            get
            {
                if (this._genericCollection != null)
                {
                    return this._genericCollection;
                }
                else
                {
                    return this._list;
                }
            }
        }
    }
}