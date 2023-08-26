using System.Collections.Generic;
using FivePD.Common.Enums;
using FivePD.Server.Interfaces.LooseData;
using FivePD.Server.Models;
using FivePD.Server.Models.LooseData;

namespace FivePD.Server.Services.LooseData;

/// <inheritdoc />
public class ArrestReportService : LooseDataRepository<ArrestReportSet>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ArrestReportService"/> class.
    /// </summary>
    /// <param name="looseDataService">The <see cref="ILooseDataService"/> to use.</param>
    public ArrestReportService(ILooseDataService looseDataService)
        : base(looseDataService)
    {
    }

    /// <inheritdoc />
    protected override List<LooseDataAttribute> RegisterSetAttributes()
    {
        return new List<LooseDataAttribute>
        {
            new()
            {
                Name = "DepartmentId",
                Label = "Department",
                DataType = LooseDataType.Integer,
                Required = false,
                ShowInGrid = false,
                Readonly = true,
            },
            new()
            {
                Name = "DateOfSubmit",
                Label = "Date of submit",
                DataType = LooseDataType.Date,
                Required = false,
                ShowInGrid = true,
                Readonly = true,
            },
            new()
            {
                Name = "DateOfArrest",
                Label = "Date of arrest",
                DataType = LooseDataType.Date,
                Required = true,
                ShowInGrid = true,
            },
            new()
            {
                Name = "AddressOfArrest",
                Label = "Address of arrest",
                DataType = LooseDataType.ShortString,
                Required = true,
            },
            new()
            {
                Name = "Weapons",
                Label = "Weapons",
                DataType = LooseDataType.ShortString,
                Required = false,
            },
            new()
            {
                Name = "Narrative",
                Label = "Narrative",
                DataType = LooseDataType.LongString,
                Required = true,
            },
            new()
            {
                Name = "ArresteeName",
                Label = "Name of arrestee",
                DataType = LooseDataType.ShortString,
                Required = true,
                ShowInGrid = true,
                Order = 1,
            },
            new()
            {
                Name = "ArresteeBirthdate",
                Label = "Birthdate of arrestee",
                DataType = LooseDataType.Date,
                Required = true,
            },
            new()
            {
                Name = "ArresteeAge",
                Label = "Age of arrestee",
                DataType = LooseDataType.Integer,
                Required = true,
            },
            new()
            {
                Name = "ArresteeGender",
                Label = "Gender of arrestee",
                DataType = LooseDataType.Integer,
                Required = true,
                Options = new LooseDataAttribute.AttributeOptions
                {
                    Items = new Dictionary<int, string>
                    {
                        { (int)FGender.Male, "Male" },
                        { (int)FGender.Female, "Female" },
                    },
                },
            },
            new()
            {
                Name = "ArresteeRace",
                Label = "Race of arrestee",
                DataType = LooseDataType.ShortString,
                Required = true,
            },
        };
    }
}