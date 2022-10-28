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
using Windows.Foundation;
using System.Diagnostics;
using System.Numerics;
using Windows.UI.Text;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

#if WINDOWS
namespace TextBlockFX.Win2D.WinUI.Effects
#else
namespace TextBlockFX.Win2D.UWP.Effects
#endif
{
    public class AdvanceEffect : ITextEffect
    {
        public object Sender { get; set; }
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
        TextEffectParam EffectParam;
        public void DrawText(string oldText,
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

            EnsureResources(ds);
            if (state == RedrawState.Idle)
            {

                Canvas_Draw(ds, args);
                return;
            }
            Canvas_Draw(ds, args);
        }
        //Engine
        public AdvanceEffect()
        {

        }



        void EnsureResources(ICanvasResourceCreatorWithDpi resourceCreator)
        {
            var targetSize = this.EffectParam.NewTextLayout.RequestedSize;
            float canvasWidth = (float)targetSize.Width;
            float canvasHeight = (float)targetSize.Height;
            var sizeDim = Math.Min(canvasWidth, canvasHeight);

            var textBrush = new CanvasSolidColorBrush(resourceCreator, Colors.Thistle);
            SetScript();
        }
        bool process = false;
        void SetScript()
        {
            if (process)
                return;
            process = true;
            if (this.Sender is TextBlockFX fx)
            {
                foreach (var sp in fx.SuperScripts)
                {
                    SetSuperscript(this.EffectParam.NewText.IndexOf(sp.Words), sp.Length);
                }
            }
            process = false;
        }



        bool ShowUnformatted;
        private void Canvas_Draw(CanvasDrawingSession sender, CanvasAnimatedDrawEventArgs args)
        {
            EnsureResources(sender);

            if (ShowUnformatted)
            {
                args.DrawingSession.DrawTextLayout(this.EffectParam.NewTextLayout, 0, 0, Colors.DarkGray);
            }
            args.DrawingSession.DrawTextLayout(this.EffectParam.NewTextLayout, 0, 0, Colors.Transparent);
            InitScript();

        }


        #region Script

        SubscriptSuperscriptRenderer subscriptSuperscriptRenderer = null;
        /// <summary>
        ///  SetSubscript(sampleText.IndexOf("subscript"), "subscript".Length);
        /// </summary>
        /// <param name="textPosition"></param>
        /// <param name="characterCount"></param>
        public void SetSubscript(int textPosition, int characterCount)
        {
            if (subscriptSuperscriptRenderer == null)
                subscriptSuperscriptRenderer = new SubscriptSuperscriptRenderer();
            TextEffectsHelper.ShrinkFontAndAttachCustomBrushData(this.EffectParam.NewTextLayout, textPosition, characterCount, CustomBrushData.BaselineAdjustmentType.Lower, this.EffectParam.TextFormat);
        }
        /// <summary>
        /// SetSubscript(textString.IndexOf("H2O") + 1, 1);       
        /// </summary>
        /// <param name="textPosition"></param>
        /// <param name="characterCount"></param>
        public void SetSuperscript(int textPosition, int characterCount)
        {
            if (subscriptSuperscriptRenderer == null)
                subscriptSuperscriptRenderer = new SubscriptSuperscriptRenderer();
            TextEffectsHelper.ShrinkFontAndAttachCustomBrushData(this.EffectParam.NewTextLayout, textPosition, characterCount, CustomBrushData.BaselineAdjustmentType.Raise, this.EffectParam.TextFormat);
        }
        void InitScript()
        {
            if (subscriptSuperscriptRenderer == null)
                subscriptSuperscriptRenderer = new SubscriptSuperscriptRenderer();
            subscriptSuperscriptRenderer.DrawingSession = this.EffectParam.DrawingSession;
            subscriptSuperscriptRenderer.TextBrush = new CanvasSolidColorBrush(this.EffectParam.DrawingSession, this.EffectParam.TextColor);

            this.EffectParam.NewTextLayout.DrawToTextRenderer(subscriptSuperscriptRenderer, new System.Numerics.Vector2(0, 0));
        }
        #endregion

    }
}
