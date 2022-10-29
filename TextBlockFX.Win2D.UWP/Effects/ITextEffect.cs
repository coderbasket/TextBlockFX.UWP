using System;
using System.Collections.Generic;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Windows.UI;
using Microsoft.Graphics.Canvas;

#if WINDOWS
namespace TextBlockFX.Win2D.WinUI
#else
namespace TextBlockFX.Win2D.UWP
#endif
{
    public interface ITextEffect : ITextEffectBase
    {
        
        object Sender { get; set; }
        void DrawText(EffectParam effectParam);
      
    }
    
}
