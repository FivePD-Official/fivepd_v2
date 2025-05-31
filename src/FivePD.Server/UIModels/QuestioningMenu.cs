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
    private readonly IOllamaService _ollamaService;
    private readonly IRandomService _randomService;

    public QuestioningMenu(
        IGameNotificationService gameNotificationService,
        IEntityService entityService,
        IConfigService configService,
        IPedStorageService pedStorageService,
        IOllamaService ollamaService,
        IRandomService randomService)
        : base(Controls.QuestioningMenu.Id, Controls.QuestioningMenu.Title, Controls.QuestioningMenu.Key)
    {
        this._gameNotificationService = gameNotificationService;
        this._entityService = entityService;
        this._configService = configService;
        this._pedStorageService = pedStorageService;
        this._ollamaService = ollamaService;
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
                    Message = "UI.ERROR.UNEXPECTED",
                });
            return;
        }

        string response;
        if (this._configService.GeneralConfig.Questioning.UseOllamaResponses)
        {
            response = await this.GetOllamaResponseAsync(fPed, pedData, question);
        }
        else
        {
            response = this.GetConfigResponse(pedData, question);
        }

        this._gameNotificationService.Notify(
            player,
            new GameNotificationOptions
            {
                Title = "FivePD",
                Message = response,
            });
    }

    private async Task<string> GetOllamaResponseAsync(FPed fPed, PedData pedData, QuestioningConfig.QuestionConfig question)
    {
        var itemList = new StringBuilder().Append("Items: ");
        if (pedData.Items.Count > 0)
        {
            for (var i = 0; i < pedData.Items.Count; i++)
            {
                itemList.Append(pedData.Items[i].Title);
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

        var language = this._configService.GeneralConfig.Ollama.Language.ToLowerInvariant();
        var prompt = @$"
SYSTEM: You are roleplaying as a civilian in GTA 5 being questioned by a police officer. You MUST respond in {language} language ONLY. Never include any meta-commentary, disclaimers, or explanations about being an AI. Never mention that you are an AI or language model. Never use any <think> tags or similar processing tags. Respond directly as the character with ONLY the character's dialogue. Provide natural, conversational responses that simulate a real interaction between a civilian and a police officer.

CHARACTER BACKGROUND:
- You are a {(pedData.Gender == FGender.Male ? "male" : "female")} civilian
- Full name: {pedData.Firstname} {pedData.Lastname}
- Date of birth: {pedData.Birthdate:MM-dd-yyyy}
- Current alcohol level: {pedData.AlcoholLevel:F2} (Legal limit: {this._configService.GeneralConfig.Peds.Alcohol.LegalLimit:F2})
- Items in possession: {itemList}
- Current situation: {(fPed.Properties.IsInVehicle ? "You are currently in a vehicle" : "You are currently on foot")}

ROLEPLAYING RULES:
1. Always maintain a realistic civilian demeanor
2. Never admit to any illegal activities
3. If you have alcohol in your system, be slightly nervous but not overly defensive
4. If you're over the legal limit, be evasive but not confrontational
5. If you have illegal items, act naturally but be careful with your words
6. Keep responses natural and conversational, like a real person would speak to a police officer
7. Show appropriate concern about the police interaction
8. If you have nothing to hide, be cooperative but not overly eager
9. IMPORTANT: Respond ONLY in {language} language
10. IMPORTANT: Never include any meta-commentary or disclaimers
11. IMPORTANT: Never mention being an AI or language model
12. IMPORTANT: Never use <think> tags or any other processing tags
13. IMPORTANT: Provide ONLY the character's direct dialogue, nothing else
14. IMPORTANT: Keep responses short and to the point (1-2 sentences maximum)
15. IMPORTANT: If nervous or defensive, show it through your words only

RESPONSE STYLE:
- Provide natural, flowing dialogue that a real person would use
- Keep responses brief and focused
- Show personality through word choice and speaking style
- If nervous, show it through speech patterns (stuttering, hesitation, etc.)
- If confident, show it through clear, direct responses
- If hiding something, show it through careful word choice
- Keep responses to 1-2 sentences maximum
- If in a vehicle, briefly acknowledge your position
- If on foot, briefly acknowledge your surroundings

CURRENT SITUATION:
You are being questioned by a police officer who has just asked: '{question.Text}'. You are currently {(fPed.Properties.IsInVehicle ? "sitting in your vehicle" : "standing on the street")}.

Provide a natural, conversational response in {language} that reflects your current state (alcohol level, items, vehicle status, etc.) while following the roleplaying rules above. Remember: respond ONLY in {language}, never include any meta-commentary, never use any <think> tags, and provide ONLY the character's direct dialogue. Keep your response short (1-2 sentences) and natural.
";

        return await this._ollamaService.GetCompletionAsync(prompt);
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