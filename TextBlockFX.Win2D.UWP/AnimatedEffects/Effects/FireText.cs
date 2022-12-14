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
    public class FireAnimated : ITextEffectAnimated
    {
        public object Sender { get; set; }
        public TimeSpan AnimationDuration { get; set; } = TimeSpan.FromMilliseconds(800);

        /// <inheritdoc />
        public TimeSpan DelayPerCluster { get; set; } = TimeSpan.FromMilliseconds(10);
        public void Update( string oldText,
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
        public void DrawText( string oldText,
            string newText,
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
            EffectParam = new TextEffectParam( oldText,
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
            
            if (state == RedrawState.Idle)
            {

                Canvas_Draw(newTextLayout.RequestedSize, args);
                return;
            }
            Canvas_Draw(newTextLayout.RequestedSize, args);
        }
       
        // References to specific effects so we can dynamically update their properties.
        CanvasCommandList textCommandList;
      BurningEffectGraph burningEffectGraph = new BurningEffectGraph();
        private void Canvas_Draw(Size size, CanvasAnimatedDrawEventArgs args)
        {
            var ds = args.DrawingSession;

            // If the text or font size has changed then recreate the text command list.
            var newFontSize = GetFontSize(size);
            SetupText(args.DrawingSession);
            ConfigureEffect(args.Timing);

            ds.DrawImage(burningEffectGraph.composite, size.ToVector2() / 2);
        }

        private void ConfigureEffect(CanvasTimingInformation timing)
        {
            // Animate the flame by shifting the Perlin noise upwards (-Y) over time.
            burningEffectGraph.flameAnimation.TransformMatrix = Matrix3x2.CreateTranslation(0, -(float)timing.TotalTime.TotalSeconds * 60.0f);

            // Scale the flame effect 2x vertically, aligned so it starts above the text.
            float verticalOffset = this.EffectParam.TextFormat.FontSize * 1.4f;

            var centerPoint = new Vector2(0, verticalOffset);

            burningEffectGraph.flamePosition.TransformMatrix = Matrix3x2.CreateScale(1, 2, centerPoint);
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
                
                ds.Clear(Color.FromArgb(0, 0, 0, 0));
               
                ds.DrawText(
                    this.EffectParam.NewText,
                    0,
                    0,
                    this.EffectParam.TextColor,
                    new CanvasTextFormat
                    {
                        FontFamily = this.EffectParam.TextFormat.FontFamily,
                        FontStretch = this.EffectParam.TextFormat.FontStretch,
                        FontWeight = this.EffectParam.TextFormat.FontWeight,
                        FontStyle = this.EffectParam.TextFormat.FontStyle,                      
                        FontSize = this.EffectParam.TextFormat.FontSize,
                        HorizontalAlignment = CanvasHorizontalAlignment.Center,
                        VerticalAlignment = CanvasVerticalAlignment.Center,
                        WordWrapping = this.EffectParam.TextFormat.WordWrapping,
                    });

            }

            // Hook up the command list to the inputs of the flame effect graph.
            burningEffectGraph.morphology.Source = textCommandList;
            burningEffectGraph.composite.Sources[1] = textCommandList;
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
       

        
    }
}
