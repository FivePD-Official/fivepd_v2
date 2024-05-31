using System;
using System.Collections.Generic;

#pragma warning disable

namespace FivePD.UnitTests.Helpers;

internal class MockServiceProvider : IServiceProvider
{
    private readonly Dictionary<Type, object> _mockServices = new();

    public void AddService<T>(T service)
    {
        this._mockServices.Add(typeof(T), service);
    }

    public object GetService(Type serviceType)
    {
        return this._mockServices[serviceType];
    }
}