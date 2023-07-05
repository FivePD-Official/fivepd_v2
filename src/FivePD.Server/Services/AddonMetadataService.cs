using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using FivePD.Server.Interfaces;
using FivePD.Server.Models;

namespace FivePD.Server.Services;

/// <inheritdoc />
public class AddonMetadataService : IAddonMetadataService
{
    private readonly Dictionary<Assembly, AddonDefinition> _definitions = new();

    /// <inheritdoc />
    public void RegisterAddonAssemblyWithDefinition(Assembly assembly, AddonDefinition definition)
    {
        this._definitions[assembly] = definition;
    }

    /// <inheritdoc />
    public AddonDefinition GetExecutingAddonDefinition()
    {
        // Technically, the executing assembly is the server. Sort through the stack trace and find a ref. to the actual addon
        foreach (var frame in new StackTrace().GetFrames())
        {
            try
            {
                if (frame.GetMethod().DeclaringType!.Assembly != Assembly.GetExecutingAssembly())
                {
                    var declaringTypeAssembly = frame.GetMethod().DeclaringType?.Assembly;
                    if (declaringTypeAssembly is not null)
                    {
                        return this._definitions[declaringTypeAssembly];
                    }
                }
            }
            catch (Exception)
            {
                // Continues when frame assembly is null
            }
        }

        return null;
    }
}