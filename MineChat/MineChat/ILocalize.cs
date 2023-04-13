using System;
using System.Globalization;

namespace MineChat
{
	public interface ILocalize
	{
		CultureInfo GetCurrentCultureInfo ();
		void SetLocale ();
	}
}

