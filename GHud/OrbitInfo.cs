using System;
using System.Drawing;

namespace GHud
{
	internal class OrbitInfo : DisplayModule
	{
		#region Constructors
		public OrbitInfo(Device dev, string argmon, Color brectC1, Color brectC2)
			: base(dev)
		{
			_name = "Orbit";
			_selectable = false;
			_isActive = false;
			_monicer = argmon;

			_width = dev.Width;
			_height = dev.Height;

			_backRectC1 = brectC1;
			_backRectC2 = brectC2;

			ResetBackRectBrush();
		}
		#endregion

		#region Fields
		private readonly string _monicer;
		private string _situationStr;
		private string _orbitVelocityStr;
		private string _orbitApoapsisStr;
		private string _orbitPeriapsisStr;
		private string _orbitApoapsisTimeStr;
		private string _orbitPeriapsisTimeStr;
		private string _orbApSuffix;
		private string _orbPeSuffix;
		private string _orbitInclinationStr;
		private string _orbBodyName;
		protected int _count;
		#endregion

		#region Methods
		private void PrepData()
		{
#if DEBUG
			_orbitVelocityStr = "174.55m/s";
			_orbitApoapsisStr = "70.48m";
			_orbitPeriapsisStr = "-598.44km";
			_orbitInclinationStr = "0.103°";
			_orbitApoapsisTimeStr = "4d 00:00:00";
			_orbitPeriapsisTimeStr = "00:00:00";
			_orbBodyName = "Kerbin";
			_orbitObjectName = "Kerbal X";
			_situationStr = "Sub Orbital";
#else
			var vessel = FlightGlobals.ActiveVessel;
			switch (vessel.situation)
			{
				case Vessel.Situations.DOCKED:
					_situationStr = "Docked";
					break;
				case Vessel.Situations.ESCAPING:
					_situationStr = "Escaping";
					break;
				case Vessel.Situations.FLYING:
					_situationStr = "Flying";
					break;
				case Vessel.Situations.LANDED:
					_situationStr = "Landed";
					break;
				case Vessel.Situations.ORBITING:
					_situationStr = "";
					break;
				case Vessel.Situations.PRELAUNCH:
					_situationStr = "Prelaunch";
					break;
				case Vessel.Situations.SPLASHED:
					_situationStr = "Splashed";
					break;
				case Vessel.Situations.SUB_ORBITAL:
					_situationStr = "Sub Orbital";
					break;
				default:
					_situationStr = "";
					break;
			}

			_orbitVelocityStr = _orbit.vel.magnitude.ToString("F2");
			_orbitApoapsisStr = Util.xMuMech_ToSI(_orbit.ApA, ref _orbApSuffix);
			_orbitApoapsisStr += _orbApSuffix + "m";
			_orbitPeriapsisStr = Util.xMuMech_ToSI(_orbit.PeA, ref _orbPeSuffix);
			_orbitPeriapsisStr += _orbPeSuffix + "m";
			_orbitInclinationStr = _orbit.inclination.ToString("F3") + "°";
			_orbitApoapsisTimeStr = Util.ConvertInterval(_orbit.timeToAp, false);
			_orbitPeriapsisTimeStr = Util.ConvertInterval(_orbit.timeToPe, false);
			_orbBodyName = _orbit.referenceBody.GetName();
#endif
		}

		private void DoRender(Rectangle rect)
		{
			CalculateRenderDimensions(rect);

			_fontPt = Math.Min(Math.Max((float)((_height / 4f) * 0.7), 7), 14);

			RenderVesselName(0);
			RenderOrbitBodyName(0);

			RenderVesselOrbitVelocity(1);
			RenderVesselOrbitInclination(1);

			RenderVesselOrbitApoapsis(2);
			RenderVesselOrbitPeriapsis(2);

			//∆v

			RenderVesselOrbitApoapsisTime(3);
			RenderVesselOrbitPeriapsisTime(3);

			RenderSituationString(4);

			RenderBorderLines(1, 4);
		}

