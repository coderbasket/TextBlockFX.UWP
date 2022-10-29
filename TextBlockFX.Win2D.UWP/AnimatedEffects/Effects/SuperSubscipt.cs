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

#if WINDOWS
namespace TextBlockFX.Win2D.WinUI.Effects
#else
namespace TextBlockFX.Win2D.UWP.Effects
#endif
{
    public class SuperSubscipt : ITextEffectAnimated
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
            resourceRealizationSize = this.EffectParam.NewTextLayout.RequestedSize;
            EnsureResources(ds, resourceRealizationSize);
            if (state == RedrawState.Idle)
            {

                Canvas_Draw(ds, args);
                return;
            }
            Canvas_Draw(ds, args);
        }
        //Engine
        public SuperSubscipt()
        {
            
        }
        
       
       

        //public float CurrentFontSize { get; set; }
        //public bool UseBoldFace { get; set; }
        //public bool UseItalicFace { get; set; }
        //public bool ShowUnformatted { get; set; }

        

        //
        // When implementing an actual subscript/superscript typography option,
        // a font author will fine-tune the placement of baselines based on how the font
        // itself looks and is measured. Adjusting these values may look better
        // on some fonts.
        // We picked some reasonable default choices for these.

        bool needsResourceRecreationb= true;
        Size resourceRealizationSize;
        float sizeDim;
        void EnsureResources(ICanvasResourceCreatorWithDpi resourceCreator, Size targetSize)
        {
            
            float canvasWidth = (float)targetSize.Width;
            float canvasHeight = (float)targetSize.Height;
            sizeDim = Math.Min(canvasWidth, canvasHeight);

            var textBrush = new CanvasSolidColorBrush(resourceCreator, Colors.Thistle);

            //sample
            SetSubscript(this.EffectParam.NewText.IndexOf("Fly") + 1, 1);
            SetSuperscript(this.EffectParam.NewText.IndexOf("see"), 2);
            SetSubscript(this.EffectParam.NewText.IndexOf("let"), "let".Length);
            SetSuperscript(this.EffectParam.NewText.IndexOf("words"), "words".Length);

           
            resourceRealizationSize = targetSize;
        }

       
        

        bool ShowUnformatted;
        private void Canvas_Draw(CanvasDrawingSession sender, CanvasAnimatedDrawEventArgs args)
        {
            EnsureResources(sender, this.EffectParam.NewTextLayout.RequestedSize);

            if (ShowUnformatted)
            {
                args.DrawingSession.DrawTextLayout(this.EffectParam.NewTextLayout, 0, 0, Colors.DarkGray);
            }
            InitScript(args);
           
        }


        #region Script
        // We need some means of telling the text layout which characters
        // are subscript and which are superscript; keying off the font size
        // isn't enough, if we want to be able to mix subscript and superscript
        // together.
        //
        // CustomBrushData is a piece of metadata we can attach to characters
        // which describes whether they're superscript or subscript. Since it's all 
        // handled by SetCustomBrush, we don't need to buffer any data, do any thinking
        // about character ranges, or optimize things for where formatting changes are.
        //
        // SetCustomBrush is a really flexible way of attaching some supplementary
        // drawing data to individual characters, and here it's a way of informing
        // SubscriptSuperscriptRenderer how to adjust baselines.
        //
        // If you'd prefer not to use CustomBrushData, or for some reason you need
        // to set the custom brush to something else, it'd also work to store 
        // superscript/subscript info in the text renderer itself.
        // 


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
        void InitScript(CanvasAnimatedDrawEventArgs args)
        {
            if (subscriptSuperscriptRenderer == null)
                subscriptSuperscriptRenderer = new SubscriptSuperscriptRenderer();
            subscriptSuperscriptRenderer.DrawingSession = args.DrawingSession;
            subscriptSuperscriptRenderer.TextBrush = new CanvasSolidColorBrush(this.EffectParam.DrawingSession, this.EffectParam.TextColor);

            this.EffectParam.NewTextLayout.DrawToTextRenderer(subscriptSuperscriptRenderer, new System.Numerics.Vector2(0, 0));
        }


        // 
        // There's a limitation to this approach of shrinking text/adjusting baselines, 
        // worth calling out. If there's an entire line of just subscript or an entire line
        // of just superscript, the overall line height will be short compared to full-size
        // text. Admittedly, this is a rather contrived and rare situation; it can be fixed by
        // inserting invisible full-size whitespace characters into your text. This approach
        // works for a vast majority of cases.
        //

        #endregion

    }
}
