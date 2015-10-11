using System;
using Windows.ApplicationModel.Core;
using Windows.Devices.Gpio;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Buzzers
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private const int BUTTON_PIN = 4;
        private const int BUZZER_PIN = 5;
        private GpioPin _buttonpin;
        private GpioPin _buzzerpin;
        private double _imageInitialHeight = 250;

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
                _buzzerpin = null;
                GpioStatus.Text = "There is no GPIO controller on this device.";
                return;
            }

            _buttonpin = gpio.OpenPin(BUTTON_PIN);
            _buzzerpin = gpio.OpenPin(BUZZER_PIN);

            if (_buttonpin.IsDriveModeSupported(GpioPinDriveMode.InputPullUp))
                _buttonpin.SetDriveMode(GpioPinDriveMode.InputPullUp);
            else
                _buttonpin.SetDriveMode(GpioPinDriveMode.Input);

            _buttonpin.DebounceTimeout = TimeSpan.FromMilliseconds(50);
            _buttonpin.ValueChanged += Buttonpin_ValueChanged;

            _buzzerpin.Write(GpioPinValue.Low);
            _buzzerpin.SetDriveMode(GpioPinDriveMode.Output);

            GpioStatus.Text = "GPIO button and buzzer pin initialized correctly.";
        }

        private async void Buttonpin_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            Windows.UI.Color color;

            switch (args.Edge)
            {
                case GpioPinEdge.FallingEdge:
                    _buzzerpin.Write(GpioPinValue.High); // Activate
                    color = Windows.UI.Colors.Red;
                    break;
                case GpioPinEdge.RisingEdge:
                    _buzzerpin.Write(GpioPinValue.Low); // Deactivate
                    color = Windows.UI.Colors.LightGray;
                    break;
            }

            // Use async process to change the LED control color
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal, () => { FlipLED(color); }
                );
        }

        private void MainPage_Unloaded(object sender, object args)
        {
            // Cleanup
            _buttonpin.Dispose();
            _buzzerpin.Dispose();
        }

        private void FlipLED(Windows.UI.Color color)
        {
            var brush = new SolidColorBrush(color);
            LED.Fill = brush;
        }

        private void Image_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            // Zoom image on tap/click
            var image = (Image)sender;

            if (image.Height.Equals(_imageInitialHeight))
                image.Height = Window.Current.Bounds.Height - this.Padding.Top - this.Padding.Bottom;
            else
                image.Height = _imageInitialHeight;
        }
    }
}
