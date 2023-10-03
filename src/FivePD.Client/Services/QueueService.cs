using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FivePD.Client.Interfaces;
using FivePD.Client.Interfaces.Lifecycle;

namespace FivePD.Client.Services;

#pragma warning disable

internal class QueueService : IQueueService, IOnInit, IOnDestroy
{
    private readonly ITickService _tickService;
    private readonly Queue<Action> _tasks = new();
    
    public QueueService(ITickService tickService)
    {
        this._tickService = tickService;
    }

    public void OnInit()
    {
        this._tickService.OnTick += this.OnTick;
    }
    
    public void OnDestroy()
    {
        this._tickService.OnTick -= this.OnTick;
    }

    public void Queue(Action task)
    {
        this._tasks.Enqueue(task);
    }

    private Task OnTick()
    {
        if (this._tasks.Count > 0)
        {
            this._tasks.Dequeue().Invoke();
        }
        
        return Task.FromResult(0);
    }
}