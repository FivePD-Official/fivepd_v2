namespace FivePD.Common.Models
{
    /// <summary>
    /// Defines the types of menu items.
    /// </summary>
    public enum MenuItemType
    {
        /// <summary>
        /// Will only trigger an action on press.
        /// </summary>
        Button,

        /// <summary>
        /// Will display a select from which the user can select a value.
        /// </summary>
        List,

        /// <summary>
        /// Will display a checkbox.
        /// </summary>
        Checkbox,
    }
}