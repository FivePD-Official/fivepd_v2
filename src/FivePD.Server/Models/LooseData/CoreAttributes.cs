namespace FivePD.Server.Models.LooseData;

#pragma warning disable

public class CoreAttributes
{
    public readonly record struct Options(bool Hidden, bool Required, bool Readonly, bool ShowInGrid); 
    
    public class SubmitDate : LooseDataAttribute
    {
        public SubmitDate(Options options = default)
        {
            this.Name = "SubmitDate";
            this.Label = "Date of submit";
            this.DataType = LooseDataType.Date;

            this.Required = options.Required;
            this.ShowInGrid = options.ShowInGrid;
            this.Hidden = options.Hidden;
            this.Readonly = options.Readonly;
        }
    }

    public class DepartmentId : LooseDataAttribute
    {
        public DepartmentId(Options options = default)
        {
            this.Name = "DepartmentId";
            this.Label = "Department";
            this.DataType = LooseDataType.Integer;

            this.Required = options.Required;
            this.ShowInGrid = options.ShowInGrid;
            this.Hidden = options.Hidden;
            this.Readonly = options.Readonly;
        }
    }

    public class DepartmentIds : LooseDataAttribute
    {
        public DepartmentIds(Options options = default)
        {
            this.Name = "DepartmentIds";
            this.Label = "Departments";
            this.DataType = LooseDataType.IntegerList;

            this.Required = options.Required;
            this.ShowInGrid = options.ShowInGrid;
            this.Hidden = options.Hidden;
            this.Readonly = options.Readonly;
        }
    }

    public class UserId : LooseDataAttribute
    {
        public UserId(Options options = default)
        {
            this.Name = "UserId";
            this.Label = "User";
            this.DataType = LooseDataType.Integer;

            this.Required = options.Required;
            this.ShowInGrid = options.ShowInGrid;
            this.Hidden = options.Hidden;
            this.Readonly = options.Readonly;
        }
    }

    public class UserIds : LooseDataAttribute
    {
        public UserIds(Options options = default)
        {
            this.Name = "UserIds";
            this.Label = "Users";
            this.DataType = LooseDataType.IntegerList;

            this.Required = options.Required;
            this.ShowInGrid = options.ShowInGrid;
            this.Hidden = options.Hidden;
            this.Readonly = options.Readonly;
        }
    }

    public class IncidentHistoryId : LooseDataAttribute
    {
        public IncidentHistoryId(Options options = default)
        {
            this.Name = "IncidentHistoryId";
            this.Label = "Incident";
            this.DataType = LooseDataType.Integer;

            this.Required = options.Required;
            this.ShowInGrid = options.ShowInGrid;
            this.Hidden = options.Hidden;
            this.Readonly = options.Readonly;
        }
    }

    public class IncidentHistoryIds : LooseDataAttribute
    {
        public IncidentHistoryIds(Options options = default)
        {
            this.Name = "IncidentHistoryIds";
            this.Label = "Incidents";
            this.DataType = LooseDataType.IntegerList;

            this.Required = options.Required;
            this.ShowInGrid = options.ShowInGrid;
            this.Hidden = options.Hidden;
            this.Readonly = options.Readonly;
        }
    }

    public class PedHistoryId : LooseDataAttribute
    {
        public PedHistoryId(Options options = default)
        {
            this.Name = "PedHistoryId";
            this.Label = "Ped";
            this.DataType = LooseDataType.Integer;

            this.Required = options.Required;
            this.ShowInGrid = options.ShowInGrid;
            this.Hidden = options.Hidden;
            this.Readonly = options.Readonly;
        }
    }

    public class PedHistoryIds : LooseDataAttribute
    {
        public PedHistoryIds(Options options = default)
        {
            this.Name = "PedHistoryIds";
            this.Label = "Peds";
            this.DataType = LooseDataType.IntegerList;

            this.Required = options.Required;
            this.ShowInGrid = options.ShowInGrid;
            this.Hidden = options.Hidden;
            this.Readonly = options.Readonly;
        }
    }
}