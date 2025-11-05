//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Collections.Specialized;
//using System.Linq;
//using System.Runtime.CompilerServices;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows.Threading;

//namespace LoaderBase
//{
//    [Serializable]
//    public class ThreadSafeCollection<T> : IList<T>, INotifyCollectionChanged
//    {
//        private IList<T> myCollection = new List<T>();

//        public event NotifyCollectionChangedEventHandler CollectionChanged;

//        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
//        {
//            if (CollectionChanged == null)
//                return;

//            foreach (NotifyCollectionChangedEventHandler handler in CollectionChanged.GetInvocationList())
//            {
//                var dispObj = handler.Target as DispatcherObject;
//                if (dispObj != null && dispObj.CheckAccess() == false)
//                {
//                    if (args.Action == NotifyCollectionChangedAction.Reset)
//                    {
//                        dispObj.Dispatcher.Invoke(DispatcherPriority.DataBind, handler, this, args);
//                    }
//                    else
//                    {
//                        dispObj.Dispatcher.BeginInvoke(DispatcherPriority.DataBind, handler, this, args);
//                    }
//                }
//                else
//                {
//                    handler(this, args);
//                }
//            }
//        }

//        [MethodImpl(MethodImplOptions.Synchronized)]
//        protected T Get(int index)
//        {
//            return myCollection[index];
//        }
//        [MethodImpl(MethodImplOptions.Synchronized)]
//        protected void Set(int index, T value)
//        {
//            if (myCollection.Count == 0 || myCollection.Count <= index)
//                return;
//            var item = myCollection[index];
//            myCollection[index] = value;
//            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, item, index));
//        }

//        [MethodImpl(MethodImplOptions.Synchronized)]
//        protected int GetCount()
//        {
//            return myCollection.Count;
//        }

//        public T this[int index] { get => Get(index); set => Set(index, value); }

//        public int Count => GetCount();

//        public bool IsReadOnly => myCollection.IsReadOnly;

//        [MethodImpl(MethodImplOptions.Synchronized)]
//        public void Add(T item)
//        {
//            myCollection.Add(item);

//            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
//        }

//        [MethodImpl(MethodImplOptions.Synchronized)]
//        public void Clear()
//        {
//            myCollection.Clear();

//            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
//        }

//        [MethodImpl(MethodImplOptions.Synchronized)]
//        public bool Contains(T item)
//        {
//            return myCollection.Contains(item);
//        }

//        [MethodImpl(MethodImplOptions.Synchronized)]
//        public void CopyTo(T[] array, int arrayIndex)
//        {
//            myCollection.CopyTo(array, arrayIndex);
//        }

//        [MethodImpl(MethodImplOptions.Synchronized)]
//        public IEnumerator<T> GetEnumerator()
//        {
//            return myCollection.GetEnumerator();
//        }

//        [MethodImpl(MethodImplOptions.Synchronized)]
//        public int IndexOf(T item)
//        {
//            return myCollection.IndexOf(item);
//        }

//        [MethodImpl(MethodImplOptions.Synchronized)]
//        public void Insert(int index, T item)
//        {
//            myCollection.Insert(index, item);
//            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
//        }

//        [MethodImpl(MethodImplOptions.Synchronized)]
//        public bool Remove(T item)
//        {
//            int index = myCollection.IndexOf(item);
//            if (index == -1)
//                return false;

//            bool rel = myCollection.Remove(item);
//            if(rel)
//                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
//            return rel;
//        }

//        [MethodImpl(MethodImplOptions.Synchronized)]
//        public void RemoveAt(int index)
//        {
//            if (myCollection.Count == 0 || myCollection.Count <= index)
//                return;

//            var item = myCollection[index];

//            myCollection.RemoveAt(index);
//            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
//        }

//        [MethodImpl(MethodImplOptions.Synchronized)]
//        IEnumerator IEnumerable.GetEnumerator()
//        {
//            return myCollection.GetEnumerator();
//        }
//    }

//}
