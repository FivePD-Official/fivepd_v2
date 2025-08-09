namespace FivePD.Server.Models.VariableData;

/// <summary>
/// Contains core variable data attributes.
/// </summary>
public class CoreAttributes
{
    /// <summary>
    /// Represents the options for a variable data attribute.
    /// </summary>
    /// <param name="Hidden">A value indicating whether the attribute is hidden.</param>
    /// <param name="Required">A value indicating whether the attribute is required.</param>
    /// <param name="Readonly">A value indicating whether the attribute is readonly.</param>
    /// <param name="ShowInGrid">A value indicating whether the attribute is shown in the grid.</param>
#pragma warning disable SA1313
    public readonly record struct Options(bool Hidden, bool Required, bool Readonly, bool ShowInGrid);
#pragma warning restore SA1313

    /// <summary>
    /// Represents the submit date attribute.
    /// </summary>
    public class SubmitDate : VariableDataAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubmitDate"/> class.
        /// </summary>
        /// <param name="options">The options for the attribute.</param>
        public SubmitDate(Options options = default)
        {
            this.Name = "SubmitDate";
            this.Label = "Date of submit";
            this.DataType = VariableDataType.Date;

            this.Required = options.Required;
            this.ShowInGrid = options.ShowInGrid;
            this.Hidden = options.Hidden;
            this.Readonly = options.Readonly;
        }
    }

    /// <summary>
    /// Represents the department ID attribute.
    /// </summary>
    public class DepartmentId : VariableDataAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DepartmentId"/> class.
        /// </summary>
        /// <param name="options">The options for the attribute.</param>
        public DepartmentId(Options options = default)
        {
            this.Name = "DepartmentId";
            this.Label = "Department";
            this.DataType = VariableDataType.Integer;

            this.Required = options.Required;
            this.ShowInGrid = options.ShowInGrid;
            this.Hidden = options.Hidden;
            this.Readonly = options.Readonly;
        }
    }

    /// <summary>
    /// Represents the department IDs attribute.
    /// </summary>
    public class DepartmentIds : VariableDataAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DepartmentIds"/> class.
        /// </summary>
        /// <param name="options">The options for the attribute.</param>
        public DepartmentIds(Options options = default)
        {
            this.Name = "DepartmentIds";
            this.Label = "Departments";
            this.DataType = VariableDataType.IntegerList;

            this.Required = options.Required;
            this.ShowInGrid = options.ShowInGrid;
            this.Hidden = options.Hidden;
            this.Readonly = options.Readonly;
        }
    }

    /// <summary>
    /// Represents the user ID attribute.
    /// </summary>
    public class UserId : VariableDataAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserId"/> class.
        /// </summary>
        /// <param name="options">The options for the attribute.</param>
        public UserId(Options options = default)
        {
            this.Name = "UserId";
            this.Label = "User";
            this.DataType = VariableDataType.Integer;

            this.Required = options.Required;
            this.ShowInGrid = options.ShowInGrid;
            this.Hidden = options.Hidden;
            this.Readonly = options.Readonly;
        }
    }

    /// <summary>
    /// Represents the user IDs attribute.
    /// </summary>
    public class UserIds : VariableDataAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserIds"/> class.
        /// </summary>
        /// <param name="options">The options for the attribute.</param>
        public UserIds(Options options = default)
        {
            this.Name = "UserIds";
            this.Label = "Users";
            this.DataType = VariableDataType.IntegerList;

            this.Required = options.Required;
            this.ShowInGrid = options.ShowInGrid;
            this.Hidden = options.Hidden;
            this.Readonly = options.Readonly;
        }
    }

    /// <summary>
    /// Represents the incident history ID attribute.
    /// </summary>
    public class IncidentHistoryId : VariableDataAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IncidentHistoryId"/> class.
        /// </summary>
        /// <param name="options">The options for the attribute.</param>
        public IncidentHistoryId(Options options = default)
        {
            this.Name = "IncidentHistoryId";
            this.Label = "Incident";
            this.DataType = VariableDataType.Integer;

            this.Required = options.Required;
            this.ShowInGrid = options.ShowInGrid;
            this.Hidden = options.Hidden;
            this.Readonly = options.Readonly;
        }
    }

    /// <summary>
    /// Represents the incident history IDs attribute.
    /// </summary>
    public class IncidentHistoryIds : VariableDataAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IncidentHistoryIds"/> class.
        /// </summary>
        /// <param name="options">The options for the attribute.</param>
        public IncidentHistoryIds(Options options = default)
        {
            this.Name = "IncidentHistoryIds";
            this.Label = "Incidents";
            this.DataType = VariableDataType.IntegerList;

            this.Required = options.Required;
            this.ShowInGrid = options.ShowInGrid;
            this.Hidden = options.Hidden;
            this.Readonly = options.Readonly;
        }
    }

    /// <summary>
    /// Represents the ped history ID attribute.
    /// </summary>
    public class PedHistoryId : VariableDataAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PedHistoryId"/> class.
        /// </summary>
        /// <param name="options">The options for the attribute.</param>
        public PedHistoryId(Options options = default)
        {
            this.Name = "PedHistoryId";
            this.Label = "Ped";
            this.DataType = VariableDataType.Integer;

            this.Required = options.Required;
            this.ShowInGrid = options.ShowInGrid;
            this.Hidden = options.Hidden;
            this.Readonly = options.Readonly;
        }
    }

    /// <summary>
    /// Represents the ped history IDs attribute.
    /// </summary>
    public class PedHistoryIds : VariableDataAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PedHistoryIds"/> class.
        /// </summary>
        /// <param name="options">The options for the attribute.</param>
        public PedHistoryIds(Options options = default)
        {
            this.Name = "PedHistoryIds";
            this.Label = "Peds";
            this.DataType = VariableDataType.IntegerList;

            this.Required = options.Required;
            this.ShowInGrid = options.ShowInGrid;
            this.Hidden = options.Hidden;
            this.Readonly = options.Readonly;
        }
    }
}