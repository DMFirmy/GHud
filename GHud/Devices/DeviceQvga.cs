using System.Drawing;
using System.Drawing.Text;

namespace GHud.Devices
{
	internal class DeviceQvga : Device
	{
		#region Constructors
		public DeviceQvga()
		{
			_isInitialized = false;
			_width = 320;
			_height = 240;
			_renderArea = new Rectangle(0, 0, _width, _height);
			_isColor = true;
			_fontPt = 14.0F;
			_renderHint = TextRenderingHint.AntiAliasGridFit;
			_clearColor = Color.Black;
			_clearBrush = Brushes.Black;
			_defaultTxtBrush = Brushes.White;
			_invertedClearBrush = Brushes.White;
			_invertedTxtBrush = Brushes.Black;
			_defaultPen = Pens.White;
			_useBackdrops = false;

			_deviceType = NativeMethods.LGLCD_DEVICE_QVGA;
			InitLcd();
		}
		#endregion
	}
}