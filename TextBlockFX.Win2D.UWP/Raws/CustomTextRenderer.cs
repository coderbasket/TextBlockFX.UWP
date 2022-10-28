using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace TextBlockFX.Win2D.UWP
{
    public class CustomTextRenderer : ICanvasTextRenderer
    {
        CanvasSolidColorBrush textBrush;
        CanvasDrawingSession drawingSession;
        public CustomTextRenderer(CanvasSolidColorBrush brush, CanvasDrawingSession ds)
        {
            textBrush = brush;
            drawingSession = ds;
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
            int[] custerMapIndices,
            uint textPosition,
            CanvasGlyphOrientation glyphOrientation)
        {
            if (glyphs == null)
                return;

            var previousTransform = drawingSession.Transform;

            drawingSession.Transform = CanvasTextLayout.GetGlyphOrientationTransform(glyphOrientation, isSideways, position);

            drawingSession.DrawGlyphRun(
                position,
                fontFace,
                fontSize,
                glyphs,
                isSideways,
                bidiLevel,
                textBrush);

            drawingSession.Transform = previousTransform;
        }

        private Rect RotateRectangle(Rect r, CanvasGlyphOrientation orientation)
        {
            switch (orientation)
            {
                case CanvasGlyphOrientation.Clockwise90Degrees:
                    return new Rect(new Point(-r.Top, r.Left), new Point(-r.Bottom, r.Right));
                case CanvasGlyphOrientation.Clockwise180Degrees:
                    return new Rect(new Point(-r.Left, -r.Top), new Point(-r.Right, -r.Bottom));
                case CanvasGlyphOrientation.Clockwise270Degrees:
                    return new Rect(new Point(r.Top, -r.Left), new Point(r.Bottom, -r.Right));
                case CanvasGlyphOrientation.Upright:
                default:
                    return r;
            }
        }

        private Rect GetLineRectangle(
            Vector2 position,
            float lineWidth,
            float lineThickness,
            float lineOffset,
            CanvasGlyphOrientation glyphOrientation)
        {
            Rect rect = new Rect(0, lineOffset, lineWidth, lineThickness);
            rect = RotateRectangle(rect, glyphOrientation);
            rect.X += position.X;
            rect.Y += position.Y;
            return rect;
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
            drawingSession.FillRectangle(
                GetLineRectangle(position, strikethroughWidth, strikethroughThickness, strikethroughOffset, glyphOrientation),
                textBrush);
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
            drawingSession.FillRectangle(
                GetLineRectangle(position, underlineWidth, underlineThickness, underlineOffset, glyphOrientation),
                textBrush);
        }

        public void DrawInlineObject(
            Vector2 baselineOrigin,
            ICanvasTextInlineObject inlineObject,
            bool isSideways,
            bool isRightToLeft,
            object brush,
            CanvasGlyphOrientation glyphOrientation)
        {
            // There aren't any inline objects in this sample.
        }

        public float Dpi { get { return 96; } }

        public bool PixelSnappingDisabled { get { return true; } }

        public Matrix3x2 Transform { get { return System.Numerics.Matrix3x2.Identity; } }

    }
}
