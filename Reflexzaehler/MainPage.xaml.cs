using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

using Windows.Devices.Gpio;
using Windows.Devices.Spi;
using Windows.Devices.Enumeration;

using WinRTXamlToolkit.Controls.DataVisualization.Charting;


// using ThingSpeakWinRT;
// using System.Threading.Tasks;

namespace Reflexzaehler
{
    
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        public class NameValueItem
        {
            public string Name { get; set; }
            public int Value { get; set; }
        }

        // Random _random = new Random();

        private GpioController gpio;
        private GpioPin pinLED_Active;
        private GpioPinValue pinLED_Active_Value;

        private SpiDevice spiDevice;

        private DispatcherTimer timer;
        private SolidColorBrush redBrush = new SolidColorBrush(Windows.UI.Colors.Red);
        private SolidColorBrush grayBrush = new SolidColorBrush(Windows.UI.Colors.LightGray);
        private int state = 0;
        private int counter = 0;

        private List<NameValueItem> items1;

        //private ThingSpeakClient theThingspeakClient = new ThingSpeakClient(false);

        public MainPage()
        {
            int iErr = 0;

            this.InitializeComponent();
            Loaded += MainPage_Loaded;
            

            // create timer
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(50);
            timer.Tick += Timer_Tick;

            // init StatusLED
            gpio = GpioController.GetDefault();
            iErr = InitStateLED( 4 );

            // chart
            items1 = new List<NameValueItem>();
            for (int i = 1; i <= 20; i++)
            {
                items1.Add(new NameValueItem { Name = "", Value = 0 }); // = _random.Next(10, 100) }); 
            }
            
            // Supply items to the series
            ((LineSeries)this.LineChart1.Series[0]).ItemsSource = items1;
            
            // [OPTIONAL] Change Y-Axis range from 0 to 1000 with interval of 250 of Series[0]
            ((LineSeries)this.LineChart1.Series[0]).DependentRangeAxis =
               new LinearAxis
               {
                   Minimum = 0,
                   Maximum = 100,
                   Orientation = AxisOrientation.Y,
                   Interval = 20,
                   ShowGridLines = true
               };

            // init done
            if ( iErr == 0)
            {
                timer.Start();
            }

        }

        
        private async void MainPage_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            string selector = SpiDevice.GetDeviceSelector();
            IReadOnlyList<DeviceInformation> devices = await DeviceInformation.FindAllAsync(selector);

            if( devices.Count > 0 )
            {
                // SPI settings
                var settings = new SpiConnectionSettings(0);
                settings.ClockFrequency = 5000000;
                settings.Mode = SpiMode.Mode3;

                spiDevice = await SpiDevice.FromIdAsync(devices[0].Id, settings);
                if( spiDevice == null)
                {
                    Debug.WriteLine("SPI0 in use");
                    Application.Current.Exit();
                }
            }
            else
            {
                Debug.WriteLine("no SPI device");
                Application.Current.Exit();
            }
        }
        

        /* public async void SendToThingspeak()
        {
            ThingSpeakFeed dataFeed = new ThingSpeakFeed { Field1 = "58.27", Field2 = "32.59" };
            dataFeed = await theThingspeakClient.UpdateFeedAsync("<Your Write API Key>", dataFeed);

        }
        */

        private int InitStateLED(int pin)
        {
            pinLED_Active = gpio.OpenPin(pin);

            if (pinLED_Active != null)
            {
                pinLED_Active_Value = GpioPinValue.Low;
                pinLED_Active.Write(pinLED_Active_Value);
                pinLED_Active.SetDriveMode(GpioPinDriveMode.Output);
            }
            else
            {
                return 1; 
            }   
            
            return 0;
        }

        private void SwitchStateLED(ref GpioPin pin, ref GpioPinValue pinValue, ref Ellipse LED)
        {
            if (pinValue == GpioPinValue.High)
            {
                pinValue = GpioPinValue.Low;
                pin.Write(pinValue);
                LED.Fill = grayBrush;
            }
            else
            {
                pinValue = GpioPinValue.High;
                pin.Write(pinValue);
                LED.Fill = redBrush;
            }
        }

        public int ReadValueFromMCP3002(byte channelNumber)
        {
            byte[] readBuffer = new byte[2];
            byte[] writeBuffer = new byte[2];

            // 0x40: start bit (0b0100_0000, 7. bit)
            // 0x20: single ended (0b0010_0000, 6. bit)
            // 0x10: channel=1 (0b0001_0000, 5. bit)
            // 0x08: MSB (0b0000_1000, 4. bit)
            writeBuffer[0] = (byte)(0x40 | 0x20);
            if (channelNumber == 1) writeBuffer[0] |= 0x10;

            spiDevice.TransferFullDuplex(writeBuffer, readBuffer);

            // byte 0: only 2 bits 
            var value = (readBuffer[0] & 0x03) << 8;
            // byte 1: all 8 bits
            value |= readBuffer[1];
            
            return value;
        }

        private void Timer_Tick(object sender, object e)
        {
            
            int adcValue;
            
            // string valueWithCounter;

            adcValue = ReadValueFromMCP3002(0); // IR photo transistor @ channel 0 of MCP 3002
            counter++;
            /*
            valueWithCounter = counter.ToString();
            valueWithCounter += ", ";
            valueWithCounter += adcValue.ToString();
            ValueListBox.Items.Add(valueWithCounter);
            ValueListBox.UpdateLayout();
            ValueListBox.ScrollIntoView(valueWithCounter);
            */

            items1.RemoveAt(0);
            items1.Add(new NameValueItem { Name = counter.ToString(), Value = adcValue }); // = _random.Next(10, 100) });
            ((LineSeries)this.LineChart1.Series[0]).Refresh();


            // trigger state LED
            if ((adcValue >= 50 ) && (state == 0))
            {
                state = 1;
                SwitchStateLED(ref pinLED_Active, ref pinLED_Active_Value, ref LED_Active);
            }
            if ((adcValue < 50 ) && ( state == 1))
            {
                state = 0;
                SwitchStateLED(ref pinLED_Active, ref pinLED_Active_Value, ref LED_Active);
            }
        }
    }
}
