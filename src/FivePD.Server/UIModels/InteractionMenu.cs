using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.Enums;
using FivePD.Common.Models;
using FivePD.Common.Utilities;
using FivePD.Server.EntityWrappers;
using FivePD.Server.EntityWrappers.PedWrapper;
using FivePD.Server.EntityWrappers.PedWrapper.States;
using FivePD.Server.EntityWrappers.VehicleWrapper;
using FivePD.Server.Interfaces;
using FivePD.Server.Models;
using FivePD.Server.UIModels.MenuModels;
using Microsoft.Extensions.Primitives;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Server.UIModels;

#pragma warning disable

public sealed class InteractionMenu : Menu
{
    private readonly IGameNotificationService _gameNotificationService;
    private readonly IEntityService _entityService;
    private readonly ICfxService _cfxService;
    private readonly IConfigService _configService;
    private readonly IInternalInteractionService _interactionService;
    private readonly IPedStorageService _pedStorageService;
    private readonly IPedBehaviorService _pedBehaviorService;

    public InteractionMenu(
        IGameNotificationService gameNotificationService,
        IEntityService entityService,
        ICfxService cfxService,
        IConfigService configService,
        IInteractionService interactionService,
        IPedStorageService pedStorageService,
        IPedBehaviorService pedBehaviorService)
        : base(Controls.InteractionMenu.Id, Controls.InteractionMenu.Title, Controls.InteractionMenu.Key)
    {
        this._gameNotificationService = gameNotificationService;
        this._entityService = entityService;
        this._cfxService = cfxService;
        this._configService = configService;
        this._interactionService = (IInternalInteractionService)interactionService;
        this._pedStorageService = pedStorageService;
        this._pedBehaviorService = pedBehaviorService;
    }

    internal MenuButtonItem SearchPed { get; private set; }

    internal MenuListItem AskForLicense { get; private set; }

    internal MenuButtonItem DrugSwabTest { get; private set; }

    internal MenuButtonItem BreathalyzerTest { get; private set; }

    internal MenuButtonItem DetainOrRelease { get; private set; }

    internal MenuButtonItem LetGo { get; private set; }

    internal MenuButtonItem StepOutOfVehicle { get; private set; }

    internal MenuListItem Movement { get; private set; }

    internal MenuButtonItem EnterVehicleAsPassenger { get; private set; }

    public override bool CanOpen(FPlayer player)
    {
        var config = this._configService.GeneralConfig.Menu.Interaction;
        if (config.MustBeOnDuty && !player.IsOnDuty)
        {
            this._gameNotificationService.Notify(
                player,
                new GameNotificationOptions
                {
                    Title = "FivePD",
                    Message = "ERROR.USER.NOT_ON_DUTY"
                });

            return false;
        }

        if (config.MustBeInDepartment && !player.IsInDepartment())
        {
            this._gameNotificationService.Notify(
                player,
                new GameNotificationOptions
                {
                    Title = "FivePD",
                    Message = "ERROR.USER.NOT_IN_DEPARTMENT"
                });

            return false;
        }

        if (config.AccessEverywhere())
        {
            return true;
        }

        var playerIsInVehicle = player.FPed.Properties.IsInVehicle;
        if (config.AccessOnlyVehicle() && playerIsInVehicle)
        {
            return true;
        }

        return false;
    }

