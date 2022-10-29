using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using static System.Net.Mime.MediaTypeNames;

#if WINDOWS
namespace TextBlockFX.Win2D.WinUI.Effects
#else
namespace TextBlockFX.Win2D.UWP.Effects
#endif
{
    public class GlowTextAnimated : ITextEffectAnimated
    {
        public object Sender { get; set; }
        /// <inheritdoc />
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

            if (state == RedrawState.Idle)
            {

                DoEffect(drawingSession, newTextLayout.RequestedSize, GlowAmount, newText);
                return;
            }
            DoEffect(drawingSession, newTextLayout.RequestedSize, GlowAmount, newText);

        }
        public GlowTextAnimated()
        {

        }
        public GlowTextAnimated(Color glowColor)
        {
            GlowColor = glowColor;
        }
        public HorizontalAlignment HorizontalContentAlignment { get; set; } = HorizontalAlignment.Stretch;
        public VerticalAlignment VerticalContentAlignment { get; set; } = VerticalAlignment.Center;
        public double ExpandAmount { get; set; } = 40;
        public float GlowAmount { get; set; } = 40;
        public  Color GlowColor { get; set; } = Colors.BlueViolet;
       
        private GlowEffectGraph glowEffectGraph = new GlowEffectGraph();
        private void DoEffect(CanvasDrawingSession ds, Size size, float amount, string text)
        {
            size.Width = size.Width - ExpandAmount;
            size.Height = size.Height - ExpandAmount;

            var offset = (float)(ExpandAmount / 2);

            
            using (var textCommandList = new CanvasCommandList(ds))
            {
                using (var textDs = textCommandList.CreateDrawingSession())
                {
                    textDs.DrawTextLayout(this.EffectParam.NewTextLayout, 0, 0, GlowColor);
                }

                glowEffectGraph.Setup(textCommandList, amount);
                ds.DrawImage(glowEffectGraph.Output, offset, offset);

                ds.DrawTextLayout(this.EffectParam.NewTextLayout, offset, offset, this.EffectParam.TextColor);
            }
        }
       
        private CanvasHorizontalAlignment GetCanvasHorizontalAlignemnt()
        {
            switch (HorizontalContentAlignment)
            {
                case HorizontalAlignment.Center:
                    return CanvasHorizontalAlignment.Center;

                case HorizontalAlignment.Left:
                    return CanvasHorizontalAlignment.Left;

                case HorizontalAlignment.Right:
                    return CanvasHorizontalAlignment.Right;

                default:
                    return CanvasHorizontalAlignment.Left;
            }
        }

        private CanvasVerticalAlignment GetCanvasVerticalAlignment()
        {
            switch (VerticalContentAlignment)
            {
                case VerticalAlignment.Center:
                    return CanvasVerticalAlignment.Center;

                case VerticalAlignment.Top:
                    return CanvasVerticalAlignment.Top;

                case VerticalAlignment.Bottom:
                    return CanvasVerticalAlignment.Bottom;

                default:
                    return CanvasVerticalAlignment.Top;
            }
        }

       
    }
   
}
