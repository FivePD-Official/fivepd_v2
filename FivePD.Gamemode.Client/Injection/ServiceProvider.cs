using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FivePD.Gamemode.Client.Injection
{
	public class ServiceProvider
	{
		private readonly List<Registration> _registrations;
		private readonly Dictionary<string, object> _instances = new Dictionary<string, object>();

		internal ServiceProvider(List<Registration> registrations)
		{
			_registrations = registrations;
		}

		private object ResolveSingleInstance(Registration registration)
		{
			var assemblyQualifiedName = registration.InstanceType.AssemblyQualifiedName;

			if (_instances.ContainsKey(assemblyQualifiedName))
			{
				return _instances[assemblyQualifiedName];
			}

			var instance = CreateInstance(registration);
			_instances.Add(assemblyQualifiedName, instance);

			return instance;
		}

		private object CreateInstance(Registration registration)
		{
			if (registration.Factory != null)
				return registration.Factory();

			var constructor = registration.InstanceType.GetConstructors().FirstOrDefault();
			var parameters = constructor.GetParameters().Select(ResolveConstructorParameter).ToArray();

			var instance = Activator.CreateInstance(registration.InstanceType, parameters);
			
			return instance;
		}

		private object ResolveConstructorParameter(ParameterInfo parameterInfo)
		{
			return Resolve(parameterInfo.ParameterType);
		}

		public object Resolve(Type type)
		{
			var matchingRegistration = _registrations.SingleOrDefault(r => r.Types.Any(t => t == type));

			if (matchingRegistration == null)
				throw new Exception("Could not resolve type " + type.AssemblyQualifiedName);

			if (matchingRegistration.IsSingleton)
				return ResolveSingleInstance(matchingRegistration);

			return CreateInstance(matchingRegistration);
		}

		public T Resolve<T>()
		{
			var resolveType = typeof(T);
			return (T) Resolve(resolveType);
		}
	}
}
