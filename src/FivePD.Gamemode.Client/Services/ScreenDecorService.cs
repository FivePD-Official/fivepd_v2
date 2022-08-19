// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Common.Builders;
using FivePD.Common.Models;
using FivePD.Gamemode.Client.Interfaces;
using FivePD.Gamemode.Client.Utilities;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Gamemode.Client.Services
{
    /**
     * <inheritdoc />
     */
    public class ScreenDecorService : IScreenDecorService
    {
        private readonly ITickService _tickService;
        private readonly ILocalPlayerService _localPlayerService;
        private readonly ILocalizationService _localizationService;

        private string _text;

        private string _version = string.Empty;
        private FDutyStatus _dutyStatus = FDutyStatus.OffDuty;
        private bool _showProtectionNotice;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScreenDecorService"/> class.
        /// </summary>
        /// <param name="tickService">The ITickService to use.</param>
        /// <param name="localPlayerService">The ILocalPlayerService to use.</param>
        /// <param name="localizationService">The ILocalizationService to use.</param>
        public ScreenDecorService(ITickService tickService, ILocalPlayerService localPlayerService, ILocalizationService localizationService)
        {
            this._tickService = tickService;
            this._localPlayerService = localPlayerService;
            this._localizationService = localizationService;
        }

        /**
         * <inheritdoc />
         */
        public string Version
        {
            get => this._version;
            set
            {
                this._version = value;
                this.BuildText();
            }
        }

        /**
         * <inheritdoc />
         */
        public FDutyStatus DutyStatus
        {
            get => this._dutyStatus;
            set
            {
                this._dutyStatus = value;
                this.BuildText();
            }
        }

        /**
         * <inheritdoc />
         */
        public bool ProtectionNotice
        {
            get => this._showProtectionNotice;
            set
            {
                this._showProtectionNotice = value;
                this.BuildText();
            }
        }

        /**
         * <inheritdoc />
         */
        public void BeginTextDraw()
        {
            this._tickService.OnTick += this.DrawOnScreen;
        }

        /**
         * <inheritdoc />
         */
        public void EndTextDraw()
        {
            this._tickService.OnTick -= this.DrawOnScreen;
        }

        private void BuildText()
        {
            // Header
            var text = new GtaStyleText()
                .White("Five")
                .Blue("PD")
                .White(" " + this._version + "\n");

            // Duty status display
            text = text
                .White(this.DutyStatusToString(this._dutyStatus) + "\n");

            // Protection notice
            if (this._showProtectionNotice)
            {
                text = text
                    .Red("Protected Build - Do Not Distribute");
            }

            this._text = text.Build();
        }

        private string DutyStatusToString(FDutyStatus dutyStatus)
        {
            return dutyStatus switch
            {
                FDutyStatus.OffDuty => this._localizationService.CurrentLocalization.PlayerStatus.OffDuty.Generic,
                FDutyStatus.OnDutyAndReceiving => this._localizationService.CurrentLocalization.PlayerStatus.OnDuty.GenericReceiving,
                FDutyStatus.OnDutyAndNotReceiving => this._localizationService.CurrentLocalization.PlayerStatus.OnDuty.GenericNotReceiving,
                _ => "Unknown",
            };
        }

        private Task DrawOnScreen()
        {
            Cfx.API.SetTextFont(0);
            Cfx.API.SetTextProportional(false);
            Cfx.API.SetTextScale(0.0f, 0.3f);
            Cfx.API.SetTextColour(255, 255, 255, 255);
            Cfx.API.SetTextDropshadow(0, 0, 0, 0, 255);
            Cfx.API.SetTextEdge(1, 0, 0, 0, 255);
            Cfx.API.SetTextDropShadow();
            Cfx.API.SetTextOutline();
            Cfx.API.SetTextEntry("STRING");
            Cfx.API.AddTextComponentString(this._text);
            Cfx.API.DrawText(0.005f, 0.005f);

            return Task.FromResult(0);
        }
    }
}