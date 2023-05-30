using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;
using FivePD.Gamemode.Server.Interfaces;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Gamemode.Server.Services;

/// <inheritdoc />
public class ConcurrentTickQueueService : ITickQueueService
{
    // Inspiration from https://github.com/citizenfx/fivem/blob/f4befbfee415e8f7aea486b61a9cdbc30408cf09/ext/webadmin/server/HttpServer.cs
    private readonly string _resourceName = Cfx.API.GetCurrentResourceName();

    private readonly ConcurrentQueue<Tuple<Action, TaskCompletionSource<int>>> _tickQueue = new();

    private readonly ConcurrentDictionary<string, IntervalTask> _intervalTasks = new();

    /// <inheritdoc />
    public Task OnTickAsync()
    {
        while (this._tickQueue.TryDequeue(out var call))
        {
            try
            {
                call.Item1();

                call.Item2.SetResult(0);
            }
            catch (Exception e)
            {
                call.Item2.SetException(e);
            }
        }

        foreach (var task in this._intervalTasks.Values)
        {
            if (task.Stopwatch.ElapsedMilliseconds >= task.IntervalInMs)
            {
                task.Action.Invoke();
                task.Stopwatch.Restart();
            }
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task EnqueueAsync(Action action)
    {
        var tcs = new TaskCompletionSource<int>();

        this._tickQueue.Enqueue(Tuple.Create(action, tcs));

        Cfx.API.ScheduleResourceTick(this._resourceName);

        return tcs.Task;
    }

    /// <inheritdoc />
    public async Task<T> EnqueueAsync<T>(Func<T> action)
    {
        var refHolder = new ReferenceHolder<T>();

        await this.EnqueueAsync(() => { refHolder.Value = action(); });

        return refHolder.Value;
    }

    /// <inheritdoc />
    public Guid EnqueueInterval(Action action, int intervalInMs)
    {
        var guid = Guid.NewGuid();

        this._intervalTasks[guid.ToString()] = new IntervalTask(action, intervalInMs);
        Cfx.API.ScheduleResourceTick(this._resourceName);

        return guid;
    }

    /// <inheritdoc />
    public void DequeueInterval(Guid guid)
    {
        this._intervalTasks.TryRemove(guid.ToString(), out _);
    }

    private sealed class ReferenceHolder<T>
    {
        public T Value { get; set; }
    }

    private sealed class IntervalTask
    {
        public IntervalTask(Action action, int intervalInMs = 0)
        {
            this.Action = action;
            this.IntervalInMs = intervalInMs;
            this.Stopwatch.Start();
        }

        public Action Action { get; }

        public int IntervalInMs { get; }

        public Stopwatch Stopwatch { get; } = new();
    }
}