using System;
using System.Collections.Generic;
using System.Numerics;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Windows.UI;

#if WINDOWS
namespace TextBlockFX.Win2D.WinUI.Effects
#else
namespace TextBlockFX.Win2D.UWP.Effects
#endif
{
    /// <summary>
    /// Built-in zoom effect of TextBlockFX
    /// </summary>
    public class ZoomAnimated : ITextEffectAnimated
    {
        public object Sender { get; set; }
        /// <inheritdoc />
        public TimeSpan AnimationDuration { get; set; } = TimeSpan.FromMilliseconds(800);

        /// <inheritdoc />
        public TimeSpan DelayPerCluster { get; set; } = TimeSpan.FromMilliseconds(10);

        /// <inheritdoc />
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
        /// <inheritdoc />
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
                DrawIdle(ds,
                    oldTextLayout,
                    newTextLayout,
                    textFormat,
                    textColor,
                    gradientBrush);

                return;
            }

            for (int i = 0; i < diffResults.Count; i++)
            {
                var diffResult = diffResults[i];

                switch (diffResult.Type)
                {
                    case DiffOperationType.Insert:
                        DrawInsert(ds,
                            diffResult.OldGlyphCluster,
                            diffResult.NewGlyphCluster,
                            oldTextLayout,
                            newTextLayout,
                            textFormat,
                            textColor,
                            gradientBrush);
                        break;
                    case DiffOperationType.Remove:
                        DrawRemove(ds,
                            diffResult.OldGlyphCluster,
                            diffResult.NewGlyphCluster,
                            oldTextLayout,
                            newTextLayout,
                            textFormat,
                            textColor,
                            gradientBrush);
                        break;
                    case DiffOperationType.Stay:
                    case DiffOperationType.Move:
                        DrawMove(ds,
                            diffResult.OldGlyphCluster,
                            diffResult.NewGlyphCluster,
                            oldTextLayout,
                            newTextLayout,
                            textFormat,
                            textColor,
                            gradientBrush);
                        break;
                    case DiffOperationType.Update:
                        DrawUpdate(ds,
                            diffResult.OldGlyphCluster,
                            diffResult.NewGlyphCluster,
                            oldTextLayout,
                            newTextLayout,
                            textFormat,
                            textColor,
                            gradientBrush);
                        break;
                }
            }
        }

        private void DrawIdle(CanvasDrawingSession ds,
            CanvasTextLayout oldTextLayout,
            CanvasTextLayout newTextLayout,
            CanvasTextFormat textFormat,
            Color textColor,
            CanvasLinearGradientBrush gradientBrush)
        {
            ds.Transform = Matrix3x2.Identity;
            ds.DrawTextLayout(newTextLayout, 0, 0, textColor);
        }

        private void DrawInsert(CanvasDrawingSession ds,
            GraphemeCluster oldCluster,
            GraphemeCluster newCluster,
            CanvasTextLayout oldTextLayout,
            CanvasTextLayout newTextLayout,
            CanvasTextFormat textFormat,
            Color textColor,
            CanvasLinearGradientBrush gradientBrush)
        {
            if (newCluster == null)
            {
                return;
            }

            float opacityProgress = Easing.UpdateProgress(newCluster.Progress, Easing.EasingFunction.ElasticOut);
            float zoomProgress = Easing.UpdateProgress(newCluster.Progress, Easing.EasingFunction.ElasticOut);
            using (ds.CreateLayer(opacityProgress))
            {
                ds.Transform = Matrix3x2.CreateScale(zoomProgress,
                new Vector2((float)(newCluster.LayoutBounds.X +
                                    newCluster.LayoutBounds.Width * 0.5),
                                    (float)(newCluster.LayoutBounds.Y +
                                    newCluster.LayoutBounds.Height * 0.5)));

                ds.DrawText(
                    newCluster.IsTrimmed
                        ? newTextLayout.GenerateTrimmingSign()
                        : newCluster.Characters,
                    (float)newCluster.DrawBounds.X,
                    (float)newCluster.DrawBounds.Y,
                    textColor,
                    textFormat);

                ds.Transform = Matrix3x2.Identity;
            }
        }

        private void DrawMove(CanvasDrawingSession ds,
            GraphemeCluster oldCluster,
            GraphemeCluster newCluster,
            CanvasTextLayout oldTextLayout,
            CanvasTextLayout newTextLayout,
            CanvasTextFormat textFormat,
            Color textColor,
            CanvasLinearGradientBrush gradientBrush)
        {
            if (oldCluster == null || newCluster == null)
            {
                return;
            }

            float oldProgress = Easing.UpdateProgress(oldCluster.Progress, Easing.EasingFunction.ElasticOut);

            var oX = oldCluster.DrawBounds.X;
            var oY = oldCluster.DrawBounds.Y;
            var nX = newCluster.DrawBounds.X;
            var nY = newCluster.DrawBounds.Y;

            var dX = nX - oX;
            var dY = nY - oY;

            ds.DrawText(
                oldCluster.IsTrimmed
                    ? oldTextLayout.GenerateTrimmingSign()
                    : oldCluster.Characters,
                (float)(oX + dX * oldProgress),
                (float)(oY + dY * oldProgress),
                textColor,
                textFormat);
        }

        private void DrawUpdate(CanvasDrawingSession ds,
            GraphemeCluster oldCluster,
            GraphemeCluster newCluster,
            CanvasTextLayout oldTextLayout,
            CanvasTextLayout newTextLayout,
            CanvasTextFormat textFormat,
            Color textColor,
            CanvasLinearGradientBrush gradientBrush)
        {
            if (oldCluster == null || newCluster == null)
            {
                return;
            }

            float oldOpacityProgress = Easing.UpdateProgress(1.0f - oldCluster.Progress, Easing.EasingFunction.ElasticIn);
            float oldZoomProgress = Easing.UpdateProgress(1.0f - oldCluster.Progress, Easing.EasingFunction.ElasticIn);
            float newOpacityProgress = Easing.UpdateProgress(newCluster.Progress, Easing.EasingFunction.ElasticOut);
            float newZoomProgress = Easing.UpdateProgress(newCluster.Progress, Easing.EasingFunction.ElasticOut);

            using (ds.CreateLayer(oldOpacityProgress))
            {
                ds.Transform = Matrix3x2.CreateScale(oldZoomProgress,
                    new Vector2((float)(oldCluster.LayoutBounds.X +
                                        oldCluster.LayoutBounds.Width * 0.5),
                        (float)(oldCluster.LayoutBounds.Y +
                                oldCluster.LayoutBounds.Height * 0.5)));

                ds.DrawText(
                    oldCluster.IsTrimmed
                        ? oldTextLayout.GenerateTrimmingSign()
                        : oldCluster.Characters,
                    (float)oldCluster.DrawBounds.X,
                    (float)oldCluster.DrawBounds.Y,
                    textColor,
                    textFormat);

                ds.Transform = Matrix3x2.Identity;
            }

            using (ds.CreateLayer(newOpacityProgress))
            {
                ds.Transform = Matrix3x2.CreateScale(newZoomProgress,
                    new Vector2((float)(newCluster.LayoutBounds.X +
                                        newCluster.LayoutBounds.Width * 0.5),
                        (float)(newCluster.LayoutBounds.Y +
                                newCluster.LayoutBounds.Height * 0.5)));

                ds.DrawText(
                    newCluster.IsTrimmed
                        ? newTextLayout.GenerateTrimmingSign()
                        : newCluster.Characters,
                    (float)newCluster.DrawBounds.X,
                    (float)newCluster.DrawBounds.Y,
                    textColor,
                    textFormat);

                ds.Transform = Matrix3x2.Identity;
            }
        }

        private void DrawRemove(CanvasDrawingSession ds,
            GraphemeCluster oldCluster,
            GraphemeCluster newCluster,
            CanvasTextLayout oldTextLayout,
            CanvasTextLayout newTextLayout,
            CanvasTextFormat textFormat,
            Color textColor,
            CanvasLinearGradientBrush gradientBrush)
        {
            if (oldCluster == null)
            {
                return;
            }

            float opacityProgress = Easing.UpdateProgress(1.0f - oldCluster.Progress, Easing.EasingFunction.ElasticIn);
            float zoomProgress = Easing.UpdateProgress(1.0f - oldCluster.Progress, Easing.EasingFunction.ElasticIn);
            using (ds.CreateLayer(opacityProgress))
            {
                ds.Transform = Matrix3x2.CreateScale(zoomProgress,
                    new Vector2((float)(oldCluster.LayoutBounds.X +
                                        oldCluster.LayoutBounds.Width * 0.5),
                        (float)(oldCluster.LayoutBounds.Y +
                                oldCluster.LayoutBounds.Height * 0.5)));
                ds.DrawText(
                    oldCluster.IsTrimmed
                        ? oldTextLayout.GenerateTrimmingSign()
                        : oldCluster.Characters,
                    (float)oldCluster.DrawBounds.X,
                    (float)oldCluster.DrawBounds.Y,
                    textColor,
                    textFormat);

                ds.Transform = Matrix3x2.Identity;
            }
        }

    }
}
