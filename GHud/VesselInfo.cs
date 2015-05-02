using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace GHud
{
	// This is a hybrid displaymodule.  It contains both and orbitinfo and orbit graph displaymodule and splits them on the screen.
	internal class VesselInfo : DisplayModule
	{
		#region Constructors
		public VesselInfo(Device dev) : base(dev)
		{
			_name = "Vessel";
			_selectable = true;
			_isActive = false;

			_width = dev.Width;
			_height = dev.Height;
			_xOff = 0;
			_yOff = 0;

			const string vesselMonicer = "✈";
			const string targetMonicer = "+";

			var orbitInfo = new OrbitInfo(dev, vesselMonicer, Color.FromArgb(0xee, 0xee, 0x00), Color.FromArgb(0xaa, 0xaa, 0x44));
			var targetOrbitInfo = new OrbitInfo(dev, targetMonicer, Color.LightBlue, Color.MediumPurple);

			var orbitGraph = new OrbitGraph(dev, Color.Yellow, vesselMonicer);
			var targetOrbitGraph = new OrbitGraph(dev, Color.LightBlue, targetMonicer);

			orbitGraph.CompanionMod = orbitInfo;
			orbitInfo.CompanionMod = orbitGraph;

			targetOrbitInfo.IsTargetTypeModule = true;
			targetOrbitGraph.IsTargetTypeModule = true;
			targetOrbitInfo.CompanionMod = targetOrbitGraph;
			targetOrbitGraph.CompanionMod = targetOrbitInfo;

			orbitInfo.Activate();
			_activeTopMod = orbitInfo;
			orbitGraph.Activate();
			_activeBottomMod = orbitGraph;

			orbitInfo.ModuleId = 1;
			targetOrbitInfo.ModuleId = 2;
			orbitGraph.ModuleId = 3;
			targetOrbitGraph.ModuleId = 4;

			_topModules.Add(orbitInfo);
			_topModules.Add(targetOrbitInfo);
			_bottomModules.Add(orbitGraph);
			_bottomModules.Add(targetOrbitGraph);

			orbitInfo = new OrbitInfo(dev, "✈", Color.FromArgb(0xee, 0xee, 0x00), Color.FromArgb(0xaa, 0xaa, 0x44));
			targetOrbitInfo = new OrbitInfo(dev, "+", Color.LightBlue, Color.MediumPurple);

			orbitGraph = new OrbitGraph(dev, Color.Yellow, "✈");
			targetOrbitGraph = new OrbitGraph(dev, Color.LightBlue, "+");

			orbitGraph.CompanionMod = orbitInfo;
			orbitInfo.CompanionMod = orbitGraph;

			targetOrbitInfo.IsTargetTypeModule = true;
			targetOrbitGraph.IsTargetTypeModule = true;
			targetOrbitInfo.CompanionMod = targetOrbitGraph;
			targetOrbitGraph.CompanionMod = targetOrbitInfo;

			orbitInfo.ModuleId = 1;
			targetOrbitInfo.ModuleId = 2;
			orbitGraph.ModuleId = 3;
			targetOrbitGraph.ModuleId = 4;

			_bottomModules.Add(orbitInfo);
			_bottomModules.Add(targetOrbitInfo);
			_topModules.Add(orbitGraph);
			_topModules.Add(targetOrbitGraph);

			if (dev.UseBackdrops)
			{
				_background = Image.FromFile("stars.gif");
			}

			var colmatrix = new ColorMatrix {Matrix33 = 0.7F};
			_imgAttr = new ImageAttributes();
			_imgAttr.SetColorMatrix(colmatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

			dev.ButtonUp += ButtonUp;
			dev.ButtonDown += ButtonDown;
		}
		#endregion

		#region Fields
		private List<DisplayModule> _topModules = new List<DisplayModule>();
		private List<DisplayModule> _bottomModules = new List<DisplayModule>();
		private DisplayModule _activeTopMod;
		private DisplayModule _activeBottomMod;

		private readonly Image _background;
		private readonly ImageAttributes _imgAttr;
		#endregion

		#region Methods
		protected override void Clear()
		{
			base.Clear();

			if (_dev.UseBackdrops)
			{
				_dev.Graphics.DrawImage(_background, new Rectangle(0, 0, _width, _height), 0, 0, _background.Width,
					_background.Height, GraphicsUnit.Pixel, _imgAttr);
			}
		}

		private static void ActivateModule(DisplayModule newActiveMod, ref DisplayModule changeActiveMod,
			ref DisplayModule otherActiveMod)
		{
			changeActiveMod.Deactivate();
			changeActiveMod = newActiveMod;
			changeActiveMod.Activate();
			otherActiveMod.Activate();
		}

		private static void CycleModules(ref List<DisplayModule> changelist, ref DisplayModule changeActiveMod,
			ref DisplayModule otherActiveMod)
		{
			var activateFirst = false;
			while (true)
			{
				var activateNext = false;
				var activated = false;

				if (activateFirst)
				{
					ActivateModule(changelist[0], ref changeActiveMod, ref otherActiveMod);
					return;
				}

				foreach (var mod in changelist)
				{
					if (activateNext)
					{
						if (mod.ModuleId == otherActiveMod.ModuleId)
						{
							continue;
						}
						ActivateModule(mod, ref changeActiveMod, ref otherActiveMod);

						activateNext = false;
						activated = true;
					}
					else
					{
						if (mod.IsActive)
						{
							activateNext = true;
						}
					}
				}
				if (!activated)
				{
					// Activate the first module since we were at the end of the list
					activateFirst = true;
					continue;
				}
				break;
			}
		}

		private void ButtonUp(object sender, EventArgs e)
		{
			if (!IsActive)
			{
				return;
			}

			CycleModules(ref _topModules, ref _activeTopMod, ref _activeBottomMod);
		}

		private void ButtonDown(object sender, EventArgs e)
		{
			var dev = sender as Device;
			if (!IsActive || dev == null)
			{
				return;
			}

			if (dev.IsColor)
			{
				CycleModules(ref _bottomModules, ref _activeBottomMod, ref _activeTopMod);
			}
		}

		private void RenderModules(IEnumerable<DisplayModule> list, Rectangle rect)
		{
#if !DEBUG
			var vessel = FlightGlobals.ActiveVessel;

			if (vessel == null)
			{
				_dev.ClearLcd("Waiting for flight...");
				return;
			}
			var target = FlightGlobals.fetch.VesselTarget;
#endif
			foreach (var mod in list)
			{
				if (!mod.IsActive)
				{
					continue;
				}

#if !DEBUG
				if (!mod.IsTargetTypeModule)
				{
					mod.SetOrbit(vessel.orbit, vessel.GetName());
					mod.Render(rect);
				}
				else
				{
					if (target != null)
					{
						mod.SetOrbit(target.GetOrbit(), target.GetName());
					}
					else
					{
						mod.SetOrbit(null, null);
					}
#endif
				mod.Render(rect);
#if !DEBUG
				}
#endif
			}
		}

		public override void Render(Rectangle rect)
		{
			if (!IsActive)
			{
				return;
			}

			if (rect.Width == 0 || rect.Height == 0)
			{
				rect = _dev.GetRect();
			}

			Clear();

			Rectangle vrect;
			if (_activeTopMod.GetType() == _activeBottomMod.GetType())
			{
				vrect = new Rectangle(rect.X, rect.Y, rect.Width, rect.Height / 2);
			}
			else if (_activeTopMod is OrbitInfo)
			{
				vrect = new Rectangle(rect.X, rect.Y, rect.Width, (int)(rect.Height * 0.4f));
			}
			else
			{
				vrect = new Rectangle(rect.X, rect.Y, rect.Width, (int)(rect.Height * 0.6f));
			}
			var trect = new Rectangle(rect.X, rect.Y + vrect.Height, rect.Width, rect.Height - vrect.Height);

			RenderModules(_topModules, vrect);
			RenderModules(_bottomModules, trect);
		}
		#endregion
	}
}