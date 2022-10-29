using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace TextBlockFX.Win2D.UWP
{
    public class EffectParam
    {
        public CanvasTextLayout TextLayout { get; }
        public CanvasTextFormat TextFormat { get; }
        public Color TextColor { get; }
        public CanvasControl Canvas { get; }
        public CanvasDrawEventArgs Args { get; }
        public string Text { get; }
        public EffectParam(string text, Color textColor, CanvasTextFormat textFormat, CanvasTextLayout textLayout, CanvasControl canvas, CanvasDrawEventArgs args)
        { 
            this.Text = text;
            this.TextFormat = textFormat;
            this.TextColor = textColor;
            this.TextLayout = textLayout;
            this.Canvas = canvas;
            this.Args = args;
        }
    }
}
