using System.Collections.Generic;
using System.Drawing;
using GHud.DataStructures;
using GHud.Modules;

namespace GHud.Extensions
{
	// ReSharper disable once InconsistentNaming
	/// <summary>
	///		Extension methods for the IEnumerable interface.
	/// </summary>
	public static class IEnumerableExtensions
	{
		#region Public Methods
		/// <summary>
		///		This method is used to render a collection of <see cref="DisplayModule"/> instances for the provided <see cref="Vessel"/>.
		/// </summary>
		/// <param name="modules">The <see cref="IEnumerable{DisplayModule}"/> being extended.</param>
		public static void Render(this IEnumerable<DisplayModule> modules)
		{
#if !DEBUG
			if (FlightGlobals.ActiveVessel == null)
			{
				return;
			}
			var vessel = FlightGlobals.ActiveVessel;
#endif
			foreach (var dmod in modules)
			{
#if !DEBUG
				var target = FlightGlobals.fetch.VesselTarget;
				Orbit orbit = null;
				var objName = "Unknown";

				if (dmod.IsTargetTypeModule)
				{
					if (target == null)
					{
						dmod.ModuleMsg("No Target", new Rectangle(0, 0, 0, 0));
					}
					else
					{
						orbit = target.GetOrbit();
						objName = target.GetName();
					}
				}
				else
				{
					orbit = vessel.orbit;
					objName = vessel.GetName();
				}

				if (orbit != null)
				{
					dmod.SetOrbit(new OrbitData(orbit), objName);
				}
#endif
				dmod.Render(new Rectangle(0, 0, 0, 0));
			}
		}
		#endregion
	}
}
