using System.Collections.Generic;
using System.Reflection;
using FivePD.Server.Models;

namespace FivePD.Server.Utilities;

/// <summary>
/// Provides access to a vehicle config options to dictionary converter.
/// </summary>
public static class VehicleConfigOptionsConverter
{
    /// <summary>
    /// Converts options to a Dictionary&lt;string, object&gt; and returns a new VehicleConfig instance with options converted.
    /// </summary>
    /// <typeparam name="TOptions">The type of options to convert.</typeparam>
    /// <param name="vehicleConfig">The original VehicleConfig instance.</param>
    /// <returns>A new VehicleConfig instance with options converted to a Dictionary&lt;string, object&gt;.</returns>
    public static VehicleConfig<Dictionary<string, object>> ConvertOptionsToDictionary<TOptions>(VehicleConfig<TOptions> vehicleConfig)
    {
        var dictionary = new Dictionary<string, object>();

        // Get all public properties of TOptions
        PropertyInfo[] properties = typeof(TOptions).GetProperties();

        foreach (PropertyInfo property in properties)
        {
            // Add property name and value to the dictionary
            dictionary[property.Name] = property.GetValue(vehicleConfig.Options);
        }

        // Create a new VehicleConfig with options converted to a Dictionary<string, object>
        return new VehicleConfig<Dictionary<string, object>>
        {
            Id = vehicleConfig.Id,
            Type = vehicleConfig.Type,
            Name = vehicleConfig.Name,
            Model = vehicleConfig.Model,
            Livery = vehicleConfig.Livery,
            Extras = vehicleConfig.Extras,
            Options = dictionary,
        };
    }
}