using System.Collections.Generic;
using System.Linq;

namespace FivePD.Server.Database;

#pragma warning disable

public class ChangeTracker
{
    private readonly HashSet<string> _changedProperties = new();

    protected void SetField<T>(ref T field, T value, string propertyName)
    {
        field = value;
        this._changedProperties.Add(propertyName);
    }

    public bool IsChanged(string propertyName) => this._changedProperties.Contains(propertyName);

    public bool HasChanges() => this._changedProperties.Any();

    public IEnumerable<string> GetChangedProperties() => this._changedProperties;
}