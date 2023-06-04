using System.Collections.Generic;
using FivePD.Gamemode.Common.Enums;
using Newtonsoft.Json.Linq;

namespace FivePD.Gamemode.Common.DTO;

#pragma warning disable

internal sealed record MenuDTO
{
    public sealed record GetAllMenu
    {
        public List<UIMenuResponse> Menus { get; set; }

        public record struct UIMenuResponse
        {
            public string Id { get; init; }

            public string Key { get; init; }

            public string Title { get; init; }

            public List<UIMenuItemResponse> Items { get; init; }

            public bool UseLocalization { get; init; }

            public record struct UIMenuItemResponse
            {
                public string Hashcode { get; init; }

                public string Title { get; init; }

                public string Description { get; init; }

                public MenuItemType Type { get; init; }

                public JArray Items { get; init; }
            }
        }
    }
}