		private void RenderVesselName(int line)
		{
			RenderString(_monicer, line, 0, ref _twoColumnLabeledOffsets, _fmtLeft, FontStyle.Bold, true);
			RenderString(_orbitObjectName, line, 1, ref _twoColumnLabeledOffsets, _fmtRight, FontStyle.Regular, true);
		}

		private void RenderOrbitBodyName(int line)
		{
			const string orbitBodyMonicer = "⊙";
			RenderString(orbitBodyMonicer, line, 2, ref _twoColumnLabeledOffsets, _fmtLeft, FontStyle.Bold, true);
			RenderString(_orbBodyName, line, 3, ref _twoColumnLabeledOffsets, _fmtRight, FontStyle.Regular, true);
		}

		private void RenderVesselOrbitVelocity(int line)
		{
			const string orbitVelocityMonicer = "⇢";
			RenderString(orbitVelocityMonicer, line, 0, ref _twoColumnLabeledOffsets, _fmtLeft);
			RenderString(_orbitVelocityStr, line, 1, ref _twoColumnLabeledOffsets, _fmtRight);
		}

		private void RenderVesselOrbitInclination(int line)
		{
			const string orbitInclinationMonicer = "θ";
			RenderString(orbitInclinationMonicer, line, 2, ref _twoColumnLabeledOffsets, _fmtLeft);
			RenderString(_orbitInclinationStr, line, 3, ref _twoColumnLabeledOffsets, _fmtRight);
		}

		private void RenderVesselOrbitApoapsis(int line)
		{
			const string orbitApoapsisMonicer = "a";
			RenderString(orbitApoapsisMonicer, line, 0, ref _twoColumnOffsets, _fmtLeft);
			RenderString(_orbitApoapsisStr, line, 0, ref _twoColumnOffsets, _fmtRight);
		}

		private void RenderVesselOrbitPeriapsis(int line)
		{
			const string orbitPeriapsisMonicer = "p";
			RenderString(orbitPeriapsisMonicer, line, 1, ref _twoColumnOffsets, _fmtLeft);
			RenderString(_orbitPeriapsisStr, line, 1, ref _twoColumnOffsets, _fmtRight);
		}

		private void RenderBorderLines(int topLine, int bottomLine)
		{
			_dev.Graphics.DrawLine(_dev.IsColor ? Pens.Green : Pens.Black, 0 + _xOff, topLine * _lineOffset + _yOff, _width + _xOff,
				topLine * _lineOffset + _yOff);
			_dev.Graphics.DrawLine(_dev.IsColor ? Pens.Green : Pens.Black, (_width / 2) + _xOff, (topLine * _lineOffset) + _yOff,
				(_width / 2) + _xOff, _height + _yOff);

			_dev.Graphics.DrawLine(_dev.IsColor ? Pens.Green : Pens.Black, 0 + _xOff, bottomLine * _lineOffset + _yOff,
				_width + _xOff, bottomLine * _lineOffset + _yOff);
		}

		private void RenderVesselOrbitApoapsisTime(int line)
		{
			RenderString(_orbitApoapsisTimeStr, line, 0, ref _twoColumnOffsets, _fmtRight);
		}

		private void RenderVesselOrbitPeriapsisTime(int line)
		{
			RenderString(_orbitPeriapsisTimeStr, line, 1, ref _twoColumnOffsets, _fmtRight);
		}

		private void RenderSituationString(int line)
		{
			if (_dev.IsColor)
			{
				RenderString(_situationStr, line, 0, ref _twoColumnOffsets, _fmtLeft);
			}
		}

		public override void Render(Rectangle rect)
		{
			if (!IsActive)
			{
				return;
			}

#if !DEBUG
			if (_orbit == null)
			{
				ModuleMsg(IsTargetTypeModule ? "No Target" : "Null Orbit", rect);
				return;
			}
#endif

			PrepData();
			DoRender(rect);
		}
		#endregion
	}
}