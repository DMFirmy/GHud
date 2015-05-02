using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;

namespace GHud
{
	// Device base class
	public abstract class Device : IDisposable
	{
		#region Delegates
		public delegate void ButtonHandler(object sender, EventArgs e);
		#endregion

		#region Constructors
		protected Device()
		{
			_valid = false;
			_width = 0;
			_height = 0;
			_isColor = false;
			_fontPt = 0.0F;
			_renderHint = TextRenderingHint.SingleBitPerPixelGridFit;
			_clearColor = Color.White;
			_clearBrush = Brushes.White;
			_invertedClearBrush = Brushes.Black;
			_defaultTxtBrush = Brushes.Black;
			_invertedTxtBrush = Brushes.White;
			_defaultPen = Pens.Black;
			_useBackdrops = false;
		}
		#endregion

		#region Fields
		protected bool _valid;
		// Device physical parameters
		protected int _width;
		protected int _height;
		protected bool _isColor;
		protected bool _useBackdrops;
		// Rendering parameters
		protected float _fontPt;
		protected TextRenderingHint _renderHint;
		protected Color _clearColor;
		protected Brush _clearBrush;
		protected Brush _invertedClearBrush;
		protected Brush _defaultTxtBrush;
		protected Brush _invertedTxtBrush;
		protected Pen _defaultPen;
		// Device LCD connection info
		private int _connection = NativeMethods.LGLCD_INVALID_CONNECTION;
		private int _device = NativeMethods.LGLCD_INVALID_DEVICE;
		protected int _deviceType = NativeMethods.LGLCD_INVALID_DEVICE;
		private uint _lastButtons;
		protected int _curFont = 1;
		protected int _numFonts = 7;
		private Bitmap _lcd; // Main rendering surface
		protected Graphics _graphics;
		private Font _sysFont;
		private readonly List<DisplayModule> _modules = new List<DisplayModule>();
		private bool _disposed;

		protected string[] _fontNames =
		{
			"Inconsolata Medium", "Arial", "Arial Narrow", "Consolas", "Terminal", "Segoe UI Light", "Segoe UI"
		};
		#endregion

		#region Properties
		public int Width
		{
			get { return _width; }
			set { _width = value; }
		}

		public int Height
		{
			get { return _height; }
			set { _height = value; }
		}

		public int CurrentFontIndex
		{
			get { return _curFont; }
		}

		public string[] FontNames
		{
			get { return _fontNames.ToArray(); }
		}

		public float FontSize
		{
			get { return _fontPt; }
		}

		public bool IsColor
		{
			get { return _isColor; }
			set { _isColor = value; }
		}

		public Graphics Graphics
		{
			get { return _graphics; }
		}

		public List<DisplayModule> Modules
		{
			get { return _modules; }
		}

		public Brush ClearBrush
		{
			get { return _clearBrush; }
		}

		public Brush InvertedClearBrush
		{
			get { return _invertedClearBrush; }
		}

		public Brush DefaultTxtBrush
		{
			get { return _defaultTxtBrush; }
		}

		public Brush InvertedTxtBrush
		{
			get { return _invertedTxtBrush; }
		}

		// Backdrops are an alpha blended background
		public bool UseBackdrops
		{
			get { return _useBackdrops; }
			set { _useBackdrops = value; }
		}
		#endregion Properties

		#region Events
		public event ButtonHandler ButtonUp;
		public event ButtonHandler ButtonDown;
		public event ButtonHandler ButtonLeft;
		public event ButtonHandler ButtonRight;
		public event ButtonHandler ButtonOk;
		public event ButtonHandler ButtonCancel;
		public event ButtonHandler ButtonMenu;
		#endregion

		#region Methods
		public bool IsValid()
		{
			return _device != NativeMethods.LGLCD_INVALID_DEVICE;
		}

		public void Dispose()
		{
			if (_disposed)
			{
				return;
			}
			_disposed = true;

			if (_sysFont != null)
			{
				_sysFont.Dispose();
			}

			_lcd.Dispose();

			if (_device != NativeMethods.LGLCD_INVALID_DEVICE)
			{
				NativeMethods.LcdClose(_device);
				_device = NativeMethods.LGLCD_INVALID_DEVICE;
			}

			if (_connection != NativeMethods.LGLCD_INVALID_CONNECTION)
			{
				NativeMethods.LcdDisconnect(_connection);
				_connection = NativeMethods.LGLCD_INVALID_CONNECTION;
			}
			GC.SuppressFinalize(this);
		}

