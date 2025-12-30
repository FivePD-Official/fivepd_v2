using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using FivePD.Server.API;
using FivePD.Server.Attributes;
using FivePD.Server.Interfaces.Addons;
using FivePD.Server.Interfaces.Lifecycle;
using FivePD.Server.Models;
using FivePD.Server.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace FivePD.Server.Services.Addons;

/// <inheritdoc cref="IAddonMetadataService" />
[Obfuscate]
public class AddonMetadataService : IAddonMetadataService, IInternalAddonMetadataService, IOnInit
{
    private readonly Dictionary<Assembly, AddonDefinition> _definitions = new();
    private readonly List<IExtension> _extensions = new();
    private readonly List<CalloutType> _calloutTypes = new();

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger;
    private readonly CalloutDetailsRepository _calloutDetailsRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddonMetadataService"/> class.
    /// </summary>
    /// <param name="serviceProvider">The <see cref="ServiceProvider"/> to use.</param>
    /// <param name="logger">The <see cref="ILogger"/> to use.</param>
    /// <param name="calloutDetailsRepository">The <see cref="CalloutDetailsRepository"/> to use.</param>
    public AddonMetadataService(
        IServiceProvider serviceProvider,
        ILogger logger,
        CalloutDetailsRepository calloutDetailsRepository)
    {
        this._serviceProvider = serviceProvider;
        this._logger = logger;
        this._calloutDetailsRepository = calloutDetailsRepository;
    }

    /// <inheritdoc />
    public ReadOnlyCollection<Type> CalloutTypes => this._calloutTypes.Select(type => type.Type).ToList().AsReadOnly();

    /// <inheritdoc/>
    public ReadOnlyCollection<IExtension> Extensions => this._extensions.AsReadOnly();

    /// <inheritdoc />
    ReadOnlyCollection<CalloutType> IInternalAddonMetadataService.CalloutTypes => this._calloutTypes.AsReadOnly();

    /// <inheritdoc/>
    ReadOnlyCollection<IExtension> IInternalAddonMetadataService.Extensions => this._extensions.AsReadOnly();

    /// <inheritdoc />
    void IOnInit.OnInit()
    {
        ((IInternalAddonMetadataService)this).CreateCalloutDetailsEntities();
    }

    /// <inheritdoc/>
    Callout IInternalAddonMetadataService.InstantiateCallout(Type type)
    {
        try
        {
            var parameters = type.GetConstructors()[0].GetParameters();
            var callout = (Callout)Activator.CreateInstance(
                type,
                parameters.Select(item => this._serviceProvider.GetService(item.ParameterType)).ToArray());

            return callout;
        }
        catch (Exception e)
        {
            this._logger.Error("Failed to instantiate callout {CalloutType}: {Exception}", type.FullName, e);
            return null;
        }
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

    /// <inheritdoc />
    void IInternalAddonMetadataService.LoadAssemblies(List<AddonDefinition> definitions)
    {
        foreach (var addon in definitions)
        {
            this._logger.ForContext("SourceIdentifier", "AddonLocator").Information(
                "Registering addon \"{AddonName}\" version \"{AddonVersion}\"",
                addon.FriendlyName,
                addon.FriendlyVersion);

            foreach (var assembly in addon.Assemblies)
            {
                var asm = Assembly.Load(assembly);
                this._definitions.Add(asm, addon);
            }
        }
    }

    /// <inheritdoc />
    void IInternalAddonMetadataService.FindAndRegisterAddons()
    {
        foreach (var assembly in this._definitions.Keys)
        {
            // Find and register extensions
            var extensionTypes = assembly
                .GetTypes()
                .Where(type => typeof(IExtension).IsAssignableFrom(type));
            foreach (var extensionType in extensionTypes)
            {
                try
                {
                    var parameters = extensionType.GetConstructors()[0].GetParameters();
                    var extension = Activator.CreateInstance(
                        extensionType,
                        parameters.Select(item => this._serviceProvider.GetService(item.ParameterType)).ToArray()) as IExtension;
                    if (extension is null)
                    {
                        this._logger.Error("Failed to load extension {Extension}", extensionType.FullName);
                        continue;
                    }

                    this._extensions.Add(extension);
                }
                catch (Exception)
                {
                    this._logger.Error("Failed to load extension {Extension}", extensionType.FullName);
                }
            }

            // Find and register callouts
            var calloutsTypes = assembly
                .GetTypes()
                .Where(type => type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(Callout)))
                .Select(type => new CalloutType(type, type.Assembly.GetName().Name))
                .ToList();
            this._calloutTypes.AddRange(calloutsTypes);
        }
    }

    /// <inheritdoc />
    void IInternalAddonMetadataService.CreateCalloutDetailsEntities()
    {
        var calloutDetailsList = this._calloutDetailsRepository.GetAll().ToList();
        foreach (var calloutDetails in calloutDetailsList)
        {
            var active = this._calloutTypes.Any(x => x.Type.Name == calloutDetails.TypeName && x.AssemblyName == calloutDetails.AssemblyName);
            this._calloutDetailsRepository.Update(calloutDetails, x => { x.Active = active; });
        }

        foreach (var type in this._calloutTypes)
        {
            var calloutDetails = calloutDetailsList.FirstOrDefault(x => x.TypeName == type.Type.Name && x.AssemblyName == type.AssemblyName);
            if (calloutDetails is null)
            {
                this._calloutDetailsRepository.Add(new CalloutDetails
                {
                    TypeName = type.Type.Name,
                    AssemblyName = type.AssemblyName,
                    RulesetType = CalloutDetailsRulesetType.Deny,
                    Active = true,
                });
            }
        }
    }
}