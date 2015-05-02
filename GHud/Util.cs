using System;

namespace GHud
{
	// This class contains a few utility functions for getting display units.   Borrwed form Mechjeb
	public static class Util
	{
		#region Methods
		//From http://svn.xMuMech.com/KSP/trunk/xMuMechLib/MuUtils.cs
		// ReSharper disable once RedundantAssignment
		public static string xMuMech_ToSI(double d, ref string suffix)
		{
			const int digits = 2;
			var exponent = Math.Log10(Math.Abs(d));
			if (Math.Abs(d) >= 1)
			{
				switch ((int)Math.Floor(exponent))
				{
					case 0:
					case 1:
					case 2:
						suffix = "";
						return d.ToString("F" + digits);
					case 3:
					case 4:
					case 5:
						suffix = "k";
						return (d / 1e3).ToString("F" + digits);
					case 6:
					case 7:
					case 8:
						suffix = "M";
						return (d / 1e6).ToString("F" + digits);
					case 9:
					case 10:
					case 11:
						suffix = "G";
						return (d / 1e9).ToString("F" + digits);
					case 12:
					case 13:
					case 14:
						suffix = "T";
						return (d / 1e12).ToString("F" + digits);
					case 15:
					case 16:
					case 17:
						suffix = "P";
						return (d / 1e15).ToString("F" + digits);
					case 18:
					case 19:
					case 20:
						suffix = "E";
						return (d / 1e18).ToString("F" + digits);
					case 21:
					case 22:
					case 23:
						suffix = "Z";
						return (d / 1e21).ToString("F" + digits);
					default:
						suffix = "Y";
						return (d / 1e24).ToString("F" + digits);
				}
			}
			if (Math.Abs(d) > 0)
			{
				switch ((int)Math.Floor(exponent))
				{
					case -1:
					case -2:
					case -3:
						suffix = "m";
						return (d * 1e3).ToString("F" + digits);
					case -4:
					case -5:
					case -6:
						suffix = "μ";
						return (d * 1e6).ToString("F" + digits);
					case -7:
					case -8:
					case -9:
						suffix = "n";
						return (d * 1e9).ToString("F" + digits);
					case -10:
					case -11:
					case -12:
						suffix = "p";
						return (d * 1e12).ToString("F" + digits);
					case -13:
					case -14:
					case -15:
						suffix = "f";
						return (d * 1e15).ToString("F" + digits);
					case -16:
					case -17:
					case -18:
						suffix = "a";
						return (d * 1e18).ToString("F" + digits);
					case -19:
					case -20:
					case -21:
						suffix = "z";
						return (d * 1e21).ToString("F" + digits);
					default:
						suffix = "y";
						return (d * 1e24).ToString("F" + digits);
				}
			}
			suffix = "";
			return "0";
		}

		public static string ConvertInterval(double seconds, bool doYears)
		{
			const string format1 = "{0:D1}y {1:D1}d {2:D2}:{3:D2}:{4:D2}";
			const string format2 = "{0:D1}d {1:D2}:{2:D2}:{3:D2}";
			const string format3 = "{0:D2}:{1:D2}:{2:D2}";

			var interval = TimeSpan.FromSeconds(seconds);
			var years = interval.Days / 365;

			string output;
			if (years > 0 && doYears)
			{
				output = string.Format(format1,
					years,
					interval.Days - (years * 365), //  subtract years * 365 for accurate day count
					interval.Hours,
					interval.Minutes,
					interval.Seconds);
			}
			else if (interval.Days > 0)
			{
				output = string.Format(format2,
					interval.Days,
					interval.Hours,
					interval.Minutes,
					interval.Seconds);
			}
			else
			{
				output = string.Format(format3,
					interval.Hours,
					interval.Minutes,
					interval.Seconds);
			}
			return output;
		}
		#endregion
	}
}