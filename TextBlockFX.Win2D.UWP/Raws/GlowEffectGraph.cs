using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextBlockFX.Win2D.UWP
{
    public class GlowEffectGraph
    {
        public ICanvasImage Output
        {
            get
            {
                return blur;
            }
        }

        MorphologyEffect morphology = new MorphologyEffect()
        {
            Mode = MorphologyEffectMode.Dilate,
            Width = 1,
            Height = 1
        };

        GaussianBlurEffect blur = new GaussianBlurEffect()
        {
            BlurAmount = 0,
            BorderMode = EffectBorderMode.Soft
        };

        public GlowEffectGraph()
        {
            blur.Source = morphology;
        }

        public void Setup(ICanvasImage source, float amount)
        {
            morphology.Source = source;

            var halfAmount = Math.Min(amount / 2, 100);
            morphology.Width = (int)Math.Ceiling(halfAmount);
            morphology.Height = (int)Math.Ceiling(halfAmount);
            blur.BlurAmount = halfAmount;
        }

    }
}
