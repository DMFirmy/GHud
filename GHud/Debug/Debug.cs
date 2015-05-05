#if DEBUG
using System;
// ReSharper disable All

namespace UnityEngine
{
	/// <summary>
	///     For DEBUG builds we need to override the <see cref="UnityEngine.Debug" /> class.
	/// </summary>
	public class Debug
	{
		public static void Log(string s)
		{
			Console.WriteLine(s);
		}

		public static void LogWarning(string s)
		{
			Console.WriteLine(s);
		}

		public static void LogError(string s)
		{
			Console.WriteLine(s);
		}
	}
}

#endif