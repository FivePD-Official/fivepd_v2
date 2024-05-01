namespace FivePD.Server.Models.VariableData;

#pragma warning disable

public class CoreAttributes
{
    public readonly record struct Options(bool Hidden, bool Required, bool Readonly, bool ShowInGrid); 
    
    public class SubmitDate : VariableDataAttribute
    {
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

    public class DepartmentId : VariableDataAttribute
    {
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

    public class DepartmentIds : VariableDataAttribute
    {
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

    public class UserId : VariableDataAttribute
    {
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

    public class UserIds : VariableDataAttribute
    {
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

    public class IncidentHistoryId : VariableDataAttribute
    {
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

    public class IncidentHistoryIds : VariableDataAttribute
    {
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

    public class PedHistoryId : VariableDataAttribute
    {
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

    public class PedHistoryIds : VariableDataAttribute
    {
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