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

namespace TextBlockFX.Win2D.UWP
{
    public class TextEffectParam
    {
        public string OldText { get; }
        public string NewText { get; }
        public List<TextDiffResult> DiffResults { get; }
        public CanvasTextLayout OldTextLayout { get; }
        public CanvasTextLayout NewTextLayout { get; }
        public CanvasTextFormat TextFormat { get; }
        public Color TextColor { get; }
        public CanvasLinearGradientBrush GradientBrush { get; }
        public RedrawState State { get; }
        public CanvasDrawingSession DrawingSession { get; }
        public CanvasAnimatedDrawEventArgs Arg { get; }
        public TextEffectParam()
        {

        }
        public TextEffectParam(string oldText,
            string newText,
            List<TextDiffResult> texts,
            CanvasTextLayout oldTxtLayout,
            CanvasTextLayout newTextLayout,
            CanvasTextFormat textFormat,
            Color textColor,
            CanvasLinearGradientBrush gradientBrush,
            RedrawState state,
            CanvasDrawingSession drawingSession,
             CanvasAnimatedDrawEventArgs arg)
        {
            this.OldText = oldText;
            this.NewText = newText;
            this.DiffResults = texts;
            this.OldTextLayout = oldTxtLayout;
            this.NewTextLayout = newTextLayout;
            this.TextFormat = textFormat;
            this.TextColor = textColor;
            this.GradientBrush = gradientBrush;
            this.State = state;
            this.DrawingSession = drawingSession;
            this.Arg = arg;
        }
    }
}
