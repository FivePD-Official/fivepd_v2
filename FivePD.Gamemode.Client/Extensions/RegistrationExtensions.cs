using FivePD.Gamemode.Client.Injection;

namespace FivePD.Gamemode.Client.Extensions
{
	public static class RegistrationExtensions
	{
		public static Registration IsSingleton(this Registration registration)
		{
			registration.IsSingleton = true;

			return registration;
		}

		public static Registration As<T>(this Registration registration)
		{
			var type = typeof(T);
			if (!registration.Types.Contains(type))
				registration.Types.Add(type);

			return registration;
		}
	}
}