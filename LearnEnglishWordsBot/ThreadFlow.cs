using System;
using System.Collections.Generic;
using System.Threading;

namespace LearnEnglishWordsBot
{
    public class ThreadFlow<T> : IDisposable
    {
        private string Name { get; }
        private Queue<T> Queue { get; }
        private object QueueLock { get; }
        private bool ThreadDestroyFlag;
        private Thread ThreadQueue { get; }
        private AutoResetEvent ThreadARE { get; }
        public delegate void FlowEvent(T Value);
        private event FlowEvent Update;

        public ThreadFlow(string name, FlowEvent flowEvent)
        {
            Name = name;
            Queue = new Queue<T>();
            QueueLock = new object();
            ThreadARE = new AutoResetEvent(false);

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
