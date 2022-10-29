using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.Graphics.Canvas.Effects;
using Windows.UI.Composition;

namespace TextBlockFX.Win2D.UWP
{
    public class TextEffectsHelper
    {
        public static void ShrinkFontAndAttachCustomBrushData(
            CanvasTextLayout textLayout,
            int textPosition,
            int characterCount,
            CustomBrushData.BaselineAdjustmentType baselineAdjustmentType, CanvasTextFormat textFormat)
        {
            float fontSizeShrinkAmount = 0.65f;
            var CurrentFontSize = textFormat.FontSize;
            if (textPosition >= 0)
            {
                textLayout.SetFontSize(textPosition, characterCount, (float)CurrentFontSize * fontSizeShrinkAmount);
                textLayout.SetCustomBrush(textPosition, characterCount, new CustomBrushData() { BaselineAdjustment = baselineAdjustmentType });

            }
        }
      
    }
   

}
