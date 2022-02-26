// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

namespace FivePD.Gamemode.Client.Interfaces
{
    /**
     * <summary>Provides utility methods for controlling
     * the content presented on user's screen passively.
     * (Used for the version banner, mainly).</summary>
     */
    public interface IScreenDecorService
    {
        /**
         * <summary>Gets or sets the text to draw on the screen.</summary>
         */
        public string TextToDraw { get; set; }

        /**
         * <summary>Begins drawing text on the screen.</summary>
         */
        public void BeginTextDraw();

        /**
         * <summary>Ends drawing text on the screen.</summary>
         */
        public void EndTextDraw();
    }
}