    protected override List<MenuItem> CreateItems()
    {
        var items = new List<MenuItem>();

        this.AskForLicense = new MenuListItem("LABEL.MENU.INTERACTION.ASK_FOR_LICENSE.TITLE", "", new List<MenuListOption>
        {
            new("LABEL.PED_LICENSE.DRIVING.SHORT"),
            new("LABEL.PED_LICENSE.FISHING.SHORT"),
            new("LABEL.PED_LICENSE.HUNTING.SHORT"),
            new("LABEL.PED_LICENSE.CCW.SHORT"),
        });
        this.AskForLicense.OnSelect += async (player, index) =>
        {
            var fPed = await this._entityService.GetClosestEntityToPlayerAsync<FPed>(player.GetUnderlyingPlayer());
            if (fPed is null || fPed.Properties.IsDead || !fPed.HasGeneratedData.Value)
            {
                return;
            }

            PedLicenseType licenseType;
            string licenseTypeKey;

            switch (index)
            {
                case 0:
                {
                    licenseType = PedLicenseType.Driving;
                    licenseTypeKey = "LABEL.PED_LICENSE.DRIVING.LONG";
                    break;
                }
                case 1:
                {
                    licenseType = PedLicenseType.Fishing;
                    licenseTypeKey = "LABEL.PED_LICENSE.FISHING.LONG";
                    break;
                }
                case 2:
                {
                    licenseType = PedLicenseType.Hunting;
                    licenseTypeKey = "LABEL.PED_LICENSE.HUNTING.LONG";
                    break;
                }
                case 3:
                {
                    licenseType = PedLicenseType.Ccw;
                    licenseTypeKey = "LABEL.PED_LICENSE.CCW.LONG";
                    break;
                }
                default: return;
            }

            var pedData = this._pedStorageService.GetPedData(fPed);
            if (pedData is null)
            {
                this._gameNotificationService.Notify(
                    player,
                    new GameNotificationOptions
                    {
                        Title = "FivePD",
                        Message = "ERROR.UNEXPECTED",
                    });
                return;
            }

            var license = pedData.GetLicense(licenseType);
            if (license is null)
            {
                this._gameNotificationService.Notify(
                    player,
                    new GameNotificationOptions
                    {
                        Title = "FivePD",
                        Message = "LABEL.MENU.INTERACTION.ASK_FOR_LICENSE.RESULT.NO_LICENSE_AVAILABLE",
                        MessageParams = new Dictionary<string, string>
                        {
                            { "type", licenseTypeKey }
                        }
                    });
                return;
            }

            this._gameNotificationService.Notify(
                player,
                new GameNotificationOptions
                {
                    Title = "FivePD",
                    Message = "LABEL.MENU.INTERACTION.ASK_FOR_LICENSE.RESULT.OK",
                    MessageParams = new Dictionary<string, string>
                    {
                        { "type", licenseTypeKey },
                        { "name", pedData.Firstname + " " + pedData.Lastname },
                        { "issue_date_label", "LABEL.DATE.ISSUE" },
                        { "issue_date", license.IssueDate.ToString("MM-dd-yyyy") },
                        { "expiration_date_label", "LABEL.DATE.EXPIRATION" },
                        { "expiration_date", license.ExpirationDate.ToString("MM-dd-yyyy") },
                        { "status_label", "LABEL.STATUS.SINGULAR" },
                        {
                            "status", license.Status switch
                            {
                                PedLicenseStatus.Valid => "LABEL.VALID",
                                PedLicenseStatus.Expired => "LABEL.EXPIRED",
                                PedLicenseStatus.Suspended => "LABEL.SUSPENDED",
                                PedLicenseStatus.Revoked => "LABEL.REVOKED",
                                _ => "LABEL.UNKNOWN",
                            }
                        },
                    }
                });
        };
        items.Add(this.AskForLicense);

        this.SearchPed = new MenuButtonItem("LABEL.MENU.INTERACTION.SEARCH_PED.TITLE", "");
        this.SearchPed.OnSelect += async player =>
        {
            var playerPed = FPed.FromFPlayer(player);

            var fPed = await this._entityService.GetClosestEntityToPlayerAsync<FPed>(player.GetUnderlyingPlayer());
            if (fPed is null || !fPed.HasGeneratedData.Value || fPed.Properties.IsInVehicle)
            {
                return;
            }

            fPed.Task.TurnTo(player.GetUnderlyingPlayer().Character);
            // playerPed.Task.PlayAnimation("anim@gangops@morgue@table@", "player_search");

            var pedData = this._pedStorageService.GetPedData(fPed);
            if (pedData is null)
            {
                this._gameNotificationService.Notify(
                    player,
                    new GameNotificationOptions
                    {
                        Title = "FivePD",
                        Message = "ERROR.UNEXPECTED",
                    });
                return;
            }

            var itemList = new StringBuilder();
            if (pedData.Items.Count > 0)
            {
                for (var i = 0; i < pedData.Items.Count; i++)
                {
                    if (pedData.Items[i].Illegal)
                    {
                        itemList.Append(pedData.Items[i].Title);
                    }
                    else
                    {
                        itemList.Append(pedData.Items[i].Title);
                    }

                    if (i < pedData.Items.Count - 1)
                    {
                        itemList.Append(", ");
                    }
                }
            }
            else
            {
                itemList.Append("-");
            }

            // playerPed.Task.ClearAnimation("random@domestic", "kneeling_base");
            this._gameNotificationService.Notify(
                player,
                new GameNotificationOptions
                {
                    Title = "FivePD",
                    Message = "LABEL.MENU.INTERACTION.SEARCH_PED.RESULT",
                    MessageParams = new Dictionary<string, string>
                    {
                        { "items", itemList.ToString() }
                    }
                });
        };
        items.Add(this.SearchPed);

        this.DrugSwabTest = new MenuButtonItem("LABEL.MENU.INTERACTION.DRUG_SWAB_TEST.TITLE", "");
        this.DrugSwabTest.OnSelect += async player =>
        {
            var fPed = await this._entityService.GetClosestEntityToPlayerAsync<FPed>(player.GetUnderlyingPlayer());
            if (fPed is null || !fPed.HasGeneratedData.Value || fPed.Properties.IsDead)
            {
                return;
            }

            if (!fPed.Properties.IsInVehicle)
            {
                fPed.Task.TurnTo(player.GetUnderlyingPlayer().Character);
            }

            var pedData = this._pedStorageService.GetPedData(fPed);
            if (pedData is null)
            {
                this._gameNotificationService.Notify(
                    player,
                    new GameNotificationOptions
                    {
                        Title = "FivePD",
                        Message = "ERROR.UNEXPECTED",
                    });
                return;
            }

            this._gameNotificationService.Notify(
                player,
                new GameNotificationOptions
                {
                    Title = "FivePD",
                    Message = pedData.Drugs.Count == 0
                        ? "LABEL.MENU.INTERACTION.DRUG_SWAB_TEST.RESULT.NEGATIVE"
                        : "LABEL.MENU.INTERACTION.DRUG_SWAB_TEST.RESULT.POSITION",
                    MessageParams = new Dictionary<string, string>
                    {
                        {
                            "result", pedData.Drugs.Count == 0
                                ? ""
                                : string.Join(", ", pedData.Drugs.Select(x => x.Name))
                        }
                    }
                });
        };
        items.Add(this.DrugSwabTest);

        this.BreathalyzerTest = new MenuButtonItem("LABEL.MENU.INTERACTION.BREATHALYZER_TEST.TITLE", "");
        this.BreathalyzerTest.OnSelect += async player =>
        {
            var fPed = await this._entityService.GetClosestEntityToPlayerAsync<FPed>(player.GetUnderlyingPlayer());
            if (fPed is null || !fPed.HasGeneratedData.Value || fPed.Properties.IsDead)
            {
                return;
            }

            if (!fPed.Properties.IsInVehicle)
            {
                fPed.Task.TurnTo(player.GetUnderlyingPlayer().Character);
            }

            var pedData = this._pedStorageService.GetPedData(fPed);
            if (pedData is null)
            {
                this._gameNotificationService.Notify(
                    player,
                    new GameNotificationOptions
                    {
                        Title = "FivePD",
                        Message = "ERROR.UNEXPECTED",
                    });
                return;
            }

            this._gameNotificationService.Notify(
                player,
                new GameNotificationOptions
                {
                    Title = "FivePD",
                    Message = "LABEL.MENU.INTERACTION.BREATHALYZER_TEST.RESULT.MAIN",
                    MessageParams = new Dictionary<string, string>
                    {
                        { "legal_limit_label", "LABEL.MENU.INTERACTION.BREATHALYZER_TEST.RESULT.LEGAL_LIMIT" },
                        { "legal_limit", this._configService.GeneralConfig.Peds.Alcohol.LegalLimit.ToString("0.00") },
                        { "result_label", "LABEL.MENU.INTERACTION.BREATHALYZER_TEST.RESULT.RESULT" },
                        { "result", pedData.AlcoholLevel.ToString("0.00") },
                    }
                });
        };
        items.Add(this.BreathalyzerTest);

        this.Movement = new MenuListItem("LABEL.MENU.INTERACTION.MOVEMENT.TITLE", "", new List<MenuListOption>
        {
            new("LABEL.MENU.INTERACTION.MOVEMENT.TYPE.STAND_STILL"),
            new("LABEL.MENU.INTERACTION.MOVEMENT.TYPE.GRAB"),
            new("LABEL.MENU.INTERACTION.MOVEMENT.TYPE.FOLLOW_ME"),
        });
        this.Movement.OnSelect += async (player, index) =>
        {
            var fPed = await this._entityService.GetClosestEntityToPlayerAsync<FPed>(player.GetUnderlyingPlayer());
            if (fPed is null || !fPed.Stopped.Value || fPed.Properties.IsDead || fPed.Properties.IsInVehicle)
            {
                return;
            }

            this._pedBehaviorService.StopBehavior(fPed);
            var playerPed = FPed.FromFPlayer(player);

            switch (index)
            {
                case 0:
                {
                    fPed.ChangeState(new FPedStandStillState(fPed));
                    break;
                }
                case 1:
                {
                    fPed.ChangeState(new FPedGrabbedState(fPed, playerPed));
                    break;
                }
                case 2:
                {
                    fPed.ChangeState(new FPedFollowPlayerState(fPed, player));
                    break;
                }
            }
        };
        items.Add(this.Movement);

        this.DetainOrRelease = new MenuButtonItem("LABEL.MENU.INTERACTION.DETAIN_RELEASE.TITLE", "");
        this.DetainOrRelease.OnSelect += async player =>
        {
            var fPed = await this._entityService.GetClosestEntityToPlayerAsync<FPed>(player.GetUnderlyingPlayer());
            if (fPed is null || fPed.Properties.IsDead || !fPed.Stopped.Value || !fPed.HasGeneratedData.Value || fPed.Properties.IsInVehicle)
            {
                return;
            }

            fPed.ChangeState(new FPedTurnToHeadingThenStandStillState(fPed, player.GetUnderlyingPlayer().Character.Heading));

            if (fPed.Detained.Value)
            {
                fPed.Detained.Value = false;
                this._gameNotificationService.Notify(
                    player,
                    new GameNotificationOptions
                    {
                        Title = "FivePD",
                        Message = "LABEL.MENU.INTERACTION.DETAIN_RELEASE.RELEASING",
                    });
                this._cfxService.SetEnableHandcuffs(fPed.GetUnderlyingPed.NetworkId, false);
            }
            else
            {
                fPed.Detained.Value = true;
                this._gameNotificationService.Notify(
                    player,
                    new GameNotificationOptions
                    {
                        Title = "FivePD",
                        Message = "LABEL.MENU.INTERACTION.DETAIN_RELEASE.DETAINING",
                    });
                this._cfxService.SetEnableHandcuffs(fPed.GetUnderlyingPed.NetworkId, true);
                this._pedBehaviorService.StopBehavior(fPed);
            }
        };
        items.Add(this.DetainOrRelease);

        this.LetGo = new MenuButtonItem("LABEL.MENU.INTERACTION.LET_GO.TITLE", "");
        this.LetGo.OnSelect += async player =>
        {
            var fPed = await this._entityService.GetClosestEntityToPlayerAsync<FPed>(player.GetUnderlyingPlayer());
            if (fPed is null || fPed.Properties.IsDead)
            {
                return;
            }

            if (fPed.Detained.Value)
            {
                this._gameNotificationService.Notify(
                    player,
                    new GameNotificationOptions
                    {
                        Title = "FivePD",
                        Message = "LABEL.MENU.INTERACTION.LET_GO.CANNOT_LET_GO_DETAINED",
                    });
                return;
            }

            this._gameNotificationService.Notify(
                player,
                new GameNotificationOptions
                {
                    Title = "FivePD",
                    Message = "LABEL.MENU.INTERACTION.LET_GO.SUCCESS",
                });

            this._pedBehaviorService.StopBehavior(fPed);
            fPed.Stopped.Value = false;
            Cfx.API.SetEntityDistanceCullingRadius(fPed.GetUnderlyingPed.Handle, 500f);
            fPed.Properties.SetAlwaysKeepTask(false);
            fPed.Properties.SetBlockPermanentEvents(false);
            this._pedBehaviorService.MakeNonCompliant(fPed);
            fPed.Properties.RemoveBlips();
            if (fPed.Properties.IsInVehicle)
            {
                var vehicleHandle = Cfx.API.GetVehiclePedIsIn(fPed.GetUnderlyingPed.Handle, false);
                var vehicleNetId = Cfx.API.NetworkGetNetworkIdFromEntity(vehicleHandle);
                fPed.ChangeState(new FPedCruiseWithVehicleState(fPed, vehicleNetId));
            }
            else
            {
                fPed.ChangeState(new FPedWanderAroundState(fPed));
            }
        };
        items.Add(this.LetGo);

        this.StepOutOfVehicle = new MenuButtonItem("LABEL.MENU.INTERACTION.STEP_OUT_VEHICLE.TITLE", "");
        this.StepOutOfVehicle.OnSelect += async player =>
        {
            var fPed = await this._entityService.GetClosestEntityToPlayerAsync<FPed>(player.GetUnderlyingPlayer());
            if (fPed is null || fPed.Properties.IsDead || !fPed.Properties.IsInVehicle)
            {
                return;
            }

            this._pedBehaviorService.StopBehavior(fPed);
            fPed.ChangeState(new FPedStepOutVehicleThenStandStillState(fPed));
        };
        items.Add(this.StepOutOfVehicle);

        this.EnterVehicleAsPassenger = new MenuButtonItem("LABEL.MENU.INTERACTION.ENTER_VEHICLE.TITLE", "");
        this.EnterVehicleAsPassenger.OnSelect += async player =>
        {
            var pedTask = this._entityService.GetClosestEntityToPlayerAsync<FPed>(player.GetUnderlyingPlayer());
            var vehicleTask = this._entityService.GetClosestEntityToPlayerAsync<FVehicle>(player.GetUnderlyingPlayer());

            await Task.WhenAll(pedTask, vehicleTask);

            var fPed = await pedTask;
            var fVehicle = await vehicleTask;
            if (fPed is null || fPed.Properties.IsDead || fPed.Properties.IsInVehicle || fVehicle is null)
            {
                return;
            }

            this._pedBehaviorService.StopBehavior(fPed);
            this._pedBehaviorService.MakeCompliant(fPed);
            fPed.ChangeState(new FPedEnterVehicleAsPassengerState(fPed, fVehicle));
        };
        items.Add(this.EnterVehicleAsPassenger);

        return items;
    }
}