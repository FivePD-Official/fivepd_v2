namespace FivePD.Server.EntityWrappers.PedWrapper.States;

#pragma warning disable

public abstract class FPedState
{
    protected readonly FPed Ped;

    protected FPedState(FPed ped)
    {
        this.Ped = ped;
    }

    public abstract void Enter();

    public abstract void Exit();
}