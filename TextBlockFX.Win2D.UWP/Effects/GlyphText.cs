using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.Graphics.Canvas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.Graphics.Canvas.Geometry;
using System.Numerics;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;

#if WINDOWS
namespace TextBlockFX.Win2D.WinUI.Effects
#else
namespace TextBlockFX.Win2D.UWP.Effects
#endif
{
    public class GlyphText : ITextEffect
    {
        /// <inheritdoc />
        public TimeSpan AnimationDuration { get; set; } = TimeSpan.FromMilliseconds(800);

        /// <inheritdoc />
        public TimeSpan DelayPerCluster { get; set; } = TimeSpan.FromMilliseconds(10);
        public void Update(string oldText,
            string newText,
            List<TextDiffResult> diffResults,
            CanvasTextLayout oldTextLayout,
            CanvasTextLayout newTextLayout,
            RedrawState state,
            ICanvasAnimatedControl canvas,
            CanvasAnimatedUpdateEventArgs args)
        {

        }
        public Color TextBrush { get; set; } = Colors.LightBlue;
        public void SetTextBoundary(params string[] words)
        {
            if (words.Length == 0)
                return;

        }
        public void DrawText(string oldText,
            string newText,
            List<TextDiffResult> diffResults,
            CanvasTextLayout oldTextLayout,
            CanvasTextLayout newTextLayout,
            CanvasTextFormat textFormat,
            Color textColor,
            CanvasLinearGradientBrush gradientBrush,
            RedrawState state,
            CanvasDrawingSession _drawingSession,
            CanvasAnimatedDrawEventArgs args)
        {
            if (diffResults == null)
                return;
           
            var ds = args.DrawingSession;
            textLayout = newTextLayout;
            testString = newText;
            _textFormat = textFormat;
            everyOtherWordBoundary = Utils.GetEveryOtherWord(testString);
            EnsureResources(ds, newTextLayout.RequestedSize);
            if (state == RedrawState.Idle)
            {

                Canvas_Draw(ds, args);
                return;
            }
            Canvas_Draw(ds, args);
        }
        public GlyphText()
        {
            CurrentTextEffectOption = TextEffectOption.UnderlineEveryOtherWord;
            CurrentTextDirection = CanvasTextDirection.LeftToRightThenTopToBottom;
            CurrentVerticalGlyphOrientation = CanvasVerticalGlyphOrientation.Default;

            ShowNonCustomText = true;

            testString = "";

            everyOtherWordBoundary = Utils.GetEveryOtherWord(testString);
        }
        //Engine
        CanvasTextFormat _textFormat;
         CanvasTextLayout textLayout;
        CanvasGeometry textReference;
        List<Utils.WordBoundary> everyOtherWordBoundary;
        CanvasSolidColorBrush textBrush;
        string testString;

        public bool ShowNonCustomText { get; set; }

        bool needsResourceRecreation = true;

        public List<CanvasTextDirection> TextDirectionOptions { get { return Utils.GetEnumAsList<CanvasTextDirection>(); } }
        public CanvasTextDirection CurrentTextDirection { get; set; }

        public List<CanvasVerticalGlyphOrientation> VerticalGlyphOrientationOptions { get { return Utils.GetEnumAsList<CanvasVerticalGlyphOrientation>(); } }
        public CanvasVerticalGlyphOrientation CurrentVerticalGlyphOrientation { get; set; }

        public enum TextEffectOption { None, UnderlineEveryOtherWord, StrikeEveryOtherWord };
        public List<TextEffectOption> TextEffectOptions { get { return Utils.GetEnumAsList<TextEffectOption>(); } }
        public TextEffectOption CurrentTextEffectOption { get; set; }

        class CustomTextRenderer : ICanvasTextRenderer
        {
            CanvasSolidColorBrush textBrush;

            public CustomTextRenderer(CanvasSolidColorBrush brush)
            {
                textBrush = brush;
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

        void EnsureResources(ICanvasResourceCreatorWithDpi resourceCreator, Size targetSize)
        {
            if (!needsResourceRecreation)
                return;

           ApplyToTextLayout(resourceCreator, (float)targetSize.Width, (float)targetSize.Height);

            textReference = CanvasGeometry.CreateText(textLayout);

            textBrush = new CanvasSolidColorBrush(resourceCreator, TextBrush);
        }

        private void ApplyToTextLayout(ICanvasResourceCreator resourceCreator, float canvasWidth, float canvasHeight)
        {
            float sizeDim = Math.Min(canvasWidth, canvasHeight);

            CanvasTextFormat textFormat = new CanvasTextFormat()
            {
                FontSize = sizeDim * 0.085f, // sizeDim * 0.25f : sizeDim * 0.085f,
                Direction = CurrentTextDirection,
                FontFamily = _textFormat.FontFamily,
                FontStretch = _textFormat.FontStretch,
                FontStyle = _textFormat.FontStyle,
                FontWeight = _textFormat.FontWeight,
            };

            //CanvasTextLayout textLayout = new CanvasTextLayout(resourceCreator, testString, textFormat, canvasWidth, canvasHeight);

            if (CurrentTextEffectOption == TextEffectOption.UnderlineEveryOtherWord)
            {
                foreach (Utils.WordBoundary wb in everyOtherWordBoundary)
                {
                    textLayout.SetUnderline(wb.Start, wb.Length, true);
                }
            }
            else if (CurrentTextEffectOption == TextEffectOption.StrikeEveryOtherWord)
            {
                foreach (Utils.WordBoundary wb in everyOtherWordBoundary)
                {
                    textLayout.SetStrikethrough(wb.Start, wb.Length, true);
                }
            }

            textLayout.VerticalGlyphOrientation = CurrentVerticalGlyphOrientation;

           
        }
        static CanvasDrawingSession drawingSession;
        private void Canvas_Draw(CanvasDrawingSession senser, CanvasAnimatedDrawEventArgs args)
        {
            EnsureResources(senser, textLayout.RequestedSize);

            drawingSession = args.DrawingSession;

            CustomTextRenderer textRenderer = new CustomTextRenderer(textBrush);
            textLayout.DrawToTextRenderer(textRenderer, 0, 0);

            if (ShowNonCustomText)
            {
                Color semitrans = Colors.White;
                semitrans.A = 127;

                float strokeWidth = 5.0f;  // 22.0f : 5.0f;
                args.DrawingSession.DrawGeometry(textReference, semitrans, strokeWidth);
            }
        }

        

       
    }
}
