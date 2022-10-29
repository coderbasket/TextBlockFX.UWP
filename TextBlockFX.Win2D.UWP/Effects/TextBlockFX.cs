using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI.Core;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System.Threading.Tasks;

#if WINDOWS
using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
#else
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
#endif

#if WINDOWS
namespace TextBlockFX.Win2D.WinUI
#else
namespace TextBlockFX.Win2D.UWP
#endif
{
    /// <summary>
    /// A lightweight control for displaying small amounts of animated text.
    /// </summary>
    [TemplatePart(Name = "ContentBorder", Type = typeof(Border))]
    [TemplatePart(Name = "AnimatedCanvas", Type = typeof(CanvasControl))]
    public sealed class TextBlockFX : Control
    {
        private CanvasControl _animatedCanvas = null;
        private string _newText = string.Empty;
        private CanvasTextFormat _textFormat = new CanvasTextFormat();
        private CanvasLinearGradientBrush _textBrush;
        private Color _textColor = Colors.White;
        private CanvasTextLayout _newTextLayout;
        private ITextEffect _textEffect;

        private TextAlignment _textAlignment = TextAlignment.Left;
        private TextDirection _textDirection = TextDirection.LeftToRightThenTopToBottom;
        private TextTrimming _textTrimming = TextTrimming.WordEllipsis;
        private TextWrapping _textWrapping = TextWrapping.WrapWholeWords;

        #region Properties

        /// <summary>
        /// Identifies the Text dependency property.
        /// </summary>
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text", typeof(string), typeof(TextBlockFX), new PropertyMetadata(default(string)));

        /// <summary>
        /// Gets or sets the text contents of a TextBlockFX.
        /// </summary>
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set
            {
                _newText = value ?? string.Empty;
                SetValue(TextProperty, value);
                UpdateDrawing();
            }
        }

        /// <summary>
        /// Identifies the TextEffect dependency property.
        /// </summary>
        public static readonly DependencyProperty TextEffectProperty = DependencyProperty.Register(
            "TextEffect", typeof(ITextEffect), typeof(TextBlockFX), new PropertyMetadata(default(ITextEffect)));

        /// <summary>
        /// Gets or sets the effect for animating text.
        /// </summary>
        public ITextEffect TextEffect
        {
            get { return (ITextEffect)GetValue(TextEffectProperty); }
            set
            {
                _textEffect = value;
                _textEffect.Sender = this;
                SetValue(TextEffectProperty, value);
                UpdateDrawing();              
            }
        }

        /// <summary>
        /// Identifies the TextAlignment dependency property.
        /// </summary>
        public static readonly DependencyProperty TextAlignmentProperty = DependencyProperty.Register(
            "TextAlignment", typeof(TextAlignment), typeof(TextBlockFX), new PropertyMetadata(default(TextAlignment)));

        /// <summary>
        /// Gets or sets a value that indicates the horizontal alignment of text content.
        /// </summary>
        public TextAlignment TextAlignment
        {
            get { return (TextAlignment)GetValue(TextAlignmentProperty); }
            set
            {
                _textAlignment = value;
                SetValue(TextAlignmentProperty, value);
                UpdateDrawing();
            }
        }

        /// <summary>
        /// Identifies the TextDirection dependency property.
        /// </summary>
        public static readonly DependencyProperty TextDirectionProperty = DependencyProperty.Register(
            "TextDirection", typeof(TextDirection), typeof(TextBlockFX), new PropertyMetadata(default(TextDirection)));

        /// <summary>
        /// Gets or sets a value that indicates direction in which the text is read.
        /// </summary>
        public TextDirection TextDirection
        {
            get { return (TextDirection)GetValue(TextDirectionProperty); }
            set
            {
                _textDirection = value;
                SetValue(TextDirectionProperty, value);
                UpdateDrawing();
            }
        }

        /// <summary>
        /// Identifies the TextTrimming dependency property.
        /// </summary>
        public static readonly DependencyProperty TextTrimmingProperty = DependencyProperty.Register(
            "TextTrimming", typeof(TextTrimming), typeof(TextBlockFX), new PropertyMetadata(default(TextTrimming)));

        /// <summary>
        /// Gets or sets the text trimming behavior to employ when content overflows the content area.
        /// </summary>
        public TextTrimming TextTrimming
        {
            get { return (TextTrimming)GetValue(TextTrimmingProperty); }
            set
            {
                _textTrimming = value;
                SetValue(TextTrimmingProperty, value);
                UpdateDrawing();
            }
        }

        /// <summary>
        /// Identifies the TextWrapping  dependency property.
        /// </summary>
        public static readonly DependencyProperty TextWrappingProperty = DependencyProperty.Register(
            "TextWrapping", typeof(TextWrapping), typeof(TextBlockFX), new PropertyMetadata(default(TextWrapping)));

        /// <summary>
        /// Gets or sets how the TextBlockFX wraps text.
        /// </summary>
        public TextWrapping TextWrapping
        {
            get { return (TextWrapping)GetValue(TextWrappingProperty); }
            set
            {
                _textWrapping = value;
                SetValue(TextWrappingProperty, value);
                UpdateDrawing();
            }
        }

        List<WordBoundary> _superScripts;
        public List<WordBoundary> SuperScripts
        {
            get { return _superScripts; }
            set
            {
                _superScripts = value;
            }
        }
        List<WordBoundary> _subScripts;
        public List<WordBoundary> SubScripts
        {
            get { return _subScripts; }
            set
            {
                _subScripts = value;
            }
        }

        #endregion
        public TextBlockFX()
        {
            this.DefaultStyleKey = typeof(TextBlockFX);

            this.Loaded += TextBlockFX_Loaded;
            this.RegisterPropertyChangedCallback(TextBlockFX.ForegroundProperty, ForegroundChangedCallback);
            this.RegisterPropertyChangedCallback(TextBlockFX.FontFamilyProperty, FontFamilyChangedCallback);
            this.RegisterPropertyChangedCallback(TextBlockFX.FontSizeProperty, FontSizeChangedCallback);
            this.RegisterPropertyChangedCallback(TextBlockFX.FontStretchProperty, FontStretchChangedCallback);
            this.RegisterPropertyChangedCallback(TextBlockFX.FontStyleProperty, FontStyleChangedCallback);
            this.RegisterPropertyChangedCallback(TextBlockFX.FontWeightProperty, FontWeightChangedCallback);

            _textFormat.TrimmingSign = CanvasTrimmingSign.Ellipsis;
        }
        /// <inheritdoc />
        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _animatedCanvas = GetTemplateChild("AnimatedCanvas") as CanvasControl;
            this.SizeChanged += TextBlockFX_SizeChanged;

            ApplyTextFormat();
            ApplyTextForeground();

            if (_animatedCanvas != null)
            {
                _animatedCanvas.CreateResources += AnimatedCanvas_CreateResources;
                _animatedCanvas.Draw += AnimatedCanvas_Draw;
            }
        }

        private void TextBlockFX_Loaded(object sender, RoutedEventArgs e)
        {
            _newText = Text ?? string.Empty;
        }

        private void TextBlockFX_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateDrawing();
        }

        #region Property Changed Callbacks

        private void ForegroundChangedCallback(DependencyObject sender, DependencyProperty dp)
        {
            ApplyTextForeground();
        }
        private void FontFamilyChangedCallback(DependencyObject sender, DependencyProperty dp)
        {
            _textFormat.FontFamily = FontFamily.Source;
            UpdateDrawing();
        }
        private void FontSizeChangedCallback(DependencyObject sender, DependencyProperty dp)
        {
            _textFormat.FontSize = (float)FontSize;
            UpdateDrawing();
        }
        private void FontStretchChangedCallback(DependencyObject sender, DependencyProperty dp)
        {
            _textFormat.FontStretch = FontStretch;
            UpdateDrawing();
        }

        private void FontStyleChangedCallback(DependencyObject sender, DependencyProperty dp)
        {
            _textFormat.FontStyle = FontStyle;
            UpdateDrawing();
        }
        private void FontWeightChangedCallback(DependencyObject sender, DependencyProperty dp)
        {
            _textFormat.FontWeight = FontWeight;
            UpdateDrawing();
        }
        #endregion

        #region Canvas Events
        private void AnimatedCanvas_CreateResources(CanvasControl sender,
            CanvasCreateResourcesEventArgs args)
        {
            // Generate CanvasGradientStops from LinearGradientBrush
            if (Foreground is LinearGradientBrush linearGradientBrush)
            {
                var stops = new CanvasGradientStop[linearGradientBrush.GradientStops.Count];

                for (int i = 0; i < linearGradientBrush.GradientStops.Count; i++)
                {
                    var gradientStop = linearGradientBrush.GradientStops[i];
                    stops[i].Color = gradientStop.Color;
                    stops[i].Position = (float)gradientStop.Offset;
                }

                _textBrush = new CanvasLinearGradientBrush(_animatedCanvas, stops);
            }
        }

        private void AnimatedCanvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            args.DrawingSession.Clear(Colors.Transparent);

            if (_textEffect == null)
            {
                CanvasTextLayout ctl = new CanvasTextLayout(sender, _newText,_textFormat, (float)sender.Size.Width, (float)sender.Size.Height);
                ctl.Options = CanvasDrawTextOptions.EnableColorFont;
                args.DrawingSession.DrawTextLayout(ctl, 0, 0, _textColor);
            }
            else
            {
                GenerateNewTextLayout(sender);
                var effectParam = new EffectParam(_newText, _textColor, _textFormat, _newTextLayout, sender, args);
                _textEffect?.DrawText(effectParam);
            }
        }

        #endregion

        #region Helpers
        bool process = false;
        async void UpdateDrawing()
        {
            if(_animatedCanvas!= null)
            {
                if (process)
                    return;
                process = true;
                this._animatedCanvas.Invalidate();
                await Task.Delay(500);
                process = false;
            }
        }
        private void ApplyTextFormat()
        {
            _textFormat.Options = CanvasDrawTextOptions.EnableColorFont | CanvasDrawTextOptions.NoPixelSnap;
            _textFormat.HorizontalAlignment = Win2dHelpers.MapCanvasHorizontalAlignment(_textAlignment);
            _textFormat.VerticalAlignment = CanvasVerticalAlignment.Center;
            _textFormat.Direction = Win2dHelpers.MapTextDirection(_textDirection);
            _textFormat.TrimmingGranularity = Win2dHelpers.MapTrimmingGranularity(_textTrimming);
            _textFormat.WordWrapping = Win2dHelpers.MapWordWrapping(_textWrapping);
            UpdateDrawing();
        }
        private void ApplyTextForeground()
        {
            if (Foreground is SolidColorBrush colorBrush)
            {
                _textColor = colorBrush.Color;
                _textBrush = null;
            }
            else if (Foreground is LinearGradientBrush linearGradientBrush)
            {
                if (_animatedCanvas != null)
                {
                    var stops = new CanvasGradientStop[linearGradientBrush.GradientStops.Count];

                    foreach (var gradientStop in linearGradientBrush.GradientStops)
                    {
                        var stop = new CanvasGradientStop()
                        {
                            Color = gradientStop.Color,
                            Position = (float)gradientStop.Offset
                        };
                    }

                    _textBrush = new CanvasLinearGradientBrush(_animatedCanvas, stops);
                }
            }
            else
            {
                if (Application.Current.Resources["DefaultTextForegroundThemeBrush"] is SolidColorBrush defaultForegroundBrush)
                {
                    _textColor = defaultForegroundBrush.Color;
                    _textBrush = null;
                }
            }
            UpdateDrawing();
        }
        private void GenerateNewTextLayout(CanvasControl resourceCreator)
        {
            _newTextLayout = new CanvasTextLayout(resourceCreator, _newText, _textFormat,
                (float)(resourceCreator.Size.Width),
                (float)(resourceCreator.Size.Height));
            _newTextLayout.Options = CanvasDrawTextOptions.EnableColorFont | CanvasDrawTextOptions.NoPixelSnap;
            _newTextLayout.VerticalAlignment = CanvasVerticalAlignment.Center;
            UpdateDrawing();
        }




        #endregion
    }
}