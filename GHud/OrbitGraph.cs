using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace GHud
{
	// Display module which displays a rendering of the current orbit and its parent body.
	internal class OrbitGraph : DisplayModule
	{
		#region Constructors
		public OrbitGraph(Device dev, Color orbPenColor, string monicer)
			: base(dev)
		{
			_name = "Orbit Graph";
			_selectable = false;
			_isActive = false;

			_monicer = monicer;

			_width = dev.Width;
			_height = dev.Height;

			_a = 0;

			// Define the rendering color of the various orbital bodies.
			_bodyColors = new Dictionary<string, Color>
			{
				{"Sun", Color.Yellow},
				{"Kerbol", Color.Yellow},
				{"Moho", Color.Peru},
				{"Eve", Color.BlueViolet},
				{"Kerbin", Color.DodgerBlue},
				{"Duna", Color.Firebrick},
				{"Dres", Color.Gray},
				{"Jool", Color.LimeGreen},
				{"Eeloo", Color.LightGray},
				{"Gilly", Color.DarkGray},
				{"Mun", Color.DarkGray},
				{"Minmus", Color.Gray},
				{"Ike", Color.DarkGray},
				{"Laythe", Color.DodgerBlue},
				{"Vall", Color.Gray},
				{"Tylo", Color.LightGray},
				{"Bop", Color.Gray},
				{"Pol", Color.SandyBrown},
				{"Default", Color.DodgerBlue}
			};

			_bodyColors.TryGetValue("Pol", out _bodyColor);

			_orbitPen = new Pen(orbPenColor, 1.0f)
			{
				Alignment = PenAlignment.Outset
			};
		}
		#endregion

		#region Methods
		// Gather and prepare data needed for rendering this frame.
#if DEBUG
		private void PrepData()
		{
#else
		private void PrepData(Orbit orbit)
		{
			// Get the color of the body being orbited
			if (!_bodyColors.TryGetValue(orbit.referenceBody.GetName(), out _bodyColor))
			{
				_bodyColors.TryGetValue("Default", out _bodyColor);
			}
#endif

#if DEBUG
			_r = 100000.0;
			_a += 0.6;
			if (_a > 360.0)
			{
				_a -= 360.0;
			}

			_dia = 80000.00;
			_atmosDia = 120000.00;
			_apR = 100000.00;
			_peR = 100000.00;

			_apA = _apR + (_dia / 2);
			_peA = _peR + (_dia / 2);
			_semiMajorAxis = (_apR + _peR) / 2;
			_eccentricity = (_apR - _peR) / (_apR + _peR);
			_semiMinorAxis = _semiMajorAxis * (1 - _eccentricity);
#else
	// Orbit info
			_apR = orbit.ApR;
			_peR = orbit.PeR;
			_apA = orbit.ApA;
			_peA = orbit.PeA;

			// Calc the diameter of the reference body and the diameter of its atmosphere
			_dia = orbit.referenceBody.Radius*2;
			_atmosDia = _dia + (orbit.referenceBody.atmosphereDepth*2);

			_semiMajorAxis = orbit.semiMajorAxis;
			_semiMinorAxis = orbit.semiMinorAxis;
			_eccentricity = orbit.eccentricity;

			_r = orbit.RadiusAtTrueAnomaly(orbit.trueAnomaly);
			_a = orbit.trueAnomaly; //(360.0 - orbit.trueAnomaly) + 90.0;
#endif
		}

		private void DoRender(Rectangle rect)
		{
			var tmpfontPt = _dev.IsColor ? _fontPt - 2.0f : _fontPt;
			var line = _dev.IsColor ? 1 : 0;

			RenderMonicer(line);

			if (_companionMod == null || !_companionMod.IsActive)
			{
				RenderOrbitApoapsis(line, tmpfontPt);
				RenderOrbitPeriapsis(line, tmpfontPt);
			}

			if (IsLeavingSoi())
			{
				ModuleMsg("Leaving Sphere of Influence", rect);
			}

			var orbitGeometry = CalculateOrbitGeometry();

			RenderOrbit(orbitGeometry);

			var bodyGeometry = CalculateBodyGeometry(orbitGeometry);

			RenderBody(bodyGeometry);

			var vesselGeometry = CalculateVesselGeometry(orbitGeometry, bodyGeometry);

			RenderVessel(vesselGeometry);

			// Calculate and draw the atmosphere.  This is drawn over the body with and alpha level.
			// By drawing this last, we get the effect of seeing the orbit and vessel behind the atmostphere.
			if (!_dev.IsColor)
			{
				return;
			}
			var atmosphereGeometry = CalculateAtmosphereGeometry(orbitGeometry, bodyGeometry);

			RenderAtmosphere(atmosphereGeometry);
		}

		private void RenderAtmosphere(AtmosphereGeometry atmosphereGeometry)
		{
			var atmosRect = new Rectangle((int)atmosphereGeometry.X, (int)atmosphereGeometry.CenterY, (int)atmosphereGeometry.Dia, (int)atmosphereGeometry.Dia);
			var atmosInnerRect = new Rectangle((int)(atmosphereGeometry.X + atmosphereGeometry.PosReduction), (int)(atmosphereGeometry.CenterY + atmosphereGeometry.PosReduction),
				(int)(atmosphereGeometry.Dia - (atmosphereGeometry.Reduction)), (int)(atmosphereGeometry.Dia - (atmosphereGeometry.Reduction)));

			using (var atmosBrush = new SolidBrush(Color.FromArgb(100, _bodyColor.R, _bodyColor.G, _bodyColor.B)))
			{
				_dev.Graphics.FillEllipse(atmosBrush, atmosInnerRect);
				_dev.Graphics.FillEllipse(atmosBrush, atmosRect);
			}
		}

		private void RenderVessel(VesselGeometry vesselGeometry)
		{
			var vesselRect = new Rectangle((int)(vesselGeometry.XX - vesselGeometry.Radius), (int)(vesselGeometry.YY - vesselGeometry.Radius), (int)vesselGeometry.Radius * 2, (int)vesselGeometry.Radius * 2);
			_dev.Graphics.FillEllipse(_dev.InvertedClearBrush, vesselRect);
		}

		private void RenderBody(BodyGeometry bodyGeometry)
		{
// Draw the body
			var bodyRect = new Rectangle((int)bodyGeometry.X, (int)bodyGeometry.CenterY, (int)bodyGeometry.Dia, (int)bodyGeometry.Dia);
			if (_dev.IsColor)
			{
				Brush bodyBrush = new SolidBrush(_bodyColor);
				_dev.Graphics.FillEllipse(bodyBrush, bodyRect);
				bodyBrush.Dispose();
			}
			else
			{
				_dev.Graphics.FillEllipse(_dev.InvertedClearBrush, bodyRect);
			}
		}

		private void RenderOrbit(OrbitGeometry orbitGeometry)
		{
// Draw the ellipse
			_ellipticalRect = new Rectangle(orbitGeometry.X, orbitGeometry.Y, (int)(orbitGeometry.Width), (int)(orbitGeometry.Height));
			if (_ellipticalRect.Height < 0)
			{
				_ellipticalRect.Height = 1;
			}
			_dev.Graphics.DrawEllipse(_orbitPen, _ellipticalRect);
		}

		private void RenderMonicer(int line)
		{
			if (_dev.IsColor)
			{
				line++;
				RenderString(_monicer, line + 1, 0, ref _twoColumnOffsets, _fmtLeft, FontStyle.Bold, false, _fontPt + 1.0f);
			}
			else
			{
				RenderString(_monicer, line + 1, 0, ref _twoColumnOffsets, _fmtLeft, FontStyle.Bold, false, _fontPt + 2.0f);
			}
		}

		private void RenderOrbitApoapsis(int line, float fontSize)
		{
			var orbApSuffix = string.Empty;
			RenderString("a:" + Util.xMuMech_ToSI(_apA, ref orbApSuffix) + orbApSuffix, line, 0, ref _twoColumnOffsets, _fmtLeft,
				FontStyle.Regular, false, fontSize);
		}

		private void RenderOrbitPeriapsis(int line, float fontSize)
		{
			var orbPeSuffix = string.Empty;
			RenderString("p:" + Util.xMuMech_ToSI(_peA, ref orbPeSuffix) + orbPeSuffix, line, 1, ref _twoColumnOffsets,
					_fmtRight, FontStyle.Regular, false, fontSize);
		}

		private bool IsLeavingSoi()
		{
			// We are leaving the sphere of influence,  no orbit can be drawn.
			if (_apR < 0 && _apR < _peR)
			{
				return true;
			}
			return false;
		}

		private OrbitGeometry CalculateOrbitGeometry()
		{
			const int border = 3;
			var geometry = new OrbitGeometry();
			// The bounding box for the drawing
			var dr = new Rectangle(_xOff + border, _yOff + border, _width - (border * 2), _height - (border * 2));

			// Calculate scaling factor used to scale from KSP orbital sizes down to device pixels
			var widthMod = dr.Width / (_semiMajorAxis * 2);
			var heightMod = dr.Height / (_semiMinorAxis * 2);

			var atmosMod = Math.Min(dr.Width, dr.Height) / _atmosDia;

			// Use the smallest of the scaling factors
			geometry.ScaleFactor = Math.Min(atmosMod, Math.Min(widthMod, heightMod));

			// Ellipse geometry scaled to display area
			geometry.Width = (_semiMajorAxis * 2) * geometry.ScaleFactor;
			geometry.Height = (_semiMinorAxis * 2) * geometry.ScaleFactor;
			geometry.X = (int)(dr.X + ((dr.Width - geometry.Width) / 2));
			geometry.Y = (int)(dr.Y + ((dr.Height - geometry.Height) / 2));

			return geometry;
		}

		private BodyGeometry CalculateBodyGeometry(OrbitGeometry orbitGeometry)
		{
			var geometry = new BodyGeometry();

			// Scale and draw the body
			geometry.Dia = _dia * orbitGeometry.ScaleFactor;

			// Determine the scaled location where the body should be centered
			geometry.CenterX = (orbitGeometry.X + (orbitGeometry.Width / 2)) - (geometry.Dia / 2);
			geometry.CenterY = (orbitGeometry.Y + (orbitGeometry.Height / 2)) - (geometry.Dia / 2);
			geometry.XOffset = (_semiMajorAxis - _peR) * orbitGeometry.ScaleFactor;
			geometry.X = geometry.CenterX + geometry.XOffset;
			
			return geometry;
		}
		
		private VesselGeometry CalculateVesselGeometry(OrbitGeometry orbitGeometry, BodyGeometry bodyGeometry)
		{
			// Calculate the drawing location of the vessel on the ellipse.
			var geometry = new VesselGeometry();

			geometry.ARadian = (_a + 90) * (Math.PI / 180);
			geometry.X = (_r * Math.Sin(geometry.ARadian)) * orbitGeometry.ScaleFactor;
			geometry.Y = (_r * Math.Cos(geometry.ARadian)) * orbitGeometry.ScaleFactor;
			geometry.XX = (int)(geometry.X + bodyGeometry.X + (bodyGeometry.Dia / 2));
			geometry.YY = (int)(geometry.Y + bodyGeometry.CenterY + (bodyGeometry.Dia / 2));

			geometry.Radius = (int)(_dev.FontSize / 3);

			return geometry;
		}

		private AtmosphereGeometry CalculateAtmosphereGeometry(OrbitGeometry orbitGeometry, BodyGeometry bodyGeometry)
		{
			var geometry = new AtmosphereGeometry();

			geometry.Dia = _atmosDia * orbitGeometry.ScaleFactor;
			geometry.CenterX = (orbitGeometry.X + (orbitGeometry.Width / 2)) - (geometry.Dia / 2);
			geometry.CenterY = (orbitGeometry.Y + (orbitGeometry.Height / 2)) - (geometry.Dia / 2);
			geometry.X = geometry.CenterX + bodyGeometry.XOffset;

			geometry.Reduction = (geometry.Dia - bodyGeometry.Dia) / 2;
			geometry.PosReduction = geometry.Reduction / 2;

			return geometry;
		}
		
		public override void Render(Rectangle rect)
		{
			if (!IsActive)
			{
				return;
			}

			// Render in the specified rectangle.  If the rect is 0, then use the whole device geometry
			CalculateRenderDimensions(rect);
#if !DEBUG
			if (_orbit == null)
			{
				ModuleMsg(IsTargetTypeModule ? "No Target" : "Null Orbit", rect);
				return;
			}
#endif
#if DEBUG
			PrepData();
#else
			PrepData(_orbit);
#endif
			DoRender(rect);
		}
		#endregion

		#region Fields
		private readonly Pen _orbitPen;
		private double _apR;
		private double _peR;
		private double _apA;
		private double _peA;
		private double _semiMajorAxis;
		private double _semiMinorAxis;
		private double _eccentricity;
		private double _atmosDia;
		private double _dia;
		private Rectangle _ellipticalRect;
		private Color _bodyColor;
		private double _a;
		private double _r;
		private readonly Dictionary<string, Color> _bodyColors;
		private readonly String _monicer;
		#endregion
	}
}