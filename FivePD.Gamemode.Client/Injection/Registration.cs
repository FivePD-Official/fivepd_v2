using System;
using System.Collections.Generic;

namespace FivePD.Gamemode.Client.Injection
{
	public class Registration<T> : Registration
	{
		public Registration()
		{
			InstanceType = typeof(T);
			Types.Add(InstanceType);
		}
	}

	public abstract class Registration
	{
		internal Type InstanceType { get; set; }

		internal List<Type> Types { get; } = new List<Type>();

		internal bool IsSingleton { get; set; }

		internal Func<object> Factory { get; set; }
	}
}