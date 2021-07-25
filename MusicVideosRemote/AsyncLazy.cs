namespace MusicVideosRemote
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    /// <summary>
    /// AsyncLazy class provides lazy async access to a singleton.
    /// </summary>
    /// <typeparam name="T">The class to access.</typeparam>
    public class AsyncLazy<T>
    {
        private readonly Lazy<Task<T>> instance;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncLazy{T}"/> class.
        /// </summary>
        /// <param name="factory"></param>
        public AsyncLazy(Func<T> factory)
        {
            instance = new Lazy<Task<T>>(() => Task.Run(factory));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncLazy{T}"/> class.
        /// </summary>
        /// <param name="factory"></param>
        public AsyncLazy(Func<Task<T>> factory)
        {
            instance = new Lazy<Task<T>>(() => Task.Run(factory));
        }

        public TaskAwaiter<T> GetAwaiter()
        {
            return instance.Value.GetAwaiter();
        }

        public void Start()
        {
            var unused = instance.Value;
        }
    }
}