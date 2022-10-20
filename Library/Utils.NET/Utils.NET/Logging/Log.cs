using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Utils.NET.Modules;

namespace Utils.NET.Logging
{
    public class Log
    {
        private struct Entry
        {
            /// <summary>
            /// The text to log
            /// </summary>
            public string text;

            /// <summary>
            /// The color of thi entry
            /// </summary>
            public ConsoleColor color;

            public Entry(string text, ConsoleColor color)
            {
                this.text = text;
                this.color = color;
            }
        }

        /// <summary>
        /// The global log instance
        /// </summary>
        private static Log instance;

        /// <summary>
        /// The backup writing method
        /// </summary>
        public static Action<string> backupWrite = Console.WriteLine;

        /// <summary>
        /// Sets the global log instance, to be called by static methods
        /// </summary>
        /// <param name="log"></param>
        public static void SetGlobalLog(Log log)
        {
            instance = log;
        }

        /// <summary>
        /// Writes a string to the global log
        /// </summary>
        /// <param name="value"></param>
        public static void Write(string value)
        {
            Write(new Entry(value, ConsoleColor.White));
        }

        /// <summary>
        /// Writes a string to the global log
        /// </summary>
        /// <param name="value"></param>
        public static void Write(string value, ConsoleColor color)
        {
            Write(new Entry(value, color));
        }

        /// <summary>
        /// Write an object to the global log
        /// </summary>
        /// <param name="value"></param>
        public static void Write(object value)
        {
            Write(new Entry(value.ToString(), ConsoleColor.White));
        }

        /// <summary>
        /// Writes an error string to the global log
        /// </summary>
        /// <param name="value"></param>
        public static void Error(string value)
        {
            Write(new Entry(value, ConsoleColor.Red));
        }

        /// <summary>
        /// Writes an error object to the global log
        /// </summary>
        /// <param name="value"></param>
        public static void Error(object value)
        {
            Write(new Entry(value.ToString(), ConsoleColor.Red));
        }

        /// <summary>
        /// Appends an entry to the log
        /// </summary>
        /// <param name="entry"></param>
        private static void Write(Entry entry)
        {
            if (instance != null)
                instance.WriteEntryConcurrent(entry);
            else
                backupWrite?.Invoke(entry.text);
        }

        /// <summary>
        /// The method used to write
        /// </summary>
        private Action<string> writeMethod;

        /// <summary>
        /// Entries queued from alternate threads
        /// </summary>
        private ConcurrentQueue<Entry> queuedEntries = new ConcurrentQueue<Entry>();

        /// <summary>
        /// The ID of the logging thread
        /// </summary>
        private int loggingThread;

        private ManualResetEvent _event = new ManualResetEvent(false);
        private bool _stopped = false;

        public Log(Action<string> writeMethod, int loggingThread)
        {
            this.writeMethod = writeMethod;
            this.loggingThread = loggingThread;
        }

        /// <summary>
        /// Writes the concurrent queue to the log
        /// </summary>
        public void WriteQueue()
        {
            while (queuedEntries.TryDequeue(out var entry))
            {
                WriteEntry(entry);
            }
        }

        /// <summary>
        /// Writes an entry to the log and checks for thread safety
        /// </summary>
        /// <param name="entry"></param>
        private void WriteEntryConcurrent(Entry entry)
        {
            if (Thread.CurrentThread.ManagedThreadId != loggingThread) // not on the correct thread
            {
                queuedEntries.Enqueue(entry);
                return;
            }

            WriteEntry(entry);
        }

        /// <summary>
        /// Writes an entry to the log
        /// </summary>
        /// <param name="entry"></param>
        private void WriteEntry(Entry entry)
        {
            Console.ForegroundColor = entry.color;
            writeMethod($"[{DateTime.Now.ToString()}] "); // write timestamp
            writeMethod(entry.text);
            writeMethod("\n");
        }

        internal static void Run()
        {
            instance?.RunLog();
        }

        private void RunLog()
        {
            while (!_stopped)
            {
                WriteQueue();
                while (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true);
                    switch (key.Key)
                    {
                        case ConsoleKey.Q:
                            StopLog();
                            break;
                    }
                }
                _event.WaitOne(50);
            }
        }

        internal static void Stop()
        {
            instance?.StopLog();
        }

        private void StopLog()
        {
            _stopped = true;
            _event.Set();
        }
    }
}
