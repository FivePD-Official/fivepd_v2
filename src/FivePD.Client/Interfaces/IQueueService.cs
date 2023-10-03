using System;
using System.Threading.Tasks;

namespace FivePD.Client.Interfaces;

#pragma warning disable

internal interface IQueueService
{
    public void Queue(Action task);
}