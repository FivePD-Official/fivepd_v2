using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.Builders;
using FivePD.Common.Enums;
using FivePD.Common.Models;
using FivePD.Server.EntityWrappers.PedWrapper.States;

namespace FivePD.Server.EntityWrappers.PedWrapper;

/// <summary>
/// Represents a wrapper around a CitizenFX ped entity, providing additional FivePD-specific functionality.
/// </summary>
public class FPed : FEntity, IFStatefulContainer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FPed"/> class.
    /// </summary>
    /// <param name="fPedBuilder">The builder containing initial ped configuration.</param>
    public FPed(FPedBuilder fPedBuilder)
    {
        this.PedDataId = fPedBuilder.PedDataId;
        this.NetworkId = fPedBuilder.NetworkId;
        this.OwningVehicleId = fPedBuilder.OwningVehicleNetworkId;

        this.Create();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FPed"/> class.
    /// </summary>
    /// <param name="fPed">The FPed to use.</param>
    private FPed(CommonFPed fPed)
    {
        this.NetworkId = fPed.NetworkId;

        this.OwningVehicleId = fPed.OwningVehicleId;

        this.Create();
    }

    /// <summary>
    /// Gets the ped's current state.
    /// </summary>
    public FPedState State { get; private set; }

    /// <summary>
    /// Gets the ped's observable with a value indicating if the ped is stopped.
    /// </summary>
    public FStateBagObservable<bool> Stopped { get; private set; }

    /// <summary>
    /// Gets the ped's observable with a value indicating if the ped has generated data.
    /// </summary>
    public FStateBagObservable<bool> HasGeneratedData { get; private set; }

    /// <summary>
    /// Gets the ped's observable with a value indicating if the ped is detained.
    /// </summary>
    public FStateBagObservable<bool> Detained { get; private set; }

    /// <summary>
    /// Gets the ped's observable with a value indicating if the ped is being moved.
    /// </summary>
    public FStateBagObservable<FPedMovement> Movement { get; private set; }

    /// <summary>
    /// Gets the properties of the <see cref="FPed"/>.
    /// </summary>
    public FPedProperties Properties { get; private set; }

    /// <summary>
    /// Gets the tasks of the <see cref="FPed"/>.
    /// </summary>
    public FPedTasks Task { get; private set; }

    /// <summary>
    /// Gets the weapons of the <see cref="FPed"/>.
    /// </summary>
    public FWeaponCollection Weapons { get; private set; }

    /// <summary>
    /// Gets the underlying <see cref="Ped"/>.
    /// </summary>
    public Ped GetUnderlyingPed => Entity.FromNetworkId(this.NetworkId) as Ped;

    /// <summary>
    /// Gets the owner of the <see cref="FPed"/>.
    /// </summary>
    public Player Owner => this.GetUnderlyingPed?.Owner;

    /// <summary>
    /// Gets the ped's PedHistory id.
    /// </summary>
    public int PedDataId { get; internal set; }

    /// <summary>
    /// Gets the entity's network identifier.
    /// </summary>
    public int NetworkId { get; }

    /// <summary>
    /// Gets the id of the vehicle the ped is owning.
    /// </summary>
    public int OwningVehicleId { get; }

    /// <summary>
    /// Created an <see cref="FPed"/> wrapper from the specified <see cref="FPlayer"/>.
    /// </summary>
    /// <param name="player">The <see cref="FPlayer"/> instance to use.</param>
    /// <returns>The FPed.</returns>
    public static FPed FromFPlayer(FPlayer player)
    {
        return new FPed(new CommonFPed(new CommonFPedBuilder(player.GetUnderlyingPlayer().Character.NetworkId)));
    }

    /// <summary>
    /// Created an <see cref="FPed"/> wrapper from the specified <see cref="Ped"/>.
    /// </summary>
    /// <param name="ped">The <see cref="Ped"/> instance to use.</param>
    /// <returns>The FPed.</returns>
    public static FPed FromPed(Ped ped)
    {
        return new FPed(new CommonFPed(new CommonFPedBuilder(ped.NetworkId)));
    }

    /// <summary>
    /// Created an <see cref="FPed"/> wrapper from the specified <see cref="Ped"/>.
    /// </summary>
    /// <param name="networkId">The network ID of the ped.</param>
    /// <returns>The FPed.</returns>
    public static FPed FromNetId(int networkId)
    {
        return new FPed(new CommonFPed(new CommonFPedBuilder(networkId)));
    }

    /// <summary>
    /// Changes the state of the ped.
    /// </summary>
    /// <param name="state">The new state.</param>
    public void ChangeState(FPedState state)
    {
        this.State?.Exit();
        this.State = state;
        this.State?.Enter();
    }

    /// <inheritdoc />
    public string GetStateKey()
    {
        return $"ped:{this.GetUnderlyingPed?.NetworkId ?? 0}";
    }

    /// <inheritdoc />
    public StateBag GetStateBag()
    {
        return this.GetUnderlyingPed?.State;
    }

    private void Create()
    {
        this.Properties = new(this);
        this.Task = new(this);
        this.Weapons = new(this);

        this.Stopped = new FStateBagObservable<bool>(this, StateBagKeys.Ped.Stopped, false);
        this.HasGeneratedData = new FStateBagObservable<bool>(this, StateBagKeys.Ped.HasGeneratedData, false);
        this.Detained = new FStateBagObservable<bool>(this, StateBagKeys.Ped.Detained, false);
        this.Movement = new FStateBagObservable<FPedMovement>(this, StateBagKeys.Ped.Movement, FPedMovement.None);
    }
}