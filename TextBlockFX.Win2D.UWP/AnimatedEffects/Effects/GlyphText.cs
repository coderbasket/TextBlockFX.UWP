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
    public class GlyphTextAnimated : ITextEffectAnimated
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
        public Color TextBrush { get; set; } = Colors.LightBlue;
        public void SetTextBoundary(params string[] words)
        {
            if (words.Length == 0)
                return;

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
            CanvasDrawingSession _drawingSession,
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
            everyOtherWordBoundary = Utils.GetEveryOtherWord(newText);
            EnsureResources(ds, newTextLayout.RequestedSize);
            if (state == RedrawState.Idle)
            {
                Canvas_Draw(ds, args);
                return;
            }
            Canvas_Draw(ds, args);
        }
        public GlyphTextAnimated()
        {
            CurrentTextEffectOption = TextEffectOption.UnderlineEveryOtherWord;
            CurrentTextDirection = CanvasTextDirection.LeftToRightThenTopToBottom;
            CurrentVerticalGlyphOrientation = CanvasVerticalGlyphOrientation.Default;

            ShowNonCustomText = true;
        }
        //Engine
       
        CanvasGeometry textReference;
        List<WordBoundary> everyOtherWordBoundary;
        CanvasSolidColorBrush textBrush;
       
        public bool ShowNonCustomText { get; set; }

        bool needsResourceRecreation = true;

        public List<CanvasTextDirection> TextDirectionOptions { get { return Utils.GetEnumAsList<CanvasTextDirection>(); } }
        public CanvasTextDirection CurrentTextDirection { get; set; }

        public List<CanvasVerticalGlyphOrientation> VerticalGlyphOrientationOptions { get { return Utils.GetEnumAsList<CanvasVerticalGlyphOrientation>(); } }
        public CanvasVerticalGlyphOrientation CurrentVerticalGlyphOrientation { get; set; }

        public enum TextEffectOption { None, UnderlineEveryOtherWord, StrikeEveryOtherWord };
        public List<TextEffectOption> TextEffectOptions { get { return Utils.GetEnumAsList<TextEffectOption>(); } }
        public TextEffectOption CurrentTextEffectOption { get; set; }

        

        void EnsureResources(ICanvasResourceCreatorWithDpi resourceCreator, Size targetSize)
        {
            if (!needsResourceRecreation)
                return;

           ApplyToTextLayout(resourceCreator, (float)targetSize.Width, (float)targetSize.Height);

            textReference = CanvasGeometry.CreateText(this.EffectParam.NewTextLayout);

            textBrush = new CanvasSolidColorBrush(resourceCreator, TextBrush);
        }

        private void ApplyToTextLayout(ICanvasResourceCreator resourceCreator, float canvasWidth, float canvasHeight)
        {
            float sizeDim = Math.Min(canvasWidth, canvasHeight);

            

            //CanvasTextLayout textLayout = new CanvasTextLayout(resourceCreator, testString, textFormat, canvasWidth, canvasHeight);

            if (CurrentTextEffectOption == TextEffectOption.UnderlineEveryOtherWord)
            {
                foreach (WordBoundary wb in everyOtherWordBoundary)
                {
                    this.EffectParam.NewTextLayout.SetUnderline(wb.Start, wb.Length, true);
                }
            }
            else if (CurrentTextEffectOption == TextEffectOption.StrikeEveryOtherWord)
            {
                foreach (WordBoundary wb in everyOtherWordBoundary)
                {
                    this.EffectParam.NewTextLayout.SetStrikethrough(wb.Start, wb.Length, true);
                }
            }

            this.EffectParam.NewTextLayout.VerticalGlyphOrientation = CurrentVerticalGlyphOrientation;

           
        }
        static CanvasDrawingSession drawingSession;
        private void Canvas_Draw(CanvasDrawingSession senser, CanvasAnimatedDrawEventArgs args)
        {
            EnsureResources(senser, this.EffectParam.NewTextLayout.RequestedSize);

            drawingSession = args.DrawingSession;

            CustomTextRenderer textRenderer = new CustomTextRenderer(textBrush, drawingSession);
            this.EffectParam.NewTextLayout.DrawToTextRenderer(textRenderer, 0, 0);

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
