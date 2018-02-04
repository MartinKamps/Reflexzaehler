using System;
using System.Collections.Generic;
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
using Windows.Devices.Gpio;
using Windows.UI.Xaml.Shapes;

using ThingSpeakWinRT;
using System.Threading.Tasks;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x407 dokumentiert.

namespace Reflexzaehler
{


    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        private GpioController gpio;
        private GpioPin pinLED_Active;
        private GpioPinValue pinLED_Active_Value;
        private DispatcherTimer timer;
        private SolidColorBrush redBrush = new SolidColorBrush(Windows.UI.Colors.Red);
        private SolidColorBrush grayBrush = new SolidColorBrush(Windows.UI.Colors.LightGray);
        private int state = 0;

        private ThingSpeakClient theThingspeakClient = new ThingSpeakClient(false);

        public MainPage()
        {
            int iErr = 0;

            this.InitializeComponent();
                        
            // create timer
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += Timer_Tick;

            // init StatusLED
            gpio = GpioController.GetDefault();
            iErr = InitStateLED( 4 );

            // init done
            if ( iErr == 0)
            {
                timer.Start();
            }

        }

         public async void SendToThingspeak()
        {
            ThingSpeakFeed dataFeed = new ThingSpeakFeed { Field1 = "58.27", Field2 = "32.59" };
            dataFeed = await theThingspeakClient.UpdateFeedAsync("<Your Write API Key>", dataFeed);

        }

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
                LED.Fill = redBrush;
            }
            else
            {
                pinValue = GpioPinValue.High;
                pin.Write(pinValue);
                LED.Fill = grayBrush;
            }
        }

        private void Timer_Tick(object sender, object e)
        {



            // trigger state LED
            if (state == 0)
            {
                state = 1;
                SwitchStateLED(ref pinLED_Active, ref pinLED_Active_Value, ref LED_Active);
            }
            else
            {
                state = 0;
                SwitchStateLED(ref pinLED_Active, ref pinLED_Active_Value, ref LED_Active);
            }
        }
    }
}
