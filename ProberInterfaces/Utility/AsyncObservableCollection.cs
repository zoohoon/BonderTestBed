using LogModule;
using System;
using System.Collections.Generic;

namespace ProberInterfaces
{

    using System.Threading;
    using System.Collections.Specialized;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.Serialization;
    using System.Threading.Tasks;

    public interface IAsyncContext
    {
        /// 
        /// Get the context of the creator thread
        /// 
        SynchronizationContext AsynchronizationContext { get; }

        /// 
        /// Test if the current executing thread is the creator thread
        /// 
        bool IsAsyncCreatorThread { get; }

        /// 
        /// Post a call to the specified method on the creator thread
        /// 
        /// Method that is to be called
        /// Method parameter/state
        void AsyncPost(SendOrPostCallback callback, object state);
    }

    [Serializable, DataContract]
    public class AsyncContext : IAsyncContext
    {
        private readonly SynchronizationContext _asynchronizationContext;

        /// 
        /// Constructor - Save the context of the creator/current thread
        /// 
        public AsyncContext()
        {
            _asynchronizationContext = SynchronizationContext.Current;
        }

        /// 
        /// Get the context of the creator thread
        /// 
        public SynchronizationContext AsynchronizationContext
        {
            get { return _asynchronizationContext; }
        }

        /// 
        /// Test if the current executing thread is the creator thread
        /// 
        public bool IsAsyncCreatorThread
        {
            get { return SynchronizationContext.Current == AsynchronizationContext; }
        }

        /// 
        /// Post a call to the specified method on the creator thread
        /// 
        /// Method that is to be called
        /// Method parameter/state
        public void AsyncPost(SendOrPostCallback callback, object state)
        {
            try
            {
                if (IsAsyncCreatorThread)
                {
                    callback(state); // Call the method directly
                }
                else
                {
                    AsynchronizationContext?.Post(callback, state);  // Post on creator thread
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    //public class ValueObjectOfTConverter : JsonConverter
    //{
    //    private static readonly Type ValueObjectGenericType = typeof(ValueObject<>);
    //    private static readonly string ValuePropertyName = nameof(ValueObject<object>.Value);

    //    public override bool CanConvert(Type objectType) =>
    //        IsSubclassOfGenericType(objectType, ValueObjectGenericType);

    //    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    //    {
    //        // converts "f5ce21a5-a0d1-4888-8d22-6f484794ac7c" => "value": "f5ce21a5-a0d1-4888-8d22-6f484794ac7c"
    //        var existingJsonWrappedInValueProperty = new JObject(new JProperty(ValuePropertyName, JToken.Load(reader)));
    //        return existingJsonWrappedInValueProperty.ToObject(objectType, serializer);
    //    }

    //    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    //    {
    //        // to implement
    //    }

    //    private static bool IsSubclassOfGenericType(Type typeToCheck, Type openGenericType)
    //    {
    //        while (typeToCheck != null && typeToCheck != typeof(object))
    //        {
    //            var cur = typeToCheck.IsGenericType ? typeToCheck.GetGenericTypeDefinition() : typeToCheck;
    //            if (openGenericType == cur) return true;

    //            typeToCheck = typeToCheck.BaseType;
    //        }

    //        return false;
    //    }
    //}


    [Serializable, DataContract]
    public class AsyncObservableCollection<T> : ObservableCollection<T>, IAsyncContext
    {
        private readonly AsyncContext _asyncContext = new AsyncContext();

        #region IAsyncContext Members
        public SynchronizationContext AsynchronizationContext { get { return _asyncContext.AsynchronizationContext; } }
        public bool IsAsyncCreatorThread { get { return _asyncContext.IsAsyncCreatorThread; } }
        public void AsyncPost(SendOrPostCallback callback, object state) { _asyncContext.AsyncPost(callback, state); }
        #endregion

        public AsyncObservableCollection() { }

        public AsyncObservableCollection(IEnumerable<T> list) : base(list) { }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            AsyncPost(RaiseCollectionChanged, e);
        }

        private void RaiseCollectionChanged(object param)
        {
            base.OnCollectionChanged((NotifyCollectionChangedEventArgs)param);
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            AsyncPost(RaisePropertyChanged, e);
        }

        private void RaisePropertyChanged(object param)
        {
            base.OnPropertyChanged((PropertyChangedEventArgs)param);
        }
        public async Task<bool> RemoveAsync(T item)
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

            this.CollectionChanged += OnCollectionChanged;

            bool removed = this.Remove(item);

            if (!removed)
            {
                this.CollectionChanged -= OnCollectionChanged;
                tcs.SetResult(false);
            }

            return await tcs.Task;

            void OnCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
            {
                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove && e.OldItems.Contains(item))
                {
                    this.CollectionChanged -= OnCollectionChanged;
                    tcs.SetResult(true);
                }
            }
        }
        public async Task<bool> AddAsync(T item)
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

            this.CollectionChanged += OnCollectionChanged;

            this.Add(item);

            return await tcs.Task;

            void OnCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
            {
                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add && e.NewItems.Contains(item))
                {
                    this.CollectionChanged -= OnCollectionChanged;
                    tcs.SetResult(true);
                }
            }
        }
    }
}
