using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using GHud.Devices;

namespace GHud.Modules
{
	public abstract class DisplayModule : IDisposable
	{
		#region Constructors
		protected DisplayModule(Device argdev)
		{
			_dev = argdev;
			_name = "INVALID";
			_selectable = false;
			_isActive = false;
			_curfont = null;

			_moduleId = 0;

			_fontPt = _dev.FontSize;

			_companionMod = null;

			_fmtLeft.Alignment = StringAlignment.Near;
			_fmtLeft.LineAlignment = StringAlignment.Center;
			_fmtCenter.Alignment = StringAlignment.Center;
			_fmtCenter.LineAlignment = StringAlignment.Center;
			_fmtRight.Alignment = StringAlignment.Far;
			_fmtRight.LineAlignment = StringAlignment.Center;
		}
		#endregion

		#region Fields
		protected string _name;
		protected int _width = 1;
		protected int _height = 1;
		protected int _xOff;
		protected int _yOff;
		protected float _fontPt;
		protected bool _selectable;
		protected bool _isActive;
		private Font _curfont;
		protected float _lineOffset;
		private int _maxSuffixWidth;
		private int _maxLabWidth;
		protected const string Mps2Suf = "m²";
		private const string BigSuf = "mT";
		private const string BigLab = "WW:";
		protected StringFormat _fmtCenter = new StringFormat();
		protected StringFormat _fmtRight = new StringFormat();
		protected StringFormat _fmtLeft = new StringFormat();
		private StringFormat _fmtDefault = StringFormat.GenericDefault;
		protected int[] _twoColumnLabeledOffsets;
		protected int[] _twoColumnOffsets;
		protected Device _dev;
		protected Color _backRectC1 = Color.Black;
		protected Color _backRectC2 = Color.Black;
		private LinearGradientBrush _backRectBrush;
		protected Orbit _orbit;
		protected string _orbitObjectName;
		protected int _moduleId;
		protected DisplayModule _companionMod;
		private bool _disposed;
		#endregion

		#region Properties
		public bool IsTargetTypeModule { get; set; }

		public bool IsActive
		{
			get { return _isActive; }
		}

		public int ModuleId
		{
			get { return _moduleId; }
			set { _moduleId = value; }
		}

		public DisplayModule CompanionMod
		{
			get { return _companionMod; }
			set { _companionMod = value; }
		}
		#endregion

		#region Public Methods
		public void Dispose()
		{
			if (_disposed)
			{
				return;
			}
			_disposed = true;

			if (_fmtCenter != null)
			{
				_fmtCenter.Dispose();
				_fmtCenter = null;
			}
			if (_fmtRight != null)
			{
				_fmtRight.Dispose();
				_fmtRight = null;
			}
			if (_fmtLeft != null)
			{
				_fmtLeft.Dispose();
				_fmtLeft = null;
			}
			if (_fmtDefault != null)
			{
				_fmtDefault.Dispose();
				_fmtDefault = null;
			}
			// ReSharper disable once InvertIf
			if (_backRectBrush != null)
			{
				_backRectBrush.Dispose();
				_backRectBrush = null;
			}
		}

		// ReSharper disable once VirtualMemberNeverOverriden.Global
		public void SetOrbit(Orbit argorbit, string objName = "Unknown")
		{
			_orbit = argorbit;
			_orbitObjectName = objName;
		}

		public void ModuleMsg(string msg, Rectangle rect, bool invert = false)
		{
			if (!IsActive)
			{
				return;
			}

			if (!string.IsNullOrEmpty(msg))
			{
				_dev.RenderSysString(msg, invert, rect);
			}
		}

		public void Activate()
		{
			_isActive = true;
		}

		public void Deactivate()
		{
			_isActive = false;
		}

		public virtual void Render(Rectangle rect)
		{
		}

		public void DisplayModuleName()
		{
			_dev.RenderSysString(_name, true, new Rectangle(0, 0, 0, 0));
		}
		#endregion

		#region Protected Methods
		protected void ResetBackRectBrush()
		{
			if (_backRectBrush != null)
			{
				_backRectBrush.Dispose();
			}
			_backRectBrush = new LinearGradientBrush(new PointF(0.5F, 0.0F), new PointF(0.5F, 1.0F), _backRectC1, _backRectC2);
		}

		protected virtual void Clear()
		{
			if (!IsActive)
			{
				return;
			}
			_dev.Graphics.FillRectangle(_dev.ClearBrush, new Rectangle(_xOff, _yOff, _width, _height));
		}

