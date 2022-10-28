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
    public class SuperSubscipt : ITextEffect
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
            _textFormat = textFormat;
            sampleText = newText;
            textBrush = new CanvasSolidColorBrush(ds, textColor);
            resourceRealizationSize = textLayout.RequestedSize;
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
            CurrentTextSampleOption = TextSampleOption.ChemicalFormula;
        }
        CanvasTextFormat _textFormat;
        CanvasTextLayout textLayout;
        CanvasSolidColorBrush textBrush;
        string sampleText = "";

        public float CurrentFontSize { get; set; }
        public bool UseBoldFace { get; set; }
        public bool UseItalicFace { get; set; }
        public bool ShowUnformatted { get; set; }

        public enum TextSampleOption
        {
            ChemicalFormula,
            RightTriangle,
            ShortExpression
        }
        public List<TextSampleOption> TextSampleOptions { get { return Utils.GetEnumAsList<TextSampleOption>(); } }
        public TextSampleOption CurrentTextSampleOption { get; set; }

        //
        // When implementing an actual subscript/superscript typography option,
        // a font author will fine-tune the placement of baselines based on how the font
        // itself looks and is measured. Adjusting these values may look better
        // on some fonts.
        // We picked some reasonable default choices for these.
        //
        const float fontSizeShrinkAmount = 0.65f;
        const float subscriptBaselineScale = 0.2f;
        const float superscriptBaselineScale = 0.7f;

        bool needsResourceRecreationb= true;
        Size resourceRealizationSize;
        float sizeDim;
        bool defaultFontSizeSet;
        void EnsureResources(ICanvasResourceCreatorWithDpi resourceCreator, Size targetSize)
        {
            
            float canvasWidth = (float)targetSize.Width;
            float canvasHeight = (float)targetSize.Height;
            sizeDim = Math.Min(canvasWidth, canvasHeight);

            textBrush = new CanvasSolidColorBrush(resourceCreator, Colors.Thistle);

            if (!defaultFontSizeSet)
            {
                CurrentFontSize = sizeDim / 20;
                
                defaultFontSizeSet = true;
            }

            
            switch (CurrentTextSampleOption)
            {
                case TextSampleOption.ChemicalFormula:
                    sampleText =
                        "H2O is the chemical formula for water.\r\n\r\n" +
                        "And, the isotope Carbon-12 may be written as 12C.\r\n\r\n" +
                        "Often, chemical formulas make use of both superscript and subscript text.";
                    break;
                case TextSampleOption.RightTriangle:
                    sampleText =
                        "The side lengths of a right-angle triangle can be written as a2 + b2 = c2.\r\n\r\n" +
                        "If the triangle's shorter sides are lengths 3 and 4, the remaining side must be 5, since 32 + 42 = 52.";
                    break;
                case TextSampleOption.ShortExpression:
                    sampleText = "ax2by3";
                    break;
                default:
                    Debug.Assert(false, "Unexpected text sample option");
                    break;
            }

           
            ApplyToTextLayout(sampleText, resourceCreator, canvasWidth, canvasHeight);

            switch (CurrentTextSampleOption)
            {
                case TextSampleOption.ChemicalFormula:
                    SetSubscript(sampleText.IndexOf("H2O") + 1, 1);
                    SetSuperscript( sampleText.IndexOf("12C"), 2);
                    SetSubscript(sampleText.IndexOf("subscript"), "subscript".Length);
                    SetSuperscript(sampleText.IndexOf("superscript"), "superscript".Length);
                    break;
                case TextSampleOption.RightTriangle:
                    for (int i = 0; i < sampleText.Length; ++i)
                    {
                        if (sampleText[i] == '2')
                            SetSuperscript(i, 1);
                    }
                    break;
                case TextSampleOption.ShortExpression:
                    SetSubscript( 1, 1);
                    SetSuperscript( 2, 1);
                    SetSubscript(4, 1);
                    SetSuperscript(5, 1);
                    break;
                default:
                    Debug.Assert(false, "Unexpected text sample option");
                    break;
            }

            subscriptSuperscriptRenderer = new SubscriptSuperscriptRenderer();
            resourceRealizationSize = targetSize;
        }

        //
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
        class CustomBrushData
        {
            public enum BaselineAdjustmentType { None, Raise, Lower }
            public BaselineAdjustmentType BaselineAdjustment;
        }

        private void ShrinkFontAndAttachCustomBrushData(
            CanvasTextLayout textLayout,
            int textPosition,
            int characterCount,
            CustomBrushData.BaselineAdjustmentType baselineAdjustmentType)
        {
            textLayout.SetFontSize(textPosition, characterCount, (float)CurrentFontSize * fontSizeShrinkAmount);
            textLayout.SetCustomBrush(textPosition, characterCount, new CustomBrushData() { BaselineAdjustment = baselineAdjustmentType });
        }

        public void SetSubscript(int textPosition, int characterCount)
        {
            ShrinkFontAndAttachCustomBrushData(textLayout, textPosition, characterCount, CustomBrushData.BaselineAdjustmentType.Lower);
        }

        public void SetSuperscript(int textPosition, int characterCount)
        {
            ShrinkFontAndAttachCustomBrushData(textLayout, textPosition, characterCount, CustomBrushData.BaselineAdjustmentType.Raise);
        }

        SubscriptSuperscriptRenderer subscriptSuperscriptRenderer = null;

        // 
        // There's a limitation to this approach of shrinking text/adjusting baselines, 
        // worth calling out. If there's an entire line of just subscript or an entire line
        // of just superscript, the overall line height will be short compared to full-size
        // text. Admittedly, this is a rather contrived and rare situation; it can be fixed by
        // inserting invisible full-size whitespace characters into your text. This approach
        // works for a vast majority of cases.
        //
        class SubscriptSuperscriptRenderer : ICanvasTextRenderer
        {
            public CanvasDrawingSession DrawingSession;
            public CanvasSolidColorBrush TextBrush;

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
                int[] clusterMapIndices,
                uint startingTextPosition,
                CanvasGlyphOrientation glyphOrientation)
            {
                if (glyphs == null || glyphs.Length == 0)
                    return;

                float scaledFontAscent = fontFace.Ascent * fontSize;
                float subscriptBaselineDropAmount = scaledFontAscent * subscriptBaselineScale;
                float superscriptBaselineRaiseAmount = scaledFontAscent * superscriptBaselineScale;

                // Draw glyph-by-glyph.
                for (int i = 0; i < glyphs.Length; ++i)
                {
                    CanvasGlyph[] singleGlyph = new CanvasGlyph[1];
                    singleGlyph[0] = glyphs[i];

                    Vector2 positionForThisGlyph = position;

                    CustomBrushData brushData = (CustomBrushData)brush;
                    if (brushData != null)
                    {
                        if (brushData.BaselineAdjustment == CustomBrushData.BaselineAdjustmentType.Lower)
                        {
                            positionForThisGlyph.Y += subscriptBaselineDropAmount;
                        }
                        else if (brushData.BaselineAdjustment == CustomBrushData.BaselineAdjustmentType.Raise)
                        {
                            positionForThisGlyph.Y -= superscriptBaselineRaiseAmount;
                        }
                    }

                    DrawingSession.DrawGlyphRun(
                        positionForThisGlyph,
                        fontFace,
                        fontSize,
                        singleGlyph,
                        isSideways,
                        bidiLevel,
                        TextBrush);

                    position.X += glyphs[i].Advance;
                }
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
                // Normally, we'd add strikethrough support here. Strikethrough isn't used by this demo.
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
                // Normally, we'd add underline support here. Underline isn't used by this demo.
            }

            public void DrawInlineObject(
                Vector2 baselineOrigin,
                ICanvasTextInlineObject inlineObject,
                bool isSideways,
                bool isRightToLeft,
                object brush,
                CanvasGlyphOrientation glyphOrientation)
            {
                // This sample doesn't use inline objects.
            }

            public float Dpi { get { return 96; } }

            public bool PixelSnappingDisabled { get { return false; } }

            public Matrix3x2 Transform { get { return System.Numerics.Matrix3x2.Identity; } }
        }


        private void ApplyToTextLayout(string sampleText, ICanvasResourceCreator resourceCreator, float canvasWidth, float canvasHeight)
        {
            CanvasTextFormat textFormat = new CanvasTextFormat()
            {
                FontFamily = _textFormat.FontFamily,
                FontSize = _textFormat.FontSize,
                WordWrapping = _textFormat.WordWrapping,
               
                FontWeight = UseBoldFace ? FontWeights.Bold : FontWeights.Normal,
                FontStyle = UseItalicFace ? Windows.UI.Text.FontStyle.Italic : Windows.UI.Text.FontStyle.Normal
            };

           
        }

        private void Canvas_Draw(CanvasDrawingSession sender, CanvasAnimatedDrawEventArgs args)
        {
            EnsureResources(sender, textLayout.RequestedSize);

            if (ShowUnformatted)
            {
                args.DrawingSession.DrawTextLayout(textLayout, 0, 0, Colors.DarkGray);
            }

            subscriptSuperscriptRenderer.DrawingSession = args.DrawingSession;
            subscriptSuperscriptRenderer.TextBrush = textBrush;

            textLayout.DrawToTextRenderer(subscriptSuperscriptRenderer, new System.Numerics.Vector2(0, 0));
        }

       
       
    }
}
