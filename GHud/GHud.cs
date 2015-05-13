using System.Linq;
using GHud.Extensions;
using System;
using System.Collections.Generic;
using GHud.Devices;
using GHud.Modules;
using Color = System.Drawing.Color;
#if !DEBUG
using UnityEngine;
#endif
// ReSharper disable UnusedMember.Global

namespace GHud
{
	/// <summary>
	///     This class handles interaction with the keyboard device and which <see cref="DisplayModule" /> instance is currently being displayed.
	/// </summary>
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

		#region Public Methods
		/// <summary>
		/// This is the config callback for the <see cref="NativeMethods.LcdSetConfigCallback"/> method.
		/// </summary>
		/// <param name="connection">The connection that is issuing the callback.</param>
		public void CfgCallback(int connection)
		{
			// NOTE: This method seems to be unused.
			_config = connection;
		}

		/// <summary>
		///     Awake is called when the script instance is being loaded.
		/// </summary>
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

				DisplayModule module;
				module = new VesselInfo(device);
				device.Modules.Add(module);
				module.Activate();
				
				var vessleGraphColor = Color.Yellow;
				module = new OrbitGraph(device, vessleGraphColor, vesselMonicer);
				device.Modules.Add(module);

				var targetGraphColor = Color.LightBlue;
				module = new OrbitGraph(device, targetGraphColor, targetMonicer)
				{
					IsTargetTypeModule = true
				};
				device.Modules.Add(module);
			}
			#endregion

			#region Initiialize Devices
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
			#endregion
		}

		/// <summary>
		///     Update is called every frame, if the MonoBehaviour is enabled.
		/// </summary>
		public void Update()
		{
#if !DEBUG
			var updateDelta = Time.time - _lastUpdate;
			if (updateDelta < 0.2f)
			{
				return;
			}

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
				dev.ClearLcd();
				dev.DoButtons();
				dev.Modules.Render();
				dev.DisplayFrame();
			}
#if !DEBUG
			_lastUpdate = Time.time;
#endif
		}

		/// <summary>
		///     This function is called when the MonoBehaviour will be destroyed.
		/// </summary>
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
		#endregion

		#region Private Methods
		/// <summary>
		///     Handles the press of the "Up" button on the keyboard.
		/// </summary>
		/// <param name="sender">The <see cref="Device" /> object that had the button pressed.</param>
		/// <param name="e">Any <see cref="EventArgs" /> that are associated with this button press event.</param>
		private static void ButtonUp(object sender, EventArgs e)
		{
		}

		/// <summary>
		///     Handles the press of the "Down" button on the keyboard.
		/// </summary>
		/// <param name="sender">The <see cref="Device" /> object that had the button pressed.</param>
		/// <param name="e">Any <see cref="EventArgs" /> that are associated with this button press event.</param>
		private static void ButtonDown(object sender, EventArgs e)
		{
		}

		/// <summary>
		///     Handles the press of the "Left" button on the keyboard.
		/// </summary>
		/// <param name="sender">The <see cref="Device" /> object that had the button pressed.</param>
		/// <param name="e">Any <see cref="EventArgs" /> that are associated with this button press event.</param>
		private static void ButtonLeft(object sender, EventArgs e)
		{
		}

		/// <summary>
		///     Handles the press of the "Right" button on the keyboard.
		/// </summary>
		/// <param name="sender">The <see cref="Device" /> object that had the button pressed.</param>
		/// <param name="e">Any <see cref="EventArgs" /> that are associated with this button press event.</param>
		private static void ButtonRight(object sender, EventArgs e)
		{
		}

		/// <summary>
		///     Handles the press of the "Ok" button on the keyboard.
		/// </summary>
		/// <param name="sender">The <see cref="Device" /> object that had the button pressed.</param>
		/// <param name="e">Any <see cref="EventArgs" /> that are associated with this button press event.</param>
		private static void ButtonOk(object sender, EventArgs e)
		{
		}

		/// <summary>
		///     Handles the press of the "Cancel" button on the keyboard.
		/// </summary>
		/// <param name="sender">The <see cref="Device" /> object that had the button pressed.</param>
		/// <param name="e">Any <see cref="EventArgs" /> that are associated with this button press event.</param>
		private static void ButtonCancel(object sender, EventArgs e)
		{
		}

		/// <summary>
		///     Handles the press of the "Menu" button on the keyboard.
		/// </summary>
		/// <param name="sender">The <see cref="Device" /> object that had the button pressed.</param>
		/// <param name="e">Any <see cref="EventArgs" /> that are associated with this button press event.</param>
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
		#endregion
	}
}