﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using EnumsNET;

namespace SJP.Schematic.Core.Utilities
{
    /// <summary>
    /// Flags controlling the behavior of <see cref="AsyncLazy{T}"/>.
    /// </summary>
    [Flags]
    public enum AsyncLazyFlags
    {
        /// <summary>
        /// No special flags. The factory method is executed on a thread pool thread, and does not retry initialization on failures (failures are cached).
        /// </summary>
        None = 0x0,

        /// <summary>
        /// Execute the factory method on the calling thread.
        /// </summary>
        ExecuteOnCallingThread = 0x1,

        /// <summary>
        /// If the factory method fails, then re-run the factory method the next time instead of caching the failed task.
        /// </summary>
        RetryOnFailure = 0x2,
    }

    /// <summary>
    /// Provides support for asynchronous lazy initialization. This type is fully threadsafe.
    /// </summary>
    /// <typeparam name="T">The type of object that is being asynchronously initialized.</typeparam>
    [DebuggerDisplay("Id = {Id}, State = {GetStateForDebugger}")]
    [DebuggerTypeProxy(typeof(AsyncLazy<>.DebugView))]
    public sealed class AsyncLazy<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncLazy&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="factory">The asynchronous delegate that is invoked to produce the value when it is needed. May not be <c>null</c>.</param>
        /// <param name="flags">Flags to influence async lazy semantics.</param>
        public AsyncLazy(Func<Task<T>> factory, AsyncLazyFlags flags = AsyncLazyFlags.None)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            if (!flags.IsValid())
                throw new ArgumentException($"The { nameof(AsyncLazyFlags) } provided must be a valid enum.", nameof(flags));
            if ((flags & AsyncLazyFlags.RetryOnFailure) == AsyncLazyFlags.RetryOnFailure)
                _factory = RetryOnFailure(_factory);
            if ((flags & AsyncLazyFlags.ExecuteOnCallingThread) != AsyncLazyFlags.ExecuteOnCallingThread)
                _factory = RunOnThreadPool(_factory);

            _instance = new Lazy<Task<T>>(_factory);
        }

        /// <summary>
        /// Starts the asynchronous initialization, if it has not already started.
        /// </summary>
        public void Start()
        {
            var unused = Task;
        }

        /// <summary>
        /// Whether the asynchronous factory method has started. This is initially <c>false</c> and becomes <c>true</c> when this instance is awaited or after <see cref="Start"/> is called.
        /// </summary>
        public bool IsStarted
        {
            get
            {
                lock (_mutex)
                    return _instance.IsValueCreated;
            }
        }

        /// <summary>
        /// Starts the asynchronous factory method, if it has not already started, and returns the resulting task.
        /// </summary>
        public Task<T> Task
        {
            get
            {
                lock (_mutex)
                    return _instance.Value;
            }
        }

        /// <summary>
        /// Asynchronous infrastructure support. This method permits instances of <see cref="AsyncLazy&lt;T&gt;"/> to be await'ed.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public TaskAwaiter<T> GetAwaiter()
        {
            return Task.GetAwaiter();
        }

        /// <summary>
        /// Asynchronous infrastructure support. This method permits instances of <see cref="AsyncLazy&lt;T&gt;"/> to be await'ed.
        /// </summary>
        /// <param name="continueOnCapturedContext"><c>true</c> to attempt to marshal the continuation back to the original context captured; otherwise, <c>false</c>.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public ConfiguredTaskAwaitable<T> ConfigureAwait(bool continueOnCapturedContext)
        {
            return Task.ConfigureAwait(continueOnCapturedContext);
        }

        [DebuggerNonUserCode]
        internal LazyState GetStateForDebugger
        {
            get
            {
                if (!_instance.IsValueCreated)
                    return LazyState.NotStarted;
                if (!_instance.Value.IsCompleted)
                    return LazyState.Executing;
                return LazyState.Completed;
            }
        }

        private Func<Task<T>> RetryOnFailure(Func<Task<T>> factory)
        {
            return async () =>
            {
                try
                {
                    return await factory().ConfigureAwait(false);
                }
                catch
                {
                    lock (_mutex)
                        _instance = new Lazy<Task<T>>(_factory);

                    throw;
                }
            };
        }

        private Func<Task<T>> RunOnThreadPool(Func<Task<T>> factory) => () => System.Threading.Tasks.Task.Run(factory);

        /// <summary>
        /// The underlying lazy task.
        /// </summary>
        private Lazy<Task<T>> _instance;

        /// <summary>
        /// The synchronization object protecting <c>_instance</c>.
        /// </summary>
        private readonly object _mutex = new object();

        /// <summary>
        /// The factory method to call.
        /// </summary>
        private readonly Func<Task<T>> _factory;

        internal enum LazyState
        {
            NotStarted,
            Executing,
            Completed
        }

        [DebuggerNonUserCode]
        internal sealed class DebugView
        {
            public DebugView(AsyncLazy<T> lazy)
            {
                _lazy = lazy;
            }

            public LazyState State => _lazy.GetStateForDebugger;

            public Task Task
            {
                get
                {
                    if (!_lazy._instance.IsValueCreated)
                        throw new InvalidOperationException("Not yet created.");
                    return _lazy._instance.Value;
                }
            }

            public T Value
            {
                get
                {
                    if (!_lazy._instance.IsValueCreated || !_lazy._instance.Value.IsCompleted)
                        throw new InvalidOperationException("Not yet created.");
                    return _lazy._instance.Value.Result;
                }
            }

            private readonly AsyncLazy<T> _lazy;
        }
    }
}