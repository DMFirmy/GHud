using System.Drawing;
using System.Drawing.Text;

namespace GHud
{
	internal class DeviceQvga : Device
	{
		#region Constructors
		public DeviceQvga()
		{
			_valid = false;
			_width = 320;
			_height = 240;
			_isColor = true;
			_fontPt = 14.0F;
			_renderHint = TextRenderingHint.AntiAliasGridFit;
			_clearColor = Color.Black;
			_defaultTxtBrush = Brushes.White;
			_clearBrush = Brushes.Black;
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