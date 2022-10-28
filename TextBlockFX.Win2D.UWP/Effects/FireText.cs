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
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.UI;
using System.Numerics;
using Windows.UI.Composition;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.Foundation;

#if WINDOWS
namespace TextBlockFX.Win2D.WinUI.Effects
#else
namespace TextBlockFX.Win2D.UWP.Effects
#endif
{
    public class FireText : ITextEffect
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
        TextEffectParam EffectParam;
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
            EffectParam = new TextEffectParam(oldText,
               newText,
               diffResults,
               oldTextLayout,
               newTextLayout,
               textFormat,
               textColor,
               gradientBrush,
               state,
               drawingSession,
               args);
            var ds = args.DrawingSession;
            text = oldText;
            newText = newText1;
            _textFormat = textFormat;
            _textColor = textColor;
            CreateFlameEffect();
            if (state == RedrawState.Idle)
            {

                Canvas_Draw(newTextLayout.RequestedSize, args);
                return;
            }
            Canvas_Draw(newTextLayout.RequestedSize, args);
        }
        Color _textColor = Colors.White;
        CanvasTextFormat _textFormat;
        // References to specific effects so we can dynamically update their properties.
        CanvasCommandList textCommandList;
        MorphologyEffect morphology;
        CompositeEffect composite;
        Transform2DEffect flameAnimation;
        Transform2DEffect flamePosition;

        string text, newText;
        float fontSize;

        
        private void Canvas_Draw(Size size, CanvasAnimatedDrawEventArgs args)
        {
            var ds = args.DrawingSession;

            // If the text or font size has changed then recreate the text command list.
            var newFontSize = GetFontSize(size);
            if (newText != text || newFontSize != fontSize)
            {
                text = newText;
                fontSize = newFontSize;
                SetupText(args.DrawingSession);
            };

            ConfigureEffect(args.Timing);

            ds.DrawImage(composite, size.ToVector2() / 2);
        }

        private void ConfigureEffect(CanvasTimingInformation timing)
        {
            // Animate the flame by shifting the Perlin noise upwards (-Y) over time.
            flameAnimation.TransformMatrix = Matrix3x2.CreateTranslation(0, -(float)timing.TotalTime.TotalSeconds * 60.0f);

            // Scale the flame effect 2x vertically, aligned so it starts above the text.
            float verticalOffset = fontSize * 1.4f;

            var centerPoint = new Vector2(0, verticalOffset);

            flamePosition.TransformMatrix = Matrix3x2.CreateScale(1, 2, centerPoint);
        }

        /// <summary>
        /// Renders text into a command list and sets this as the input to the flame
        /// effect graph. The effect graph must already be created before calling this method.
        /// </summary>
        private void SetupText(ICanvasResourceCreator resourceCreator)
        {
            textCommandList = new CanvasCommandList(resourceCreator);

            using (var ds = textCommandList.CreateDrawingSession())
            {
                fontSize = _textFormat.FontSize;
                ds.Clear(Color.FromArgb(0, 0, 0, 0));
                _textFormat.VerticalAlignment = CanvasVerticalAlignment.Top;
                _textFormat.HorizontalAlignment = CanvasHorizontalAlignment.Center;
                ds.DrawText(
                    text,
                    0,
                    0,
                    _textColor,
                    new CanvasTextFormat
                    {
                        FontFamily = _textFormat.FontFamily,
                        FontStretch = _textFormat.FontStretch,
                        FontWeight = _textFormat.FontWeight,
                        FontStyle = _textFormat.FontStyle,                      
                        FontSize = fontSize,
                        HorizontalAlignment = CanvasHorizontalAlignment.Center,
                        VerticalAlignment = CanvasVerticalAlignment.Top
                    });

            }

            // Hook up the command list to the inputs of the flame effect graph.
            morphology.Source = textCommandList;
            composite.Sources[1] = textCommandList;
        }

        /// <summary>
        /// Calculates a good font size so the text will fit even on smaller phone screens.
        /// </summary>
        private static float GetFontSize(Size displaySize)
        {
            const float maxFontSize = 72;
            const float scaleFactor = 12;

            return Math.Min((float)displaySize.Width / scaleFactor, maxFontSize);
        }

        /// <summary>
        /// Generate the flame effect graph. This method is called before the text command list
        /// (input) is created.
        /// </summary>
        private void CreateFlameEffect()
        {
            // Thicken the text.
            morphology = new MorphologyEffect
            {
                // The Source property is set by SetupText().
                Mode = MorphologyEffectMode.Dilate,
                Width = 7,
                Height = 1
            };

            // Blur, then colorize the text from black to red to orange as the alpha increases.
            var colorize = new ColorMatrixEffect
            {
                Source = new GaussianBlurEffect
                {
                    Source = morphology,
                    BlurAmount = 3f
                },
                ColorMatrix = new Matrix5x4
                {
                    M11 = 0f,
                    M12 = 0f,
                    M13 = 0f,
                    M14 = 0f,
                    M21 = 0f,
                    M22 = 0f,
                    M23 = 0f,
                    M24 = 0f,
                    M31 = 0f,
                    M32 = 0f,
                    M33 = 0f,
                    M34 = 0f,
                    M41 = 0f,
                    M42 = 1f,
                    M43 = 0f,
                    M44 = 1f,
                    M51 = 1f,
                    M52 = -0.5f,
                    M53 = 0f,
                    M54 = 0f
                }
            };

            // Generate a Perlin noise field (see flamePosition).
            // Animate the noise by modifying flameAnimation's transform matrix at render time.
            flameAnimation = new Transform2DEffect
            {
                Source = new BorderEffect
                {
                    Source = new TurbulenceEffect
                    {
                        Frequency = new Vector2(0.109f, 0.109f),
                        Size = new Vector2(500.0f, 80.0f)
                    },
                    // Use Mirror extend mode to allow us to spatially translate the noise
                    // without any visible seams.
                    ExtendX = CanvasEdgeBehavior.Mirror,
                    ExtendY = CanvasEdgeBehavior.Mirror
                }
            };

            // Give the flame its wavy appearance by generating a displacement map from the noise
            // (see flameAnimation) and applying this to the text.
            // Stretch and position this flame behind the original text.
            flamePosition = new Transform2DEffect
            {
                Source = new DisplacementMapEffect
                {
                    Source = colorize,
                    Displacement = flameAnimation,
                    Amount = 40.0f
                }
                // Set the transform matrix at render time as it depends on window size.
            };

            // Composite the text over the flames.
            composite = new CompositeEffect()
            {
                Sources = { flamePosition, null }
                // Replace null with the text command list when it is created.
            };
        }

       

        // Alternative entrypoint for use by AppIconGenerator.
        internal void DrawIcon(CanvasDrawingSession drawingSession, string text)
        {
            this.text = text;
            this.fontSize = 64;

            CreateFlameEffect();
            SetupText(drawingSession);
            ConfigureEffect(new CanvasTimingInformation());

            drawingSession.DrawImage(flamePosition);
            drawingSession.DrawImage(textCommandList);
        }
        
    }
}
