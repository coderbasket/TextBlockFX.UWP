using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using TextBlockFX.Win2D.UWP.Effects;
using TextBlockFX.Win2D.UWP;
using TextBlockFX;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Text;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Core;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Sample.Win2D.UWP
{
    public sealed partial class TextEffectPage : Page
    {
      
        private readonly DispatcherTimer _timer = new DispatcherTimer();

        private int _index = -1;

        private string[] _sampleTexts;

        private ITextEffect _selectedEffect;
        private int _selectedSampleTextIndex;

        public List<BuiltInEffect> BuiltInEffects => new List<BuiltInEffect>()
        {
            new BuiltInEffect("Advanced", new AdvanceEffectFX()),            

        };

        public ITextEffect SelectedEffect
        {
            get => _selectedEffect;
            set
            {
                _selectedEffect = value;
                TBFX.TextEffect = _selectedEffect;
            }
        }

        public int SelectedSampleTextIndex
        {
            get => _selectedSampleTextIndex;
            set
            {
                _selectedSampleTextIndex = value;

                switch (value)
                {
                    default:
                    case 0:
                        _sampleTexts = DataBaseHelper._inOtherWords;
                        break;
                    case 1:
                        _sampleTexts = DataBaseHelper._textsOfMencius;
                        break;
                    case 2:
                        _sampleTexts = DataBaseHelper._textsOfMakenaide;
                        break;
                    case 3:
                        _sampleTexts = DataBaseHelper._textsOfOdeToJoy;
                        break;
                }
            }
        }

        public List<ComboWrapper<FontStretch>> FontStretches => GetEnumAsList<FontStretch>();

        public List<ComboWrapper<FontStyle>> FontStyles => GetEnumAsList<FontStyle>();

        public List<ComboWrapper<FontWeight>> FontWeightsList => new List<ComboWrapper<FontWeight>>()
        {
            new ComboWrapper<FontWeight>("ExtraBlack", FontWeights.ExtraBlack),
            new ComboWrapper<FontWeight>("Black", FontWeights.Black),
            new ComboWrapper<FontWeight>("ExtraBold", FontWeights.ExtraBold),
            new ComboWrapper<FontWeight>("Bold", FontWeights.Bold),
            new ComboWrapper<FontWeight>("SemiBold", FontWeights.SemiBold),
            new ComboWrapper<FontWeight>("Medium", FontWeights.Medium),
            new ComboWrapper<FontWeight>("Normal", FontWeights.Normal),
            new ComboWrapper<FontWeight>("SemiLight", FontWeights.SemiLight),
            new ComboWrapper<FontWeight>("Light", FontWeights.Light),
            new ComboWrapper<FontWeight>("ExtraLight", FontWeights.ExtraLight),
            new ComboWrapper<FontWeight>("Thin", FontWeights.Thin),
        };

        public TextEffectPage()
        {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;
            _timer.Interval = TimeSpan.FromMilliseconds(1000);
            _timer.Tick += _timer_Tick;
            _sampleTexts = DataBaseHelper._inOtherWords;
            //this.TBFX.SuperScripts = new List<WordBoundary>() 
            //{ 
            //    new WordBoundary() { Words = "5", Length = 1 },
            //    new WordBoundary() { Words = "4", Length = 1 },
            //     new WordBoundary() { Words = "6", Length = 1 },
            //};
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < FontStretches.Count; i++)
            {
                if (FontStretches[i].Value == FontStretch.Normal)
                {
                    FontStretchComboBox.SelectedIndex = i;
                }
            }

            for (int i = 0; i < FontStyles.Count; i++)
            {
                if (FontStyles[i].Value == FontStyle.Normal)
                {
                    FontStyleComboBox.SelectedIndex = i;
                }
            }

            for (int i = 0; i < FontWeightsList.Count; i++)
            {
                if (FontWeightsList[i].Value.Weight == FontWeights.Normal.Weight)
                {
                    FontWeightComboBox.SelectedIndex = i;
                }
            }
            AutoPlayButton.IsChecked = true;
            Start();
            //StartIndexer();
            var currentView = SystemNavigationManager.GetForCurrentView();
            currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            currentView.BackRequested += delegate
            {
                try
                {
                    this.Frame.GoBack();
                }
                catch { }
            };
        }
        async void StartIndexer()
        {
            int c = 0;
            foreach (var i in BuiltInEffects)
            {
                await Task.Delay(3000);
                this.EffectComboBox.SelectedIndex = c;
                c++;
            }
            StartIndexer();
        }
        private void _timer_Tick(object sender, object e)
        {
            SetSampleText();
            _timer.Stop();
        }

        private void InputBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            TBFX.Text = InputBox.Text;
        }

        private void AutoPlayButton_OnClick(object sender, RoutedEventArgs e)
        {
            Start();
        }
        void Start()
        {
            if (AutoPlayButton.IsChecked == true)
            {
                _index = -1;
                SetSampleText();
                InputBox.IsEnabled = false;
            }
            else
            {
                InputBox.IsEnabled = true;
                _timer.Stop();
            }
        }
        private void SetSampleText()
        {
            _index = (_index + 1) % _sampleTexts.Length;
            string text = _sampleTexts[_index];
            TBFX.Text = text;
        }

        private void TBFX_OnRedrawStateChanged(object sender, RedrawState e)
        {
            if (AutoPlayButton.IsChecked == true && e == RedrawState.Idle)
            {
                _timer.Start();
            }
        }

        private void EffectComboBox_OnLoaded(object sender, RoutedEventArgs e)
        {
            EffectComboBox.SelectedIndex = 0;
        }

        private void TextComboBox_OnLoaded(object sender, RoutedEventArgs e)
        {
            TextComboBox.SelectedIndex = 0;
        }

        private static List<ComboWrapper<T>> GetEnumAsList<T>()
        {
            var names = Enum.GetNames(typeof(T)).ToList();
            var values = Enum.GetValues(typeof(T)).Cast<T>().ToList();
            return names.Zip(values, (k, v) => new ComboWrapper<T>(k, v)).ToList();
        }
    }

    

}
