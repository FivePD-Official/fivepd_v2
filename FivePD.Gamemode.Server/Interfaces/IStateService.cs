// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

namespace FivePD.Gamemode.Server.Interfaces
{
    /**
     * <summary>Persists volatile data for usage between modules.</summary>
     */
    public interface IStateService
    {
        /**
         * <summary>Gets the data associated with a given key.</summary>
         * <param name="container">The state container to target.</param>
         * <param name="key">The specific key associated with the data.</param>
         * <returns>Previously set data.</returns>
         */
        public dynamic GetData(string container, string key);

        /**
         * <summary>Gets the data associated with a given key.</summary>
         * <param name="container">The state container to target.</param>
         * <param name="key">The specific key associated with the data.</param>
         * <param name="data">The data to store.</param>
         */
        public void SetData(string container, string key, object data);
    }
}