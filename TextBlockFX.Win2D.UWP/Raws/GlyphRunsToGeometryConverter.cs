using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace TextBlockFX.Win2D.UWP
{
    public class GlyphRunsToGeometryConverter : ICanvasTextRenderer
    {
        private List<CanvasGeometry> geometries = new List<CanvasGeometry>();
        private ICanvasResourceCreator resourceCreator;

        public GlyphRunsToGeometryConverter(ICanvasResourceCreator rc)
        {
            resourceCreator = rc;
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
            CanvasGeometry geometry = CanvasGeometry.CreateGlyphRun(
                resourceCreator,
                position,
                fontFace,
                fontSize,
                glyphs,
                isSideways,
                bidiLevel,
                measuringMode,
                glyphOrientation);
            geometries.Add(geometry);
        }

        public CanvasGeometry GetGeometry()
        {
            return CanvasGeometry.CreateGroup(resourceCreator, geometries.ToArray());
        }

        private float GetGlyphOrientationInRadians(CanvasGlyphOrientation glyphOrientation)
        {
            switch (glyphOrientation)
            {
                case CanvasGlyphOrientation.Upright: return 0;
                case CanvasGlyphOrientation.Clockwise90Degrees: return (float)Math.PI / 2;
                case CanvasGlyphOrientation.Clockwise180Degrees: return -(float)Math.PI;
                case CanvasGlyphOrientation.Clockwise270Degrees:
                default: return -(float)Math.PI / 2;
            }
        }

        private CanvasGeometry GetTransformedRectangle(
            float width,
            float thickness,
            float offset,
            Vector2 position,
            CanvasGlyphOrientation glyphOrientation)
        {
            var geometry = CanvasGeometry.CreateRectangle(
                resourceCreator,
                new Rect(0, offset, width, thickness));

            var rotate = System.Numerics.Matrix3x2.CreateRotation(GetGlyphOrientationInRadians(glyphOrientation));
            var translate = System.Numerics.Matrix3x2.CreateTranslation(position);

            return geometry.Transform(rotate * translate);
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
            var geometry = GetTransformedRectangle(strikethroughWidth, strikethroughThickness, strikethroughOffset, position, glyphOrientation);

            geometries.Add(geometry);
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
            var geometry = GetTransformedRectangle(underlineWidth, underlineThickness, underlineOffset, position, glyphOrientation);

            geometries.Add(geometry);
        }

        public void DrawInlineObject(
            Vector2 baselineOrigin,
            ICanvasTextInlineObject inlineObject,
            bool isSideways,
            bool isRightToLeft,
            object brush,
            CanvasGlyphOrientation glyphOrientation)
        {
        }

        public float Dpi { get { return 96; } }

        public bool PixelSnappingDisabled { get { return true; } }

        public Matrix3x2 Transform { get { return System.Numerics.Matrix3x2.Identity; } }

    }

}
