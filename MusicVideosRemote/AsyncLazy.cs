namespace MusicVideosRemote
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    public class AsyncLazy<T>
    {
        readonly Lazy<Task<T>> instance;

        public AsyncLazy(Func<T> factory)
        {
            instance = new Lazy<Task<T>>(() => Task.Run(factory));
        }

        public AsyncLazy(Func<Task<T>> factory)
        {
            instance = new Lazy<Task<T>>(() => Task.Run(factory));
        }

        public TaskAwaiter<T> GetAwaiter()
        {
            return instance.Value.GetAwaiter();

            
        }

        //public void Start()
        //{
        //    var unused = instance.Value;
        //}
    }
}