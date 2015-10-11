using System;
using Windows.ApplicationModel.Core;
using Windows.Devices.Gpio;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Button_Switch
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private const int BUTTON_PIN = 4;
        private GpioPin _buttonpin;
        private double _imageInitialHeight = 500;

        public MainPage()
        {
            this.InitializeComponent();

            Unloaded += MainPage_Unloaded;

            InitGPIO();
        }

        private void InitGPIO()
        {
            var gpio = GpioController.GetDefault();

            // Show an error if there is no GPIO controller
            if (gpio == null)
            {
                _buttonpin = null;
                GpioStatus.Text = "There is no GPIO controller on this device.";
                return;
            }

            _buttonpin = gpio.OpenPin(BUTTON_PIN);

            if (_buttonpin.IsDriveModeSupported(GpioPinDriveMode.InputPullUp))
                _buttonpin.SetDriveMode(GpioPinDriveMode.InputPullUp);
            else
                _buttonpin.SetDriveMode(GpioPinDriveMode.Input);

            _buttonpin.DebounceTimeout = TimeSpan.FromMilliseconds(50); // Delay time the ValueChanged event is fired after pressing or releasing the button
            _buttonpin.ValueChanged += Buttonpin_ValueChanged;

            GpioStatus.Text = "GPIO button pin initialized correctly.";
        }

        private void MainPage_Unloaded(object sender, object args)
        {
            // Cleanup
            _buttonpin.Dispose();
        }

        private async void Buttonpin_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            Windows.UI.Color color;

            switch (args.Edge)
            {
                case GpioPinEdge.FallingEdge:
                    color = Windows.UI.Colors.Red;
                    break;
                case GpioPinEdge.RisingEdge:
                    color = Windows.UI.Colors.LightGray;
                    break;
            }

            // Use async process to change the LED control color
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal,() => { FlipLED(color); }
                );
        }

        private void FlipLED(Windows.UI.Color color)
        {
            var brush = new SolidColorBrush(color);
            LED.Fill = brush;
        }

        private void Delay_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (_buttonpin == null) return;

            _buttonpin.DebounceTimeout = TimeSpan.FromMilliseconds(e.NewValue);
            DelayText.Text = $"{e.NewValue} ms";
        }

        private void Image_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            // Zoom image on tap/click
            if (Image1.Height.Equals(_imageInitialHeight))
                Image1.Height = Window.Current.Bounds.Height - this.Padding.Top - this.Padding.Bottom;
            else
                Image1.Height = _imageInitialHeight;
        }
    }
}
