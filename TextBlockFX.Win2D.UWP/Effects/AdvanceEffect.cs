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
using System.Drawing;
using Color = Windows.UI.Color;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.UI;
using Size = Windows.Foundation.Size;
using static TextBlockFX.Win2D.UWP.Effects.OutlineText;
using Microsoft.Graphics.Canvas.Geometry;

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
                if (fx.SubScripts?.Count > 0)
                {
                    foreach (var sb in fx.SubScripts)
                    {
                        SetSubscript(this.EffectParam.NewText.IndexOf(sb.Words), sb.Length);
                    }
                }
                if (fx.SuperScripts?.Count > 0)
                {
                    foreach (var sp in fx.SuperScripts)
                    {
                        SetSuperscript(this.EffectParam.NewText.IndexOf(sp.Words), sp.Length);
                    }
                }

            }
            process = false;
        }



        bool ShowUnformatted;
        private void Canvas_Draw(CanvasDrawingSession sender, CanvasAnimatedDrawEventArgs args)
        {
            EnsureResources(sender);
           
            foreach(var action in actionOrders.OrderBy(p=>p.Order).ToList())
            {
                action.Action.Invoke();
            }
            if (ShowUnformatted)
            {
                args.DrawingSession.DrawTextLayout(this.EffectParam.NewTextLayout, 0, 0, Colors.DarkGray);
            }
            args.DrawingSession.DrawTextLayout(this.EffectParam.NewTextLayout, 0, 0, Colors.Transparent);
            InitScript();
           

        }
        List<ActionOrder> actionOrders= new List<ActionOrder>();
        #region GlowEffect
        bool applyGlowEffect = false;
        Color _glowColor = Colors.Yellow;
        float _amount = 40;
        public AdvanceEffect ApplyGlowEffect(Windows.UI.Color glowColor, float amount = 40)
        {
            _glowColor = glowColor;
            _amount = amount;
            applyGlowEffect = true;
            var number = 0;
            if (actionOrders.Count > 0)
            {
                number = actionOrders.LastOrDefault().Order;
                number++;
            }
            actionOrders.Add(new ActionOrder() { Order= number, Action = ApplyGlowEffect, });
            return this;
        }
        void ApplyGlowEffect()
        {
            float offset = 0;
            using (var textCommandList = new CanvasCommandList(this.EffectParam.DrawingSession))
            {
                using (var textDs = textCommandList.CreateDrawingSession())
                {
                    textDs.DrawTextLayout(this.EffectParam.NewTextLayout, 0, 0, _glowColor);
                }

                GlowEffectGraph glowEffectGraph = new GlowEffectGraph();
                glowEffectGraph.Setup(textCommandList, _amount);
                this.EffectParam.DrawingSession.DrawImage(glowEffectGraph.Output, offset, offset);
            }
           
        }
        #endregion

        #region Outline
        CanvasStrokeStyle dashedStroke = new CanvasStrokeStyle()
        {
            DashStyle = CanvasDashStyle.Solid,
        };
        Color _outlineColor = Colors.Navy;
        bool isOutline = false;
        public AdvanceEffect ApplyOutlineEffect(Windows.UI.Color outlineColor, CanvasStrokeStyle style = null)
        {
            if (style != null)
            {
                dashedStroke = style;
            }
            _outlineColor = outlineColor;
            isOutline = true;
            var number = 0;
            if (actionOrders.Count > 0)
            {
                number = actionOrders.LastOrDefault().Order;
                number++;
            }
            actionOrders.Add(new ActionOrder() { Order = number, Action = ApplyOutlined, });
            return this;
        }

        void ApplyOutlined()
        {
            GlyphRunsToGeometryConverter converter = new GlyphRunsToGeometryConverter(this.EffectParam.DrawingSession);

            this.EffectParam.NewTextLayout.DrawToTextRenderer(converter, 0, 0);

            var textGeometry = converter.GetGeometry();
            float strokeWidth = 15.0f;

            this.EffectParam.DrawingSession.DrawGeometry(textGeometry, Colors.Blue, strokeWidth, dashedStroke);

        }
        #endregion

        #region Glyph
        Color _glyphColor = Colors.ForestGreen;
        Color semitrans = Colors.White;
        public AdvanceEffect ApplyGlyphEffect(Color glyphColor, Color semiTransColor)
        {
            _glyphColor = glyphColor;
            semitrans = semiTransColor;
            var number = 0;
            if (actionOrders.Count > 0)
            {
                number = actionOrders.LastOrDefault().Order;
                number++;
            }
            actionOrders.Add(new ActionOrder() { Order = number, Action = ApplyGlyph, });
            return this;
        }
        void ApplyGlyph()
        {
            var brush = new CanvasSolidColorBrush(this.EffectParam.DrawingSession, _glyphColor);
            CustomTextRenderer textRenderer = new CustomTextRenderer(brush, this.EffectParam.DrawingSession);
            this.EffectParam.NewTextLayout.DrawToTextRenderer(textRenderer, 0, 0);
            semitrans.A = 127;
            float strokeWidth = 22.0f;//5.0f;
            var textReference = CanvasGeometry.CreateText(this.EffectParam.NewTextLayout);
            this.EffectParam.DrawingSession.DrawGeometry(textReference, semitrans, strokeWidth);
        } 
        #endregion
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
    public class ActionOrder
    {
        public int Order { get; set; }
        public Action Action { get; set; }
    }
}
