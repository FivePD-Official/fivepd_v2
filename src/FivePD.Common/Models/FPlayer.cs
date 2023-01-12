using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CitizenFX.Core;
using FivePD.Common.Attributes;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Common.Models
{
    /**
     * <summary>The base player class for the
     * gamemode. Provides a wrapper around game
     * functionality for easier interactions
     * with the framework.</summary>
     */
    public class FPlayer : FStatefulContainer
    {
        private static List<FPlayer> _cachedPlayers = new();

        /**
         * <summary>Initializes a new instance of the <see cref="FPlayer"/> class.</summary>
         * <param name="netId">The network identifier that corresponds to the player.</param>
         */
        public FPlayer(string netId)
        {
            this.NetId = netId;

            // Cache the player
            _cachedPlayers.Add(this);

            // Sets up duty status tracking
            this.DutyStatus = new FObservable<FDutyStatus>(
                this,
                "FivePD:DutyStatus",
                FDutyStatus.OffDuty);
        }

        /**
         * <summary>Gets the player's network identifier.</summary>
         */
        public string NetId { get; private set; }

        /// <summary>
        /// Gets the player's duty status observable.
        /// </summary>
        public FObservable<FDutyStatus> DutyStatus { get; private set; }

        /// <summary>
        /// Attempts to get the player with the specified netId.
        /// </summary>
        /// <param name="netId">The Cfx network identifier.</param>
        /// <returns>The FPlayer.</returns>
        public static FPlayer FromNetId(string netId)
        {
            // Try to get the player from the cache
            try
            {
                return _cachedPlayers
                    .First(i => i.NetId == netId);
            }

            // If not found, create a new player
            catch (Exception)
            {
                return new FPlayer(netId);
            }
        }

        /// <summary>
        /// Attempts to get the player with the specified netId.
        /// </summary>
        /// <param name="netId">The Cfx network identifier.</param>
        /// <returns>The FPlayer.</returns>
        public static FPlayer FromNetId(int netId)
        {
            return FromNetId(netId.ToString());
        }

        /// <summary>
        /// Gets the underlying Cfx player.
        /// </summary>
        /// <returns>A Cfx player.</returns>
        public Player GetUnderlyingPlayer()
        {
            #if IS_GAMEMODE_SERVER
            return CreateInstance<Player>(this.NetId);
            #else
            return new Player(Cfx.API.GetPlayerFromServerId(int.Parse(this.NetId)));
            #endif
        }

        /**
         * <inheritdoc />
         */
        public override string ToString()
        {
            return this.NetId;
        }

        /// <inheritdoc />
        public override string GetStateKey()
        {
            return $"player:{this.NetId}";
        }

        /// <inheritdoc />
        public override StateBag GetStateBag()
        {
            return this.GetUnderlyingPlayer().State;
        }

        #pragma warning disable S3011
        /**
         * !! HACK ALERT !! : Cfx does not let us
         * manually create a player from
         * a network ID. This solution is
         * much faster than the alternative...
         * so it will be okay for now.
         */
        private static T CreateInstance<T>(params object[] args)
        {
            var type = typeof(T);
            if (type.FullName == null)
            {
                throw new ArgumentException("Invalid type");
            }

            var instance = type.Assembly.CreateInstance(
                type.FullName,
                false,
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                args,
                null,
                null);

            return (T)instance;
        }
        #pragma warning restore S3011
    }
}