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
            if (state == RedrawState.Idle)
            {

                Canvas_Draw(args.DrawingSession, newTextLayout.RequestedSize, args);
                return;
            }
            Canvas_Draw(args.DrawingSession, newTextLayout.RequestedSize, args);
        }
        //Engine

        CanvasGeometry textGeometry;
        CanvasStrokeStyle dashedStroke = new CanvasStrokeStyle()
        {
            DashStyle = CanvasDashStyle.Dash
        };
       
        List<WordBoundary> everyOtherWordBoundary;

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
        void EnsureResources(ICanvasResourceCreatorWithDpi resourceCreator, Size targetSize)
        {
            if (!needsResourceRecreation)
                return;

            ApplyToTextLayout(resourceCreator, (float)targetSize.Width, (float)targetSize.Height);

            if (CurrentTextOutlineGranularityOption == TextOutlineGranularity.Layout)
            {
                textGeometry = CanvasGeometry.CreateText(this.EffectParam.NewTextLayout);
            }
            else
            {
                GlyphRunsToGeometryConverter converter = new GlyphRunsToGeometryConverter(resourceCreator);

                this.EffectParam.NewTextLayout.DrawToTextRenderer(converter, 0, 0);

                textGeometry = converter.GetGeometry();
            }
        }

        private void ApplyToTextLayout(ICanvasResourceCreator resourceCreator, float canvasWidth, float canvasHeight)
        {
            
            everyOtherWordBoundary = Utils.GetEveryOtherWord(this.EffectParam.NewText);

            float sizeDim = Math.Min(canvasWidth, canvasHeight);

           
            if (CurrentTextEffectOption == TextEffectOption.UnderlineEveryOtherWord)
            {
                foreach (var wb in everyOtherWordBoundary)
                {
                    this.EffectParam.NewTextLayout.SetUnderline(wb.Start, wb.Length, true);
                }
            }
            else if (CurrentTextEffectOption == TextEffectOption.StrikeEveryOtherWord)
            {
                foreach (var wb in everyOtherWordBoundary)
                {
                    this.EffectParam.NewTextLayout.SetStrikethrough(wb.Start, wb.Length, true);
                }
            }

            this.EffectParam.NewTextLayout.TrimmingGranularity = CanvasTextTrimmingGranularity.Character;
            this.EffectParam.NewTextLayout.TrimmingSign = CanvasTrimmingSign.Ellipsis;

            this.EffectParam.NewTextLayout.VerticalGlyphOrientation = CurrentVerticalGlyphOrientation;

            
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
                args.DrawingSession.DrawTextLayout(this.EffectParam.NewTextLayout, 0, 0, semitrans);
            }
        }
        
    }
    
}