		public void RenderSysString(string msg, bool invert, Rectangle rect)
		{
			if (rect.Width == 0 && rect.Height == 0)
			{
				rect.Width = _width;
				rect.Height = _height;
			}

			if (_sysFont == null)
			{
				_sysFont = new Font(_fontNames[_curFont], _fontPt, FontStyle.Bold);
			}
			var strBounds = _graphics.MeasureString(msg, _sysFont, new Point(0, 0), StringFormat.GenericDefault);

			// ReSharper disable once PossibleLossOfFraction
			var x = (int)((rect.Width / 2) - (strBounds.Width / 2) + rect.X);
			// ReSharper disable once PossibleLossOfFraction
			var y = (int)((rect.Height / 2) - (strBounds.Height / 2) + rect.Y);

			if (invert)
			{
				var invertRect = new Rectangle(x, y, (int)(strBounds.Width + 4), (int)(strBounds.Height + 4));
				_graphics.FillRectangle(_invertedClearBrush, invertRect);
			}

			_graphics.DrawString(msg, _sysFont, invert ? _invertedTxtBrush : _defaultTxtBrush, x, y);
		}

		// Poorly named.  This simply clears the bitmap that will eventually be written to the LCD
		public void ClearLcd(string msg)
		{
			if (_graphics == null)
			{
				return;
			}

			_graphics.Clear(_clearColor);

			if (!string.IsNullOrEmpty(msg))
			{
				RenderSysString(msg, false, new Rectangle(0, 0, 0, 0));
			}
		}

		public void DisplayFrame()
		{
			if (!_valid)
			{
				return;
			}
			var bmp = _lcd.GetHbitmap();
			NativeMethods.LcdUpdateBitmap(_device, bmp, _deviceType);
		}

		public Rectangle GetRect()
		{
			return new Rectangle(0, 0, _width, _height);
		}

		protected void InitLcd()
		{
			_connection = NativeMethods.LcdConnectEx("GHud", 0, 0);
			if (NativeMethods.LGLCD_INVALID_CONNECTION == _connection)
			{
				return;
			}
			_device = NativeMethods.LcdOpenByType(_connection, _deviceType);
			if (NativeMethods.LGLCD_INVALID_DEVICE == _device)
			{
				return;
			}

			_lcd = new Bitmap(_width, _height);

			_graphics = Graphics.FromImage(_lcd);

			_graphics.TextRenderingHint = _renderHint;

			ClearLcd("Waiting for Flight...");
			NativeMethods.LcdSetAsLCDForegroundApp(_device, NativeMethods.LGLCD_FORE_YES);
			_valid = true;
		}

		public void DoButtons()
		{
			var buttons = NativeMethods.LcdReadSoftButtons(_device);

			if (buttons == _lastButtons)
			{
				return;
			}
			if ((buttons & (NativeMethods.LGLCD_BUTTON_1 | NativeMethods.LGLCD_BUTTON_LEFT)) != 0)
			{
				if (ButtonLeft != null)
				{
					ButtonLeft(this, EventArgs.Empty);
				}
			}
			if ((buttons & (NativeMethods.LGLCD_BUTTON_2 | NativeMethods.LGLCD_BUTTON_RIGHT)) != 0)
			{
				if (ButtonRight != null)
				{
					ButtonRight(this, EventArgs.Empty);
				}
			}
			if ((buttons & (NativeMethods.LGLCD_BUTTON_3 | NativeMethods.LGLCD_BUTTON_OK)) != 0)
			{
				if (ButtonOk != null)
				{
					ButtonOk(this, EventArgs.Empty);
				}
			}
			if ((buttons & (NativeMethods.LGLCD_BUTTON_4 | NativeMethods.LGLCD_BUTTON_MENU)) != 0)
			{
				if (ButtonMenu != null)
				{
					ButtonMenu(this, EventArgs.Empty);
				}
			}
			if ((buttons & NativeMethods.LGLCD_BUTTON_UP) != 0)
			{
				if (ButtonUp != null)
				{
					ButtonUp(this, EventArgs.Empty);
				}
			}
			if ((buttons & NativeMethods.LGLCD_BUTTON_DOWN) != 0)
			{
				if (ButtonDown != null)
				{
					ButtonDown(this, EventArgs.Empty);
				}
			}
			if ((buttons & NativeMethods.LGLCD_BUTTON_CANCEL) != 0)
			{
				if (ButtonCancel != null)
				{
					ButtonCancel(this, EventArgs.Empty);
				}
			}

			_lastButtons = buttons;
		}
		#endregion
	}
}