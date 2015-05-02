using System;
using System.Collections.Generic;
using System.Drawing;
using Color = System.Drawing.Color;
#if !DEBUG
using UnityEngine;
#endif

namespace GHud
{
	[KSPAddon(KSPAddon.Startup.Instantly, true)]
	public class GHud : MonoBehaviour
	{
		#region Fields
		// ReSharper disable once FieldCanBeMadeReadOnly.Local
		private List<Device> _devices = new List<Device>();
		private static GHud _gHudMain;
		private float _lastUpdate;
		private int _config;
		private bool _lcdInitialized;
		#endregion

		#region Methods
		public void OnDestroy()
		{
			foreach (var dev in _devices)
			{
				dev.Dispose();
			}
			if (_lcdInitialized)
			{
				NativeMethods.LcdDeInit();
			}
		}

		public void CfgCallback(int connection)
		{
			_config = connection;
		}

		private static void ButtonUp(object sender, EventArgs e)
		{
		}

		private static void ButtonDown(object sender, EventArgs e)
		{
		}

		private static void ButtonLeft(object sender, EventArgs e)
		{
		}

		private static void ButtonRight(object sender, EventArgs e)
		{
		}

		private static void ButtonOk(object sender, EventArgs e)
		{
		}

		private static void ButtonCancel(object sender, EventArgs e)
		{
		}

		// Cycle through the existing modules
		private static void ButtonMenu(object sender, EventArgs e)
		{
			var dev = sender as Device;
			if (dev == null)
			{
				return;
			}

			var activate = false;
			var activated = false;
			foreach (var dmod in dev.Modules)
			{
				if (activate)
				{
					dmod.Activate();
					activated = true;
					break;
				}
				if (!dmod.IsActive)
				{
					continue;
				}
				activate = true;
				dmod.Deactivate();
			}

			if (!activated)
			{
				dev.Modules[0].Activate();
			}
		}

		public void Awake()
		{
			if (_gHudMain != null)
			{
				return;
			}
			_gHudMain = this;
#if !DEBUG
			DontDestroyOnLoad(_gHudMain);
#endif
			if (!_lcdInitialized)
			{
				NativeMethods.LcdInit();
				_lcdInitialized = true;
			}

			const string vesselMonicer = "✈";
			const string targetMonicer = "+";

			#region Black & White Device
			Device device = new DeviceBw();
			if (device.IsValid())
			{
				_devices.Add(device);

				var orbitInfoColor = Color.Black;
				var orbitGraphColor = Color.Yellow;

				var vesselInfo = new OrbitInfo(device, vesselMonicer, orbitInfoColor, orbitInfoColor);
				vesselInfo.Activate();
				device.Modules.Add(vesselInfo);

				var targetinfo = new OrbitInfo(device, targetMonicer, orbitInfoColor, orbitInfoColor)
				{
					IsTargetTypeModule = true
				};
				device.Modules.Add(targetinfo);

				var vesselGraph = new OrbitGraph(device, orbitGraphColor, vesselMonicer);
				device.Modules.Add(vesselGraph);

				var targetGraph = new OrbitGraph(device, orbitGraphColor, targetMonicer)
				{
					IsTargetTypeModule = true
				};
				device.Modules.Add(targetGraph);
			}
			#endregion

			#region Color Device
			device = new DeviceQvga();
			if (device.IsValid())
			{
				_devices.Add(device);

				var vesselInfo = new VesselInfo(device);
				vesselInfo.Activate();
				device.Modules.Add(vesselInfo);

				var vessleGraphColor = Color.Yellow;
				var vessleGraph = new OrbitGraph(device, vessleGraphColor, vesselMonicer);
				device.Modules.Add(vessleGraph);

				var targetGraphColor = Color.LightBlue;
				var targetGraph = new OrbitGraph(device, targetGraphColor, targetMonicer)
				{
					IsTargetTypeModule = true
				};
				device.Modules.Add(targetGraph);
			}
			#endregion

			foreach (var dev in _devices)
			{
				dev.ButtonUp += ButtonUp;
				dev.ButtonDown += ButtonDown;
				dev.ButtonLeft += ButtonLeft;
				dev.ButtonRight += ButtonRight;
				dev.ButtonOk += ButtonOk;
				dev.ButtonCancel += ButtonCancel;
				dev.ButtonMenu += ButtonMenu;

				dev.DisplayFrame();
			}
		}

		/// <summary>
		///     This method gets called each frame to update the LCD display.
		/// </summary>
		public void Update()
		{
#if !DEBUG
			var updateDelta = Time.time - _lastUpdate;

			if (updateDelta < 0.2f)
			{
				return;
			}
			_lastUpdate = Time.time;

			var vessel = FlightGlobals.ActiveVessel;
			if (vessel == null)
			{
				foreach (var dev in _devices)
				{
					dev.ClearLcd("Waiting for Flight...");
					dev.DisplayFrame();
				}
				return;
			}
#endif
			foreach (var dev in _devices)
			{
				dev.ClearLcd("");
				dev.DoButtons();

				foreach (var dmod in dev.Modules)
				{
#if !DEBUG
	// TODO: This needs a rewrite.  All this crap should be done in the display classes.
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
						dmod.SetOrbit(orbit, objName);
					}
#endif
					dmod.Render(new Rectangle(0, 0, 0, 0));
				}

				dev.DisplayFrame();
			}

#if !DEBUG
			_lastUpdate = Time.time;
#endif
		}
		#endregion
	}
}