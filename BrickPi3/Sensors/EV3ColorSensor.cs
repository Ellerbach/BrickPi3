//////////////////////////////////////////////////////////
// This code has been originally created by Laurent Ellerbach
// It intend to make the excellent BrickPi3 from Dexter Industries working
// on a RaspberryPi 2 or 3 runing Windows 10 IoT Core in Universal
// Windows Platform.
// Credits:
// - Dexter Industries Code
// - MonoBrick for great inspiration regarding sensors implementation in C#
//
// This code is origianlly created for the original BrickPi
// see https://github.com/ellerbach/BrickPi
//
// This code is under https://opensource.org/licenses/ms-pl
//
//////////////////////////////////////////////////////////

using BrickPi3.Extensions;
using BrickPi3.Models;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace BrickPi3.Sensors
{
    public sealed class EV3ColorSensor : INotifyPropertyChanged, ISensor
    {
        private Brick brick = null;
        private ColorSensorMode colorMode;
        private Int16[] rawValues = new Int16[4];

        private const int RedIndex = 0;
        private const int GreenIndex = 1;
        private const int BlueIndex = 2;
        private const int BackgroundIndex = 3;

        /// <summary>
        /// Initialize an EV3 Color Sensor
        /// </summary>
        /// <param name="port">Sensor port</param>
        public EV3ColorSensor(Brick brick, BrickPortSensor port) : this(brick, port, ColorSensorMode.Color, 1000)
        { }

        /// <summary>
        /// Initialize an EV3 Color Sensor
        /// </summary>
        /// <param name="port">Sensor port</param>
        /// <param name="mode">Color mode</param>
        public EV3ColorSensor(Brick brick, BrickPortSensor port, ColorSensorMode mode) : this(brick, port, mode, 1000)
        { }

        /// <summary>
        /// Initilaize an EV3 Color Sensor
        /// </summary>
        /// <param name="port">Sensor port</param>
        /// <param name="mode">Color mode</param>
        /// <param name="timeout">Period in millisecond to check sensor value changes</param>
        public EV3ColorSensor(Brick brick, BrickPortSensor port, ColorSensorMode mode, int timeout)
        {
            this.brick = brick;
            Port = port;
            colorMode = mode;
            //set the correct mode
            //brick.BrickPi.Sensor[(int)Port].Type = GetEV3Mode(mode);
            brick.set_sensor_type((byte)Port, GetEV3Mode(mode));
            periodRefresh = timeout;
            timer = new Timer(UpdateSensor, this, TimeSpan.FromMilliseconds(timeout), TimeSpan.FromMilliseconds(timeout));
        }

        private Timer timer = null;
        private void StopTimerInternal()
        {
            if (timer != null)
            {
                timer.Dispose();
                timer = null;
            }
        }

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        /// <summary>
        /// To notify a property has changed. The minimum time can be set up
        /// with timeout property
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        private int periodRefresh;
        /// <summary>
        /// Period to refresh the notification of property changed in milliseconds
        /// </summary>
        public int PeriodRefresh
        {
            get { return periodRefresh; }
            set
            {
                periodRefresh = value;
                timer.Change(TimeSpan.FromMilliseconds(periodRefresh), TimeSpan.FromMilliseconds(periodRefresh));
            }
        }
        private SENSOR_TYPE GetEV3Mode(ColorSensorMode mode)
        {
            SENSOR_TYPE ret = SENSOR_TYPE.EV3_COLOR_REFLECTED; //.EV3_COLOR_M0;
            switch (mode)
            {
                //question is about the various modes and if theyr are supported and how
                case ColorSensorMode.Color:
                    ret = SENSOR_TYPE.EV3_COLOR_COLOR; //BrickSensorType.EV3_COLOR_M2;
                    break;
                case ColorSensorMode.Reflection:
                    ret = SENSOR_TYPE.EV3_COLOR_REFLECTED; //BrickSensorType.EV3_COLOR_M0;
                    break;
                case ColorSensorMode.Green:
                    ret = SENSOR_TYPE.EV3_COLOR_RAW_REFLECTED; //BrickSensorType.EV3_COLOR_M3;
                    break;
                case ColorSensorMode.Blue:
                    ret = SENSOR_TYPE.EV3_COLOR_COLOR_COMPONENTS; //BrickSensorType.EV3_COLOR_M4;
                    break;
                case ColorSensorMode.Ambient:
                    ret = SENSOR_TYPE.EV3_COLOR_AMBIENT; //BrickSensorType.EV3_COLOR_M1;
                    break;
            }
            return ret;

        }

        public ColorSensorMode ColorMode
        {
            get { return colorMode; }
            set
            {
                if (value != colorMode)
                {
                    colorMode = value;
                    //brick.BrickPi.Sensor[(int)Port].Type = GetEV3Mode(colorMode);
                    brick.set_sensor_type((byte)Port, GetEV3Mode(colorMode));
                }
            }
        }

        private int value;
        private string valueAsString;

        /// <summary>
        /// Return the raw value of the sensor
        /// </summary>
        public int Value
        {
            get { return ReadRaw(); }
            internal set
            {
                if (value != this.value)
                {
                    this.value = value;
                    OnPropertyChanged(nameof(Value));
                }
            }
        }

        /// <summary>
        /// Return the raw value  as a string of the sensor
        /// </summary>
        public string ValueAsString
        {
            get { return ReadAsString(); }
            internal set
            {
                if (valueAsString != value)
                {
                    valueAsString = value;
                    OnPropertyChanged(nameof(ValueAsString));
                }
            }
        }
        /// <summary>
        /// Update the sensor and this will raised an event on the interface
        /// </summary>
        public void UpdateSensor(object state)
        {
            Value = ReadRaw();
            ValueAsString = ReadAsString();
        }

        private void GetRawValues()
        {
            try
            {
                //rawValues[BackgroundIndex] = (short)(brick.BrickPi.Sensor[(int)Port].Value & 0xFF);
                //rawValues[BlueIndex] = (short)((brick.BrickPi.Sensor[(int)Port].Value >> 8) & 0xFF);
                //rawValues[GreenIndex] = (short)((brick.BrickPi.Sensor[(int)Port].Value >> 16) & 0xFF);
                //rawValues[RedIndex] = (short)((brick.BrickPi.Sensor[(int)Port].Value >> 24) & 0xFF);
                var ret = brick.get_sensor((byte)Port);
                switch (colorMode)
                {
                    case ColorSensorMode.Color:
                    case ColorSensorMode.Reflection:
                    case ColorSensorMode.Ambient:
                        rawValues[BackgroundIndex] = ret[0];
                        rawValues[BlueIndex] = 0;
                        rawValues[GreenIndex] = 0;
                        rawValues[RedIndex] = 0;
                        break;
                    case ColorSensorMode.Green:
                    case ColorSensorMode.Blue:
                        rawValues[BackgroundIndex] = ret[0];
                        rawValues[BlueIndex] = ret[1];
                        rawValues[GreenIndex] = ret[2];
                        rawValues[RedIndex] = ret[3];
                        break;
                }
            }
            catch (Exception)
            {

                ;
            }

        }

        public int ReadRaw()
        {
            int val = 0;
            switch (colorMode)
            {
                case ColorSensorMode.Color:
                case ColorSensorMode.Reflection:
                case ColorSensorMode.Ambient:
                    val = (int)ReadColor();
                    break;
                case ColorSensorMode.Green:
                case ColorSensorMode.Blue:
                    val = CalculateRawAverage();
                    break;
            }
            return val;
        }

        /// <summary>
        /// Read the intensity of the reflected or ambient light in percent. In color mode the color index is returned
        /// </summary>
        public int Read()
        {
            int val = 0;
            switch (colorMode)
            {

                case ColorSensorMode.Color:
                case ColorSensorMode.Reflection:
                case ColorSensorMode.Ambient:
                    val = (int)ReadColor();
                    break;
                default:
                    val = CalculateRawAverageAsPct();
                    break;
            }
            return val;
        }

        private int CalculateRawAverage()
        {
            if (colorMode == ColorSensorMode.Color)
            {
                //GetRawValues();
                //return (int)(rawValues[RedIndex] + rawValues[BlueIndex] + rawValues[GreenIndex]) / 3;
                return 0;
            }
            else
                try
                {
                    var ret = brick.get_sensor((byte)Port);
                    return (ret[0] + (ret[1] >> 8) + (ret[2] >> 16) + ret[3] >> 24) / 255 / 3;
                    //return brick.BrickPi.Sensor[(int)Port].Value / 255 / 3;
                }
                catch (Exception)
                {
                    return 0;
                }

        }

        private int CalculateRawAverageAsPct()
        {
            //Need to find out what is the ADC resolution
            //1023 is probablt not the correct one
            return (CalculateRawAverage() * 100) / 1023;
        }

        public string ReadTest()
        {
            GetRawValues();
            string ret = "";
            for (int i = 0; i < rawValues.Length; i++)
                ret += " " + rawValues[i];
            ret += " " + (rawValues[BackgroundIndex] + (rawValues[BlueIndex] >> 8) + (rawValues[GreenIndex] >> 16) + rawValues[RedIndex] >> 24).ToString(); //brick.BrickPi.Sensor[(int)Port].Value;
            return ret;

        }

        public string ReadAsString()
        {
            string s = "";
            switch (colorMode)
            {
                case ColorSensorMode.Color:
                    s = ReadColor().ToString();
                    break;
                case ColorSensorMode.Reflection:
                case ColorSensorMode.Green:
                case ColorSensorMode.Blue:
                    s = Read().ToString();
                    break;
                case ColorSensorMode.Ambient:
                    s = Read().ToString();
                    break;
            }

            return s;
        }

        /// <summary>
        /// Reads the color.
        /// </summary>
        /// <returns>The color.</returns>
        public Color ReadColor()
        {
            Color color = Color.None;
            if (colorMode == ColorSensorMode.Color)
            {
                try
                {
                    //color = (Color)brick.BrickPi.Sensor[(int)Port].Value; //CalculateColor();
                    color = (Color)brick.get_sensor((byte)Port)[0];
                }
                catch (Exception)
                {

                    color = Color.None;
                }

            }
            return color;
        }

        /// <summary>
        /// Reads the color of the RGB.
        /// </summary>
        /// <returns>The RGB color.</returns>
        public RGBColor ReadRGBColor()
        {
            GetRawValues();
            return new RGBColor((byte)rawValues[RedIndex], (byte)rawValues[GreenIndex], (byte)rawValues[BlueIndex]);
        }

        private Color CalculateColor()
        {
            //Taken from the LeJos source code - thanks ;-)
            GetRawValues();
            int red = rawValues[RedIndex];
            int blue = rawValues[BlueIndex];
            int green = rawValues[GreenIndex];
            int blank = rawValues[BackgroundIndex];
            // we have calibrated values, now use them to determine the color

            // The following algorithm comes from the 1.29 Lego firmware.
            if (red > blue && red > green)
            {
                // Red dominant color
                if (red < 65 || (blank < 40 && red < 110))
                    return Color.Black;
                if (((blue >> 2) + (blue >> 3) + blue < green) &&
                        ((green << 1) > red))
                    return Color.Yellow;
                if ((green << 1) - (green >> 2) < red)
                    return Color.Red;
                if (blue < 70 || green < 70 || (blank < 140 && red < 140))
                    return Color.Black;
                return Color.White;
            }
            else if (green > blue)
            {
                // Green dominant color
                if (green < 40 || (blank < 30 && green < 70))
                    return Color.Black;
                if ((blue << 1) < red)
                    return Color.Yellow;
                if ((red + (red >> 2)) < green ||
                        (blue + (blue >> 2)) < green)
                    return Color.Green;
                if (red < 70 || blue < 70 || (blank < 140 && green < 140))
                    return Color.Black;
                return Color.White;
            }
            else
            {
                // Blue dominant color
                if (blue < 48 || (blank < 25 && blue < 85))
                    return Color.Black;
                if ((((red * 48) >> 5) < blue && ((green * 48) >> 5) < blue) ||
                        ((red * 58) >> 5) < blue || ((green * 58) >> 5) < blue)
                    return Color.Blue;
                if (red < 60 || green < 60 || (blank < 110 && blue < 120))
                    return Color.Black;
                if ((red + (red >> 3)) < blue || (green + (green >> 3)) < blue)
                    return Color.Blue;
                return Color.White;
            }
        }


        public BrickPortSensor Port
        {
            get; internal set;
        }

        public string GetSensorName()
        {
            return "EV3 Color Sensor";
        }

        public void SelectNextMode()
        {
            ColorMode = ColorMode.Next();
            return;
        }

        public void SelectPreviousMode()
        {
            ColorMode = ColorMode.Previous();
            return;
        }

        public int NumberOfModes()
        {
            return Enum.GetNames(typeof(ColorSensorMode)).Length;
        }

        public string SelectedMode()
        {
            return ColorMode.ToString();
        }
    }
}
