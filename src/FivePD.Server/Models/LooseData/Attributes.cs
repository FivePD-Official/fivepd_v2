namespace FivePD.Server.Models.LooseData;

#pragma warning disable

public class Attributes
{
    public class DepartmentId : LooseDataAttribute
    {
        public DepartmentId()
        {
            this.Name = "DepartmentId";
            this.Label = "Department";
            this.DataType = LooseDataType.Integer;
            this.Required = false;
            this.ShowInGrid = false;
            this.Readonly = true;
        }
    }

    public class DepartmentIds : LooseDataAttribute
    {
        public DepartmentIds()
        {
            this.Name = "DepartmentIds";
            this.Label = "Departments";
            this.DataType = LooseDataType.IntegerList;
            this.Required = false;
            this.ShowInGrid = false;
            this.Readonly = true;
        }
    }

    public class UserId : LooseDataAttribute
    {
        public UserId()
        {
            this.Name = "UserId";
            this.Label = "User";
            this.DataType = LooseDataType.Integer;
            this.Required = false;
            this.ShowInGrid = false;
            this.Readonly = true;
        }
    }

    public class UserIds : LooseDataAttribute
    {
        public UserIds()
        {
            this.Name = "UserIds";
            this.Label = "Users";
            this.DataType = LooseDataType.IntegerList;
            this.Required = false;
            this.ShowInGrid = false;
            this.Readonly = true;
        }
    }

    public class IncidentHistoryId : LooseDataAttribute
    {
        public IncidentHistoryId()
        {
            this.Name = "IncidentHistoryId";
            this.Label = "Incident";
            this.DataType = LooseDataType.Integer;
            this.Required = false;
            this.ShowInGrid = false;
            this.Readonly = true;
        }
    }

    public class IncidentHistoryIds : LooseDataAttribute
    {
        public IncidentHistoryIds()
        {
            this.Name = "IncidentHistoryIds";
            this.Label = "Incidents";
            this.DataType = LooseDataType.IntegerList;
            this.Required = false;
            this.ShowInGrid = false;
            this.Readonly = true;
        }
    }

    public class PedHistoryId : LooseDataAttribute
    {
        public PedHistoryId()
        {
            this.Name = "PedHistoryId";
            this.Label = "Ped";
            this.DataType = LooseDataType.Integer;
            this.Required = false;
            this.ShowInGrid = false;
            this.Readonly = true;
        }
    }

    public class PedHistoryIds : LooseDataAttribute
    {
        public PedHistoryIds()
        {
            this.Name = "PedHistoryIds";
            this.Label = "Peds";
            this.DataType = LooseDataType.IntegerList;
            this.Required = false;
            this.ShowInGrid = false;
            this.Readonly = true;
        }
    }
}