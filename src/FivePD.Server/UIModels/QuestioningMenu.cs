using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FivePD.Common;
using FivePD.Common.ConfigModels.GeneralConfigs;
using FivePD.Common.Enums;
using FivePD.Common.Models;
using FivePD.Server.EntityWrappers;
using FivePD.Server.EntityWrappers.PedWrapper;
using FivePD.Server.Interfaces;
using FivePD.Server.Models;
using FivePD.Server.UIModels.MenuModels;

namespace FivePD.Server.UIModels;

#pragma warning disable

public sealed class QuestioningMenu : Menu
{
    private readonly IGameNotificationService _gameNotificationService;
    private readonly IEntityService _entityService;
    private readonly IConfigService _configService;
    private readonly IPedStorageService _pedStorageService;
    private readonly IRandomService _randomService;

    public QuestioningMenu(
        IGameNotificationService gameNotificationService,
        IEntityService entityService,
        IConfigService configService,
        IPedStorageService pedStorageService,
        IRandomService randomService)
        : base(Controls.QuestioningMenu.Id, Controls.QuestioningMenu.Title, Controls.QuestioningMenu.Key)
    {
        this._gameNotificationService = gameNotificationService;
        this._entityService = entityService;
        this._configService = configService;
        this._pedStorageService = pedStorageService;
        this._randomService = randomService;
    }

    public override bool CanOpen(FPlayer player)
    {
        if (player.FPed.Properties.IsInVehicle)
        {
            return false;
        }

        return true;
    }

    protected override List<MenuItem> CreateItems()
    {
        var items = new List<MenuItem>();
        var questions = this._configService.GeneralConfig.Questioning.Questions;

        foreach (var question in questions)
        {
            var menuItem = new MenuButtonItem(question.Text, "");
            menuItem.OnSelect += async player => await this.OnQuestionSelectedAsync(player, question);
            items.Add(menuItem);
        }

        return items;
    }

    private async Task OnQuestionSelectedAsync(FPlayer player, QuestioningConfig.QuestionConfig question)
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
                Message = this.GetConfigResponse(pedData, question),
            });
    }

    private string GetConfigResponse(PedData pedData, QuestioningConfig.QuestionConfig question)
    {
        var hasIllegalItems = pedData.Items.Any(x => x.Illegal);
        var isOverAlcoholLimit = pedData.AlcoholOverLegalLimit();
        var hasDrugs = pedData.Drugs.Count > 0;

        var eligibleResponses = question.Responses
            .Where(r =>
            {
                var conditions = r.Conditions;
                return (!conditions.HasIllegalItems || conditions.HasIllegalItems == hasIllegalItems) &&
                       (!conditions.IsOverAlcoholLimit || conditions.IsOverAlcoholLimit == isOverAlcoholLimit) &&
                       (!conditions.HasDrugs || conditions.HasDrugs == hasDrugs);
            })
            .ToList();

        if (eligibleResponses.Count == 0)
        {
            return "I don't want to answer that.";
        }

        var totalChance = eligibleResponses.Sum(r => r.Chance);
        var roll = this._randomService.Next(0, totalChance);
        var currentChance = 0;

        foreach (var response in eligibleResponses)
        {
            currentChance += response.Chance;
            if (roll < currentChance)
            {
                return response.Text;
            }
        }

        return eligibleResponses[0].Text;
    }
} 