using System;
using System.Collections.Concurrent;
using System.Threading;

namespace ET
{
    public class ThreadSynchronizationContext : SynchronizationContext
    {
        public static ThreadSynchronizationContext Instance { get; } = new (Thread.CurrentThread.ManagedThreadId);
        private readonly int threadId;
        // 线程同步队列,发送接收socket回调都放到该队列,由poll线程统一执行
        private readonly ConcurrentQueue<Action> queue = new ();
        private Action a;
        private ThreadSynchronizationContext(int threadId)
        {
            this.threadId = threadId;
        }
        public void Update()
        {
            while (true)
            {
                if (!queue.TryDequeue(out a)) return;
                try
                {
                    a();
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }
        public override void Post(SendOrPostCallback callback, object state)
        {
            if (Thread.CurrentThread.ManagedThreadId == threadId)
            {
                try
                {
                    callback(state);
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
                return;
            }
            queue.Enqueue(() => callback(state));
        }
        public void PostNext(Action action)
        {
            queue.Enqueue(action);
        }
    }
}