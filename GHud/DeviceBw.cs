using System.Drawing;
using System.Drawing.Text;

namespace GHud
{
	internal class DeviceBw : Device
	{
		#region Constructors
		public DeviceBw()
		{
			_valid = false;
			_width = 160;
			_height = 43;
			_isColor = false;
			_fontPt = 7.0F;
			_renderHint = TextRenderingHint.SingleBitPerPixelGridFit;
			_clearColor = Color.White;
			_defaultTxtBrush = Brushes.Black;
			_clearBrush = Brushes.White;
			_invertedClearBrush = Brushes.Black;
			_invertedTxtBrush = Brushes.White;
			_defaultPen = Pens.Black;
			_useBackdrops = false;

			_deviceType = NativeMethods.LGLCD_DEVICE_BW;
			InitLcd();
		}
		#endregion
	}
}