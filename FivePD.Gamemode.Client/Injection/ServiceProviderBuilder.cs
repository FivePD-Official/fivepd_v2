using System;
using System.Collections.Generic;
using System.Linq;

namespace FivePD.Gamemode.Client.Injection
{
	public class ServiceProviderBuilder
	{
		private readonly List<Registration> _registration = new List<Registration>();

		public Registration<T> RegisterType<T>()
		{
			var type = typeof(T);
			var registration = new Registration<T>();

			if (_registration.Any(r => r.Types.Any(t => t == type)))
				throw new Exception($"Type {type.AssemblyQualifiedName} already registered.");

			_registration.Add(registration);

			return registration;
		}

		public Registration<T> Register<T>(Func<T> factory)
		{
			var type = typeof(T);
			var registration = new Registration<T>
			{
				Factory = () => factory()
			};

			if (_registration.Any(r => r.Types.Any(t => t == type)))
				throw new Exception($"Type {type.AssemblyQualifiedName} already registered.");

			_registration.Add(registration);

			return registration;
		}

		public ServiceProvider Build()
		{
			return new ServiceProvider(_registration);
		}
	}
}