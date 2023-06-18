using System;
using System.Linq.Expressions;
using System.Reflection;
using FivePD.Gamemode.Common.ConfigModels.GeneralConfigs;
using FivePD.Gamemode.Server.Interfaces;

namespace FivePD.Gamemode.Server.Services;

/// <inheritdoc />
public class FeatureFlagService : IFeatureFlagService
{
    private readonly FeatureFlagsConfig _featureFlagsConfig;

    /// <summary>
    /// Initializes a new instance of the <see cref="FeatureFlagService"/> class.
    /// </summary>
    /// <param name="configService">The <see cref="IConfigService"/> to use.</param>
    public FeatureFlagService(IConfigService configService)
    {
        this._featureFlagsConfig = configService.GeneralConfig.FeatureFlags;
    }

    /// <inheritdoc />
    public bool IsFeatureEnabled(Expression<Func<FeatureFlagsConfig, bool>> selector)
    {
        return this.GetNestedPropertyValue(this._featureFlagsConfig, selector);
    }

    private bool GetNestedPropertyValue(FeatureFlagsConfig obj, Expression<Func<FeatureFlagsConfig, bool>> expression)
    {
        if (expression.Body is not MemberExpression memberExpression)
        {
            return false;
        }

        if (memberExpression.Member is not PropertyInfo propertyInfo)
        {
            return false;
        }

        var getMethod = propertyInfo.GetGetMethod();
        if (getMethod is null)
        {
            return false;
        }

        var func = expression.Compile();
        return func(obj);
    }
}