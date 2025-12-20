using FivePD.Server.EntityWrappers;
using FivePD.Server.EntityWrappers.PedWrapper;

namespace FivePD.Server.Interfaces;

#pragma warning disable

/// <summary>
/// Service responsible for managing the behavioral logic and AI decision-making of peds.
/// </summary>
public interface IPedBehaviorService
{
    /// <summary>
    /// Starts the behavior logic for a specific ped.
    /// </summary>
    /// <param name="player">The player that triggered or is the target of the behavior.</param>
    /// <param name="fPed">The ped to start the behavior for.</param>
    void StartBehavior(FPlayer player, FPed fPed);

    /// <summary>
    /// Stops the behavior logic for a specific ped and resets its state.
    /// </summary>
    /// <param name="ped">The ped to stop the behavior for.</param>
    void StopBehavior(FPed ped);
    
    /// <summary>
    /// Stops the behavior logic for a specific ped and resets its state.
    /// </summary>
    /// <param name="ped">The network id of the ped to stop the behavior for.</param>
    void StopBehavior(int networkId);
}