		protected void RenderString(string str, int line, int column, ref int[] cOffsets, StringFormat fmt,
			FontStyle style = FontStyle.Regular, bool backRect = false, float curFontPt = 0.0f, float xoffset = 0.0f,
			float yoffset = 0.0f)
		{
			if (!IsActive)
			{
				return;
			}

			// ReSharper disable once CompareOfFloatsByEqualityOperator
			var cfont = new Font(_dev.FontNames[_dev.CurrentFontIndex], curFontPt == 0 ? _fontPt : curFontPt, style);
			CalcColumns(cfont);

			xoffset += _xOff;
			yoffset += _yOff;
			var rect1 = new Rectangle((int)(cOffsets[column] + xoffset),
				(int)(Math.Floor(_lineOffset * line) + yoffset),
				cOffsets[column + 1] - cOffsets[column], //+xoffset 
				(int)(Math.Ceiling(_lineOffset))); // +yoffset

			if (backRect)
			{
				FillRectGradient(rect1, 0.5f, _backRectC1, _backRectC2, 0.5f, _backRectC2, _backRectC1);
				_dev.Graphics.DrawString(str, cfont, _dev.InvertedTxtBrush, rect1, fmt);
			}
			else
			{
				_dev.Graphics.DrawString(str, cfont, _dev.DefaultTxtBrush, rect1, fmt);
			}

			cfont.Dispose();
		}

		/// <summary>
		///     Calculates the dimensions of the rendering area based on the dimensions of the supplied <see cref="Rectangle" />.
		///     If the supplied <see cref="Rectangle" /> has a <see cref="Rectangle.Width" /> & <see cref="Rectangle.Height" /> are
		///     both set to 0, then the dimensions will be set to the size of the entire device display, otherwise they will be set
		///     to the values of the suppled <see cref="Rectangle" />.
		/// </summary>
		/// <param name="rect">
		///     A<see cref="Rectangle" /> that defines the area to be rendered to, or else a rectangle with a
		///     <see cref="Rectangle.Width" /> & <see cref="Rectangle.Height" /> both set to 0 to set the dimensions to the full
		///     size of the device dislay.
		/// </param>
		protected void CalculateRenderDimensions(Rectangle rect)
		{
			if (rect.Width != 0 && rect.Height != 0)
			{
				_width = rect.Width;
				_height = rect.Height;
				_xOff = rect.X;
				_yOff = rect.Y;
			}
			else
			{
				_width = _dev.Width;
				_height = _dev.Height;
				_xOff = 0;
				_yOff = 0;
			}
		}
		#endregion

		#region Private Methods
		// Calculates a set of column offset info based on measurements of the font and the display geometry
		private void CalcColumns(Font font)
		{
			if (!IsActive || font == null)
			{
				return;
			}

			var maxSufBounds = _dev.Graphics.MeasureString(BigSuf, font, new Point(0, 0), _fmtDefault);
			_maxSuffixWidth = (int)(maxSufBounds.Width + 1);

			var maxLabBounds = _dev.Graphics.MeasureString(BigLab, font, new Point(0, 0), _fmtDefault);
			_maxLabWidth = (int)(maxLabBounds.Width);

			var smallCol = _maxLabWidth;
			var largeCol = (_width - (smallCol * 2)) / 2;

			_lineOffset = (float)Math.Round(font.GetHeight());

			if (_twoColumnLabeledOffsets == null)
			{
				_twoColumnLabeledOffsets = new int[5];
			}
			_twoColumnLabeledOffsets[0] = _xOff + 0;
			_twoColumnLabeledOffsets[1] = _xOff + smallCol;
			_twoColumnLabeledOffsets[2] = _twoColumnLabeledOffsets[1] + largeCol;
			_twoColumnLabeledOffsets[3] = _twoColumnLabeledOffsets[2] + smallCol;
			_twoColumnLabeledOffsets[4] = _xOff + _width;

			if (_twoColumnOffsets == null)
			{
				_twoColumnOffsets = new int[3];
			}
			_twoColumnOffsets[0] = _xOff + 0;
			_twoColumnOffsets[1] = _xOff + _width / 2;
			_twoColumnOffsets[2] = _xOff + _width;
		}

		private void FillRectGradient(Rectangle rect, float pct1, Color c1, Color c2, float pct2, Color c3, Color c4)
		{
			var rect1 = new Rectangle(rect.X, rect.Y, rect.Width, (int)Math.Ceiling(rect.Height * pct1));
			var rect2 = new Rectangle(rect.X, rect.Y + rect1.Height - 1, rect.Width, (int)Math.Ceiling(rect.Height * pct2) + 1);

			var tmpbrush = new LinearGradientBrush(rect2, c3, c4, LinearGradientMode.Vertical);
			_dev.Graphics.FillRectangle(tmpbrush, rect2);
			tmpbrush.Dispose();

			tmpbrush = new LinearGradientBrush(rect1, c1, c2, LinearGradientMode.Vertical);
			_dev.Graphics.FillRectangle(tmpbrush, rect1);
			tmpbrush.Dispose();
		}
		#endregion
	}
}