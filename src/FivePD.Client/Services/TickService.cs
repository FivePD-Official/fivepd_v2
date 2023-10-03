using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Client.Interfaces;
using FivePD.Client.Interfaces.Lifecycle;

namespace FivePD.Client.Services;

/// <inheritdoc />
internal class TickService : ITickService, IOnInit
{
    private readonly AddTickHandler _addTickHandler;

    private readonly RemoveTickHandler _removeTickHandler;

    private readonly ConcurrentDictionary<string, IntervalTask> _intervalTasks = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="TickService"/> class.
    /// </summary>
    /// <param name="addTickHandler">The method to use when adding a function to be called each tick.</param>
    /// <param name="removeTickHandler">The method to use when removing a function.</param>
    public TickService(AddTickHandler addTickHandler, RemoveTickHandler removeTickHandler)
    {
        this._addTickHandler = addTickHandler;
        this._removeTickHandler = removeTickHandler;
    }

    /// <inheritdoc />
    public event Func<Task> OnTick
    {
        add => this._addTickHandler(value);

        remove => this._removeTickHandler(value);
    }

    /// <inheritdoc/>
    public void OnInit()
    {
        this.OnTick += () =>
        {
            var dateNowTicks = DateTime.Now.Ticks;
            foreach (var task in this._intervalTasks.Values)
            {
                var ticksDiff = dateNowTicks - task.Date.Ticks;
                var millisecondsDiff = ticksDiff / TimeSpan.TicksPerMillisecond;

                if (millisecondsDiff >= task.IntervalInMs)
                {
                    task.Action?.Invoke();
                    task.TaskAction?.Invoke();
                    task.Date = DateTime.Now;
                }
            }

            return Task.FromResult(0);
        };
    }

    /// <inheritdoc />
    public Guid EnqueueInterval(Action action, int intervalInMs)
    {
        var guid = Guid.NewGuid();

        this._intervalTasks[guid.ToString()] = new IntervalTask(action, intervalInMs);

        return guid;
    }

    /// <inheritdoc />
    public void DequeueInterval(Guid guid)
    {
        this._intervalTasks.TryRemove(guid.ToString(), out _);
    }

    private sealed class IntervalTask
    {
        public IntervalTask(Action action, int intervalInMs = 0)
        {
            this.Action = action;
            this.IntervalInMs = intervalInMs;
        }

        public IntervalTask(Func<Task> action, int intervalInMs = 0)
        {
            this.TaskAction = action;
            this.IntervalInMs = intervalInMs;
        }

        public Action Action { get; }

        public Func<Task> TaskAction { get; }

        public int IntervalInMs { get; }

        public DateTime Date { get; set; } = DateTime.Now;
    }
}