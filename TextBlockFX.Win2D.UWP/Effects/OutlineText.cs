using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.Graphics.Canvas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using Windows.UI;
using Microsoft.Graphics.Canvas.Geometry;
using System.Numerics;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.Storage;
using System.Runtime.InteropServices.WindowsRuntime;

#if WINDOWS
namespace TextBlockFX.Win2D.WinUI.Effects
#else
namespace TextBlockFX.Win2D.UWP.Effects
#endif
{
    public class OutlineText : ITextEffect
    {
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
        public void DrawText(string oldText,
            string newText1,
            List<TextDiffResult> diffResults,
            CanvasTextLayout oldTextLayout,
            CanvasTextLayout newTextLayout,
            CanvasTextFormat textFormat,
            Color textColor,
            CanvasLinearGradientBrush gradientBrush,
            RedrawState state,
            CanvasDrawingSession drawingSession,
            CanvasAnimatedDrawEventArgs args)
        {
            if (diffResults == null)
                return;

            var ds = args.DrawingSession;
            
            textLayout = newTextLayout;
            _textFormat = textFormat;
            testString = newText1;
            if (state == RedrawState.Idle)
            {

                Canvas_Draw(args.DrawingSession, newTextLayout.RequestedSize, args);
                return;
            }
            Canvas_Draw(args.DrawingSession, newTextLayout.RequestedSize, args);
        }
        //Engine
        string testString = "";
        CanvasTextLayout textLayout;
        CanvasGeometry textGeometry;
        CanvasTextFormat _textFormat;
        CanvasStrokeStyle dashedStroke = new CanvasStrokeStyle()
        {
            DashStyle = CanvasDashStyle.Dash
        };
       
        List<Utils.WordBoundary> everyOtherWordBoundary;

        public bool ShowNonOutlineText { get; set; }

        bool needsResourceRecreation = true;

        public List<CanvasTextDirection> TextDirectionOptions { get { return Utils.GetEnumAsList<CanvasTextDirection>(); } }
        public CanvasTextDirection CurrentTextDirection { get; set; }

        public List<CanvasVerticalGlyphOrientation> VerticalGlyphOrientationOptions { get { return Utils.GetEnumAsList<CanvasVerticalGlyphOrientation>(); } }
        public CanvasVerticalGlyphOrientation CurrentVerticalGlyphOrientation { get; set; }

        public enum TextLengthOption { Short, Paragraph };
        public List<TextLengthOption> TextLengthOptions { get { return Utils.GetEnumAsList<TextLengthOption>(); } }
        public TextLengthOption CurrentTextLengthOption { get; set; }

        public enum TextEffectOption { None, UnderlineEveryOtherWord, StrikeEveryOtherWord };
        public List<TextEffectOption> TextEffectOptions { get { return Utils.GetEnumAsList<TextEffectOption>(); } }
        public TextEffectOption CurrentTextEffectOption { get; set; }

        //
        // Apps using text-to-geometry will typically use the 'Layout' option. 
        //
        // The 'GlyphRun' option exercises a lower-level API, and demonstrates 
        // how a custom text renderer could use text-to-geometry. The visual 
        // output between these two options should be identical.
        //
        public enum TextOutlineGranularity { Layout, GlyphRun }

        public List<TextOutlineGranularity> TextOutlineGranularityOptions { get { return Utils.GetEnumAsList<TextOutlineGranularity>(); } }
        public TextOutlineGranularity CurrentTextOutlineGranularityOption { get; set; }
        public OutlineText()
        {
            CurrentTextEffectOption = TextEffectOption.None;
            CurrentTextDirection = CanvasTextDirection.LeftToRightThenTopToBottom;
            CurrentVerticalGlyphOrientation = CanvasVerticalGlyphOrientation.Default;
            CurrentTextLengthOption = TextLengthOption.Paragraph;
            CurrentTextOutlineGranularityOption = TextOutlineGranularity.GlyphRun;
            ShowNonOutlineText = true;
            CurrentTextOutlineGranularityOption = TextOutlineGranularity.Layout;
        }
        class GlyphRunsToGeometryConverter : ICanvasTextRenderer
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

        void EnsureResources(ICanvasResourceCreatorWithDpi resourceCreator, Size targetSize)
        {
            if (!needsResourceRecreation)
                return;

            if (textLayout != null)
            {
                //textLayout.Dispose();
                //if(textGeometry!= null)
                //  textGeometry.Dispose();
            }

            ApplyToTextLayout(resourceCreator, (float)targetSize.Width, (float)targetSize.Height);

            if (CurrentTextOutlineGranularityOption == TextOutlineGranularity.Layout)
            {
                textGeometry = CanvasGeometry.CreateText(textLayout);
            }
            else
            {
                GlyphRunsToGeometryConverter converter = new GlyphRunsToGeometryConverter(resourceCreator);

                textLayout.DrawToTextRenderer(converter, 0, 0);

                textGeometry = converter.GetGeometry();
            }

            //needsResourceRecreation = false;
        }

        private void ApplyToTextLayout(ICanvasResourceCreator resourceCreator, float canvasWidth, float canvasHeight)
        {
            
            everyOtherWordBoundary = Utils.GetEveryOtherWord(testString);

            float sizeDim = Math.Min(canvasWidth, canvasHeight);

           
            if (CurrentTextEffectOption == TextEffectOption.UnderlineEveryOtherWord)
            {
                foreach (var wb in everyOtherWordBoundary)
                {
                    textLayout.SetUnderline(wb.Start, wb.Length, true);
                }
            }
            else if (CurrentTextEffectOption == TextEffectOption.StrikeEveryOtherWord)
            {
                foreach (var wb in everyOtherWordBoundary)
                {
                    textLayout.SetStrikethrough(wb.Start, wb.Length, true);
                }
            }

            textLayout.TrimmingGranularity = CanvasTextTrimmingGranularity.Character;
            textLayout.TrimmingSign = CanvasTrimmingSign.Ellipsis;

            textLayout.VerticalGlyphOrientation = CurrentVerticalGlyphOrientation;

            
        }

        private void Canvas_Draw(CanvasDrawingSession sender, Size size, CanvasAnimatedDrawEventArgs args)
        {
            EnsureResources(sender, size);

            float strokeWidth = CurrentTextLengthOption == TextLengthOption.Paragraph ? 2.0f : 15.0f;
           
            args.DrawingSession.DrawGeometry(textGeometry, Colors.White, strokeWidth, dashedStroke);

            if (ShowNonOutlineText)
            {
                Color semitrans = Colors.CornflowerBlue;
                semitrans.A = 127;
                args.DrawingSession.DrawTextLayout(textLayout, 0, 0, semitrans);
            }
        }
        
    }
    static class Utils
    {
        public static List<T> GetEnumAsList<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>().ToList();
        }

        public static Matrix3x2 GetDisplayTransform(Vector2 outputSize, Vector2 sourceSize)
        {
            // Scale the display to fill the control.
            var scale = outputSize / sourceSize;
            var offset = Vector2.Zero;

            // Letterbox or pillarbox to preserve aspect ratio.
            if (scale.X > scale.Y)
            {
                scale.X = scale.Y;
                offset.X = (outputSize.X - sourceSize.X * scale.X) / 2;
            }
            else
            {
                scale.Y = scale.X;
                offset.Y = (outputSize.Y - sourceSize.Y * scale.Y) / 2;
            }

            return Matrix3x2.CreateScale(scale) *
                   Matrix3x2.CreateTranslation(offset);
        }

        public static CanvasGeometry CreateStarGeometry(ICanvasResourceCreator resourceCreator, float scale, Vector2 center)
        {
            Vector2[] points =
            {
                new Vector2(-0.24f, -0.24f),
                new Vector2(0, -1),
                new Vector2(0.24f, -0.24f),
                new Vector2(1, -0.2f),
                new Vector2(0.4f, 0.2f),
                new Vector2(0.6f, 1),
                new Vector2(0, 0.56f),
                new Vector2(-0.6f, 1),
                new Vector2(-0.4f, 0.2f),
                new Vector2(-1, -0.2f),
            };

            var transformedPoints = from point in points
                                    select point * scale + center;

            return CanvasGeometry.CreatePolygon(resourceCreator, transformedPoints.ToArray());
        }

        public static float DegreesToRadians(float angle)
        {
            return angle * (float)Math.PI / 180;
        }

        static readonly Random random = new Random();

        public static Random Random
        {
            get { return random; }
        }

        public static float RandomBetween(float min, float max)
        {
            return min + (float)random.NextDouble() * (max - min);
        }

        public static async Task<byte[]> ReadAllBytes(string filename)
        {
            var uri = new Uri("ms-appx:///" + filename);
            var file = await StorageFile.GetFileFromApplicationUriAsync(uri);
            var buffer = await FileIO.ReadBufferAsync(file);

            return buffer.ToArray();
        }

        public static async Task<T> TimeoutAfter<T>(this Task<T> task, TimeSpan timeout)
        {
            if (task == await Task.WhenAny(task, Task.Delay(timeout)))
            {
                return await task;
            }
            else
            {
                throw new TimeoutException();
            }
        }

        public struct WordBoundary { public int Start; public int Length; }

        public static List<WordBoundary> GetEveryOtherWord(string str)
        {
            List<WordBoundary> result = new List<WordBoundary>();

            for (int i = 0; i < str.Length; ++i)
            {
                if (str[i] == ' ')
                {
                    int nextSpace = str.IndexOf(' ', i + 1);
                    int limit = nextSpace == -1 ? str.Length : nextSpace;

                    WordBoundary wb = new WordBoundary();
                    wb.Start = i + 1;
                    wb.Length = limit - i - 1;
                    result.Add(wb);
                    i = limit;
                }
            }
            return result;
        }
    }
}
