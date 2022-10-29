using GalaSoft.MvvmLight.Command;
using Sample.Win2D.UWP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using TextBlockFX;
using TextBlockFX.Win2D.UWP;
using TextBlockFX.Win2D.UWP.Effects;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Sample.UWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }
        public ICommand GotoAnimateCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    this.Frame.Navigate(typeof(AnimatedEffectPage));
                });
            }
        }
        public ICommand GotoTextEffectsCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    this.Frame.Navigate(typeof(TextEffectPage));
                });
            }
        }
    }
}
