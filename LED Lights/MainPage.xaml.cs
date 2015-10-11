using System;
using Windows.Devices.Gpio;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace LED_Lights
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private int _ledStatus = 0;
        private double _imageInitialHeight = 250;
        private const int REDLED_PIN = 5;
        private const int BLUELED_PIN = 6;
        private const int GREENLED_PIN = 13;
        private GpioPin _redpin;
        private GpioPin _bluepin;
        private GpioPin _greenpin;
        private DispatcherTimer _timer;
        private SolidColorBrush _redBrush = new SolidColorBrush(Windows.UI.Colors.Red);
        private SolidColorBrush _blueBrush = new SolidColorBrush(Windows.UI.Colors.Blue);
        private SolidColorBrush _greenBrush = new SolidColorBrush(Windows.UI.Colors.Green);

        public MainPage()
        {
            InitializeComponent();

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(500);
            _timer.Tick += Timer_Tick;
            _timer.Start();

            Unloaded += MainPage_Unloaded;

            InitGPIO();
        }

        private void InitGPIO()
        {
            var gpio = GpioController.GetDefault();

            // Show an error if there is no GPIO controller
            if (gpio == null)
            {
                _redpin = null;
                _bluepin = null;
                _greenpin = null;
                GpioStatus.Text = "There is no GPIO controller on this device.";
                return;
            }

            _redpin = gpio.OpenPin(REDLED_PIN);
            _bluepin = gpio.OpenPin(BLUELED_PIN);
            _greenpin = gpio.OpenPin(GREENLED_PIN);

            _redpin.Write(GpioPinValue.High);
            _redpin.SetDriveMode(GpioPinDriveMode.Output);
            _bluepin.Write(GpioPinValue.High);
            _bluepin.SetDriveMode(GpioPinDriveMode.Output);
            _greenpin.Write(GpioPinValue.High);
            _greenpin.SetDriveMode(GpioPinDriveMode.Output);

            GpioStatus.Text = "GPIO blue/red/green pin initialized correctly.";
        }

        private void MainPage_Unloaded(object sender, object args)
        {
            // Cleanup
            _redpin.Dispose();
            _bluepin.Dispose();
            _greenpin.Dispose();
        }

        private void FlipLED()
        {
            SolidColorBrush brush;

            if (_ledStatus == 0)
            {
                _ledStatus = 1;
                if (_redpin != null && _bluepin != null && _greenpin != null)
                {
                    //turn on red
                    _redpin.Write(GpioPinValue.High);
                    _bluepin.Write(GpioPinValue.Low);
                    _greenpin.Write(GpioPinValue.Low);
                }
                brush = _redBrush;
            }
            else if (_ledStatus == 1)
            {
                _ledStatus = 2;
                if (_redpin != null && _bluepin != null && _greenpin != null)
                {
                    //turn on blue
                    _redpin.Write(GpioPinValue.Low);
                    _bluepin.Write(GpioPinValue.High);
                    _greenpin.Write(GpioPinValue.Low);
                }
                brush = _blueBrush;
            }
            else
            {
                _ledStatus = 0;
                if (_redpin != null && _bluepin != null && _greenpin != null)
                {
                    //turn on green
                    _redpin.Write(GpioPinValue.Low);
                    _bluepin.Write(GpioPinValue.Low);
                    _greenpin.Write(GpioPinValue.High);
                }
                brush = _greenBrush;
            }

            LED.Fill = brush;
        }

        private void TurnOffLED()
        {
            if (_ledStatus == 1) FlipLED();
        }

        private void Timer_Tick(object sender, object e)
        {
            FlipLED();
        }

        private void Delay_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (_timer == null) return;
            
            if (e.NewValue == Delay.Minimum)
            {
                DelayText.Text = "Stopped";
                _timer.Stop();
                TurnOffLED();
            }
            else
            {
                DelayText.Text = e.NewValue + "ms";
                _timer.Interval = TimeSpan.FromMilliseconds(e.NewValue);
                _timer.Start();
            }
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
