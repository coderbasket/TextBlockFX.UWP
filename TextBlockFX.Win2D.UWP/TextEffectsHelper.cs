using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

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
    public class CustomBrushData
    {
        public enum BaselineAdjustmentType { None, Raise, Lower }
        public BaselineAdjustmentType BaselineAdjustment;
    }
    class SubscriptSuperscriptRenderer : ICanvasTextRenderer
    {
        public CanvasDrawingSession DrawingSession;
        public CanvasSolidColorBrush TextBrush;
        public float SubscriptBaselineScale { get; set; } = 0.2f;
        public float SuperscriptBaselineScale { get; set; } = 0.7f;
       
        public SubscriptSuperscriptRenderer()
        {

        }

        public void DrawGlyphRun(
            Vector2 position,
            CanvasFontFace fontFace,
            float fontSize,
            CanvasGlyph[] glyphs,
            bool isSideways,
            uint bidiLevel,
            object brush,
            CanvasTextMeasuringMode measuringMode,
            string locale,
            string textString,
            int[] clusterMapIndices,
            uint startingTextPosition,
            CanvasGlyphOrientation glyphOrientation)
        {
            if (glyphs == null || glyphs.Length == 0)
                return;

            float scaledFontAscent = fontFace.Ascent * fontSize;
            float subscriptBaselineDropAmount = scaledFontAscent * SubscriptBaselineScale;
            float superscriptBaselineRaiseAmount = scaledFontAscent * SuperscriptBaselineScale;

            // Draw glyph-by-glyph.
            for (int i = 0; i < glyphs.Length; ++i)
            {
                CanvasGlyph[] singleGlyph = new CanvasGlyph[1];
                singleGlyph[0] = glyphs[i];

                Vector2 positionForThisGlyph = position;

                CustomBrushData brushData = (CustomBrushData)brush;
                if (brushData != null)
                {
                    if (brushData.BaselineAdjustment == CustomBrushData.BaselineAdjustmentType.Lower)
                    {
                        positionForThisGlyph.Y += subscriptBaselineDropAmount;
                    }
                    else if (brushData.BaselineAdjustment == CustomBrushData.BaselineAdjustmentType.Raise)
                    {
                        positionForThisGlyph.Y -= superscriptBaselineRaiseAmount;
                    }
                }

                DrawingSession.DrawGlyphRun(
                    positionForThisGlyph,
                    fontFace,
                    fontSize,
                    singleGlyph,
                    isSideways,
                    bidiLevel,
                    TextBrush);

                position.X += glyphs[i].Advance;
            }
        }

        public void DrawStrikethrough(
            Vector2 position,
            float strikethroughWidth,
            float strikethroughThickness,
            float strikethroughOffset,
            CanvasTextDirection textDirection,
            object brush,
            CanvasTextMeasuringMode measuringMode,
            string locale,
            CanvasGlyphOrientation glyphOrientation)
        {
            // Normally, we'd add strikethrough support here. Strikethrough isn't used by this demo.
        }

        public void DrawUnderline(
            Vector2 position,
            float underlineWidth,
            float underlineThickness,
            float underlineOffset,
            float runHeight,
            CanvasTextDirection textDirection,
            object brush,
            CanvasTextMeasuringMode measuringMode,
            string locale,
            CanvasGlyphOrientation glyphOrientation)
        {
            // Normally, we'd add underline support here. Underline isn't used by this demo.
        }

        public void DrawInlineObject(
            Vector2 baselineOrigin,
            ICanvasTextInlineObject inlineObject,
            bool isSideways,
            bool isRightToLeft,
            object brush,
            CanvasGlyphOrientation glyphOrientation)
        {
            // This sample doesn't use inline objects.
        }

        public float Dpi { get { return 96; } }

        public bool PixelSnappingDisabled { get { return false; } }

        public Matrix3x2 Transform { get { return System.Numerics.Matrix3x2.Identity; } }
    }

}
