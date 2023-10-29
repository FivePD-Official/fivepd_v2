namespace FivePD.Server.Models.LooseData;

#pragma warning disable

public class Attributes
{
    public class SubmitDate : LooseDataAttribute
    {
        public SubmitDate(bool hidden)
        {
            this.Name = "SubmitDate";
            this.Label = "Date of submit";
            this.DataType = LooseDataType.Date;
            this.Required = false;
            this.ShowInGrid = true;
            this.Hidden = hidden;
        }
    }

    public class DepartmentId : LooseDataAttribute
    {
        public DepartmentId(bool hidden)
        {
            this.Name = "DepartmentId";
            this.Label = "Department";
            this.DataType = LooseDataType.Integer;
            this.Required = false;
            this.ShowInGrid = false;
            this.Hidden = hidden;
        }
    }

    public class DepartmentIds : LooseDataAttribute
    {
        public DepartmentIds(bool hidden)
        {
            this.Name = "DepartmentIds";
            this.Label = "Departments";
            this.DataType = LooseDataType.IntegerList;
            this.Required = false;
            this.ShowInGrid = false;
            this.Hidden = hidden;
        }
    }

    public class UserId : LooseDataAttribute
    {
        public UserId(bool hidden)
        {
            this.Name = "UserId";
            this.Label = "User";
            this.DataType = LooseDataType.Integer;
            this.Required = false;
            this.ShowInGrid = false;
            this.Hidden = hidden;
        }
    }

    public class UserIds : LooseDataAttribute
    {
        public UserIds(bool hidden)
        {
            this.Name = "UserIds";
            this.Label = "Users";
            this.DataType = LooseDataType.IntegerList;
            this.Required = false;
            this.ShowInGrid = false;
            this.Hidden = hidden;
        }
    }

    public class IncidentHistoryId : LooseDataAttribute
    {
        public IncidentHistoryId(bool hidden)
        {
            this.Name = "IncidentHistoryId";
            this.Label = "Incident";
            this.DataType = LooseDataType.Integer;
            this.Required = false;
            this.ShowInGrid = false;
            this.Hidden = hidden;
        }
    }

    public class IncidentHistoryIds : LooseDataAttribute
    {
        public IncidentHistoryIds(bool hidden)
        {
            this.Name = "IncidentHistoryIds";
            this.Label = "Incidents";
            this.DataType = LooseDataType.IntegerList;
            this.Required = false;
            this.ShowInGrid = false;
            this.Hidden = hidden;
        }
    }

    public class PedHistoryId : LooseDataAttribute
    {
        public PedHistoryId(bool hidden)
        {
            this.Name = "PedHistoryId";
            this.Label = "Ped";
            this.DataType = LooseDataType.Integer;
            this.Required = false;
            this.ShowInGrid = false;
            this.Hidden = hidden;
        }
    }

    public class PedHistoryIds : LooseDataAttribute
    {
        public PedHistoryIds(bool hidden)
        {
            this.Name = "PedHistoryIds";
            this.Label = "Peds";
            this.DataType = LooseDataType.IntegerList;
            this.Required = false;
            this.ShowInGrid = false;
            this.Hidden = hidden;
        }
    }
}