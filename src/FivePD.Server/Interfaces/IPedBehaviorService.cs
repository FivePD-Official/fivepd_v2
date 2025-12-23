using FivePD.Server.EntityWrappers;
using FivePD.Server.EntityWrappers.PedWrapper;

namespace FivePD.Server.Interfaces;

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
    public void StartBehavior(FPlayer player, FPed fPed);

    /// <summary>
    /// Stops the behavior logic for a specific ped and resets its state.
    /// </summary>
    /// <param name="fPed">The ped to stop the behavior for.</param>
    public void StopBehavior(FPed fPed);

    /// <summary>
    /// Stops the behavior logic for a specific ped and resets its state.
    /// </summary>
    /// <param name="networkId">The network id of the ped to stop the behavior for.</param>
    public void StopBehavior(int networkId);

    /// <summary>
    /// Adjusts the behavior of the specified ped to make it compliant, such as disabling fleeing and enabling certain combat limitations.
    /// </summary>
    /// <param name="fPed">The ped to modify, ensuring compliance behavior adjustments are applied.</param>
    public void MakeCompliant(FPed fPed);

    /// <summary>
    /// Forces a ped to become non-compliant, overriding any existing compliant behavior.
    /// </summary>
    /// <param name="fPed">The ped to make non-compliant.</param>
    public void MakeNonCompliant(FPed fPed);
}