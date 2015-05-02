using System;

namespace testGHud
{
	/// <summary>
	///		A simple console application for testing the GHud KSP plugin.
	/// </summary>
	static class Program
	{
		// ReSharper disable once UnusedParameter.Local
		static void Main(string[] args)
		{
#if !DEBUG
			Console.WriteLine(@"This program can only be run for a DEBUG build.");
			Console.ReadKey();
#else
			Console.WriteLine(@"Press ESC to stop");
			Console.WriteLine(System.IO.Directory.GetCurrentDirectory());
			var hud = new GHud.GHud();
			hud.Awake();
			do
			{
				while (!Console.KeyAvailable)
				{
					hud.Update();
				}
			} while (Console.ReadKey(true).Key != ConsoleKey.Escape);
			hud.OnDestroy();
#endif
		}
	}
}
