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
using Microsoft.Graphics.Canvas.Geometry;
using Windows.UI.Xaml.Media;
using static TextBlockFX.Win2D.UWP.Effects.OutlineTextAnimated;

#if WINDOWS
namespace TextBlockFX.Win2D.WinUI.Effects
#else
namespace TextBlockFX.Win2D.UWP.Effects
#endif
{
    public class AdvanceEffect : ITextEffect
    {
        List<ActionOrder> actionOrders = new List<ActionOrder>();
        public object Sender { get; set; }
        EffectParam Tf;
        public AdvanceEffect()
        {

        }
        
        public void DrawText(EffectParam effectParam)
        {
            this.Tf = effectParam;
            Draw();
            
        }
        void Draw()
        {
            foreach (var action in actionOrders.OrderBy(p => p.Order).ToList())
            {
                action.Action.Invoke();
            }
            var session = this.Tf.Args.DrawingSession;                        
            session.DrawTextLayout(this.Tf.TextLayout, 0,0, this.Tf.TextColor);
            SetWordsBoundary();
            //actionOrders.Clear();
            
        }
        #region GlowEffect
        Color _glowColor = Colors.Yellow;
        const float _amountC = 30;
        float _amount = _amountC;
        bool applyGlowEffect = false;
        public AdvanceEffect ApplyGlowEffect(Windows.UI.Color glowColor, float amount = _amountC)
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
            actionOrders.Add(new ActionOrder() { Order = number, Action = ApplyGlowEffect, });
            return this;
        }
        void ApplyGlowEffect()
        {
            float offset = 0;
            using (var textCommandList = new CanvasCommandList(this.Tf.Args.DrawingSession))
            {
                using (var textDs = textCommandList.CreateDrawingSession())
                {
                    textDs.DrawTextLayout(this.Tf.TextLayout, 0, 0, _glowColor);
                }

                GlowEffectGraph glowEffectGraph = new GlowEffectGraph();
                glowEffectGraph.Setup(textCommandList, _amount);
                this.Tf.Args.DrawingSession.DrawImage(glowEffectGraph.Output, offset, offset);
            }

        }
        #endregion
        #region Outline
        CanvasStrokeStyle dashedStroke = new CanvasStrokeStyle()
        {
            DashStyle = CanvasDashStyle.Solid,
        };
        Color _outlineColor = Colors.Navy;
        public AdvanceEffect ApplyOutlineEffect(Windows.UI.Color outlineColor, CanvasStrokeStyle style = null)
        {
            if (style != null)
            {
                dashedStroke = style;
            }
            _outlineColor = outlineColor;
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
            GlyphRunsToGeometryConverter converter = new GlyphRunsToGeometryConverter(this.Tf.Args.DrawingSession);

            this.Tf.TextLayout.DrawToTextRenderer(converter, 0, 0);

            var textGeometry = converter.GetGeometry();
            float strokeWidth = 15.0f;

            this.Tf.Args.DrawingSession.DrawGeometry(textGeometry, _outlineColor, strokeWidth, dashedStroke);

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
            var brush = new CanvasSolidColorBrush(this.Tf.Args.DrawingSession, _glyphColor);
            CustomTextRenderer textRenderer = new CustomTextRenderer(brush, this.Tf.Args.DrawingSession);
            this.Tf.TextLayout.DrawToTextRenderer(textRenderer, 0, 0);
            semitrans.A = 127;
            float strokeWidth = 22.0f;//5.0f;
            var textReference = CanvasGeometry.CreateText(this.Tf.TextLayout);
            this.Tf.Args.DrawingSession.DrawGeometry(textReference, semitrans, strokeWidth);
        }


        #endregion
        Color _undelineColor = Colors.OrangeRed;
        bool u_colorSet = false;
        public AdvanceEffect SetUndelineColor(Color undelineColor)
        {
            _undelineColor = undelineColor;
            u_colorSet = true;
            return this;
        }
        void SetWordsBoundary()
        {
            if (Sender == null)
                return;
            if(Sender is TextBlockFX fx)
            {
                if(fx.UnderlineWords?.Count> 0)
                {
                    foreach (var wb in fx.UnderlineWords)
                    {
                        var startIndex = this.Tf.Text.IndexOf(wb.Words);
                        this.Tf.TextLayout.SetUnderline(startIndex, wb.Words.Length, true);
                    }
                    //var textGeometry = CanvasGeometry.CreateText(this.Tf.TextLayout);
                    //GlyphRunsToGeometryConverter converter = new GlyphRunsToGeometryConverter(Tf.Canvas);
                    //textGeometry = converter.GetGeometry();
                    //this.Tf.TextLayout.DrawToTextRenderer(converter, 0, 0);
                    //float strokeWidth = 15.0f;

                    //Tf.Args.DrawingSession.DrawGeometry(textGeometry, Colors.ForestGreen, strokeWidth, dashedStroke);
                    Color semitrans = Tf.TextColor;
                    if (u_colorSet)
                        semitrans = _undelineColor;
                    semitrans.A = 127;
                    Tf.Args.DrawingSession.DrawTextLayout(this.Tf.TextLayout, 0, 0, semitrans);
                   
                }
                
            }
        }
        bool process = false;
       
    }

}
