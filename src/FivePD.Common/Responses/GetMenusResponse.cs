using System.Collections.Generic;
using FivePD.Common.Models;
using Newtonsoft.Json.Linq;

#pragma warning disable CS1591
#pragma warning disable CS1591
#pragma warning disable SA1600

namespace FivePD.Common.Responses
{
    internal class GetMenusResponse
    {
        public List<UIMenuResponse> Menus { get; set; }

        public class UIMenuResponse
        {
            public string Hashcode { get; set; }

            public string Key { get; set; }

            public string Title { get; set; }

            public List<UIMenuItemResponse> Items { get; set; }

            public bool UseLocalization { get; set; }

            public class UIMenuItemResponse
            {
                public string Hashcode { get; set; }

                public string Title { get; set; }

                public string Description { get; set; }

                public MenuItemType Type { get; set; }

                public JArray Items { get; set; }
            }
        }
    }
}