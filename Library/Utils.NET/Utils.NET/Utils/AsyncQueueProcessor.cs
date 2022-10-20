using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Utils.NET.Utils
{
    public class AsyncQueueProcessor<T>
    {
        /// <summary>
        /// Action used to process elements
        /// </summary>
        private Func<T, Task> processor;

        /// <summary>
        /// Queue to store elements for processing
        /// </summary>
        private Queue<T> queue = new Queue<T>();

        /// <summary>
        /// Object used to lock the queue
        /// </summary>
        private object queueLock = new object();

        /// <summary>
        /// If the processing is running
        /// </summary>
        private bool running = false;

        public AsyncQueueProcessor(Func<T, Task> processor)
        {
            this.processor = processor;
        }

        /// <summary>
        /// Adds an element to the processing queue
        /// </summary>
        /// <param name="element"></param>
        public void Push(T element)
        {
            lock (queueLock)
            {
                queue.Enqueue(element);
                if (running) return;
                running = true;
            }

            RunProcessing();
        }

        private async void RunProcessing()
        {
            while (TryDequeue(out var element))
            {
                await processor(element);
            }
        }

        private bool TryDequeue(out T element)
        {
            lock (queueLock)
            {
                if (queue.Count == 0)
                {
                    running = false;
                    element = default;
                    return false;
                }
                else
                {
                    element = queue.Dequeue();
                    return true;
                }
            }
        }
    }
}
