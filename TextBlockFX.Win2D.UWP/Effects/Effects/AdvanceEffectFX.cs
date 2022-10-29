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
using Windows.UI.Xaml.Media;

#if WINDOWS
namespace TextBlockFX.Win2D.WinUI.Effects
#else
namespace TextBlockFX.Win2D.UWP.Effects
#endif
{
    public class AdvanceEffectFX : ITextEffect
    {
        public object Sender { get; set; }
        EffectParam Tf;
        public AdvanceEffectFX()
        {

        }        
        public void DrawText(EffectParam effectParam)
        {
            this.Tf = effectParam;
            Draw();
            
        }
        void Draw()
        {
            var session = this.Tf.Args.DrawingSession;                        
            session.DrawTextLayout(this.Tf.TextLayout, 0,0, this.Tf.TextColor);          
        }
    }
   
}
