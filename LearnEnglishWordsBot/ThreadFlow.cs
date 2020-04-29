using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LearnEnglishWordsBot
{
    public class ThreadFlow<T> : IDisposable
    {
        readonly string Name;
        Queue<T> Queue = new Queue<T>();
        object QueueLock = new object();
        bool ThreadDestroyFlag = false;
        Thread ThreadQueue = null;
        AutoResetEvent ThreadARE = new AutoResetEvent(false);
        public delegate void FlowEvent(T Value);
        event FlowEvent Update;

        public ThreadFlow(
            string name,
            FlowEvent flowEvent)
        {
            Name = name;

            ThreadQueue = new Thread(new ThreadStart(ThreadWork))
            {
                IsBackground = true,
                Name = name
            };
            ThreadQueue.Start();

            Update += flowEvent;
        }

        public void Dispose()
        {
            ThreadDestroyFlag = true;

            ThreadARE.Set();
            if (ThreadQueue != null)
                if (ThreadQueue.IsAlive)
                    ThreadQueue.Join();
        }

        public void Set(T value)
        {
            lock (QueueLock)
                Queue.Enqueue(value);
            ThreadARE.Set();
        }

        void ThreadWork()
        {
            while (ThreadDestroyFlag == false)
            {
                if (Queue.Count > 0)
                {
                    T Value;
                    lock (QueueLock)
                        Value = Queue.Dequeue();
                    Update(Value);
                }
                else
                    ThreadARE.WaitOne();
            }
        }
    }
}
