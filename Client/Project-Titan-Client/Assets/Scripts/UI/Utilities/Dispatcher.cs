using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Dispatcher
{
    private ConcurrentQueue<Action> queue = new ConcurrentQueue<Action>();

    public void Push(Action action)
    {
        queue.Enqueue(action);
    }

    public void RunActions()
    {
        while (queue.TryDequeue(out var action))
            action();
    }
}
