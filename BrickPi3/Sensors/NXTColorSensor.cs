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

namespace BrickPi3.Sensors
{

    public enum ColorSensorMode
    {
        Color = SENSOR_TYPE.NXT_COLOR_FULL, //BrickSensorType.COLOR_FULL,
        Reflection = SENSOR_TYPE.NXT_COLOR_RED, //BrickSensorType.COLOR_RED,
        Green = SENSOR_TYPE.NXT_COLOR_GREEN, //BrickSensorType.COLOR_GREEN,
        Blue = SENSOR_TYPE.NXT_COLOR_BLUE, //BrickSensorType.COLOR_BLUE,
        Ambient = SENSOR_TYPE.NXT_COLOR_OFF // BrickSensorType.COLOR_NONE
    }

    /// <summary>
    /// Colors that can be read from the EV3 color sensor
    /// </summary>
    public enum Color
    {
#pragma warning disable
        None = 0, Black = 1, Blue = 2, Green = 3,
        Yellow = 4, Red = 5, White = 6, Brown = 7
#pragma warning restore
    };

    /// <summary>
    /// Class that holds RGB colors
    /// </summary>
    public sealed class RGBColor
    {
        private byte red;
        private byte green;
        private byte blue;
        /// <summary>
        /// Initializes a new instance of the <see cref="BrickPi.Sensors.RGBColor"/> class.
        /// </summary>
        /// <param name='red'>
        /// Red value
        /// </param>
        /// <param name='green'>
        /// Green value
        /// </param>
        /// <param name='blue'>
        /// Blue value
        /// </param>
		public RGBColor(byte red, byte green, byte blue) { this.red = red; this.green = green; this.blue = blue; }

        /// <summary>
        /// Gets the red value
        /// </summary>
        /// <value>
        /// The red value
        /// </value>
        public byte Red { get { return red; } }

        /// <summary>
        /// Gets the green value
        /// </summary>
        /// <value>
        /// The green value
        /// </value>
        public byte Green { get { return green; } }

        /// <summary>
        /// Gets the blue value
        /// </summary>
        /// <value>
        /// The blue value
        /// </value>
        public byte Blue { get { return blue; } }
    }

    public sealed class NXTColorSensor : INotifyPropertyChanged, ISensor
    {
        private Brick brick = null;
        private ColorSensorMode colorMode;

        private const int RedIndex = 0;
        private const int GreenIndex = 1;
        private const int BlueIndex = 2;
        private const int BackgroundIndex = 3;


        //private Int16[] colorValues = new Int16[4];
        private Int16[] rawValues = new Int16[4];


        /// <summary>
        /// Initialize a NXT Color Sensor
        /// </summary>
        /// <param name="port">Sensor port</param>
        public NXTColorSensor(Brick brick, BrickPortSensor port) : this(brick, port, ColorSensorMode.Color, 1000)
        { }

        /// <summary>
        /// Initialize a NXT Color Sensor
        /// </summary>
        /// <param name="port">Sensor port</param>
        /// <param name="mode">Color mode</param>
        public NXTColorSensor(Brick brick, BrickPortSensor port, ColorSensorMode mode) : this(brick, port, mode, 1000)
        { }

        /// <summary>
        /// Initialize a NXT Color Sensor
        /// </summary>
        /// <param name="port">Sensor port</param>
        /// <param name="mode">Color mode</param>
        /// <param name="timeout">Period in millisecond to check sensor value changes</param>
        public NXTColorSensor(Brick brick, BrickPortSensor port, ColorSensorMode mode, int timeout)
        {
            this.brick = brick;
            Port = port;
            colorMode = mode;
            //brick.BrickPi.Sensor[(int)Port].Type = (BrickSensorType)mode;
            brick.set_sensor_type((byte)Port, (SENSOR_TYPE)mode);
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


        public ColorSensorMode ColorMode
        {
            get { return colorMode; }
            set
            {
                if (value != colorMode)
                {
                    colorMode = value;
                    //brick.BrickPi.Sensor[(int)Port].Type = (BrickSensorType)colorMode;
                    brick.set_sensor_type((byte)Port, (SENSOR_TYPE)colorMode);
                }
            }
        }


        public BrickPortSensor Port
        {
            get; internal set;
        }

        public string GetSensorName()
        {
            return "NXT Color Sensor";
        }

        private void GetRawValues()
        {
            try
            {
                var ret = brick.get_sensor((byte)Port);
                for (int i = 0; i < rawValues.Length; i++)
                    //rawValues[i] = (short)brick.BrickPi.Sensor[(int)Port].Array[i];
                    rawValues[i] = (short)ret[i];
            }
            catch (Exception)
            { }

        }

        public int ReadRaw()
        {
            int val = 0;
            switch (colorMode)
            {
                case ColorSensorMode.Color:
                    val = (int)ReadColor();
                    break;
                case ColorSensorMode.Reflection:
                case ColorSensorMode.Green:
                case ColorSensorMode.Blue:
                    val = CalculateRawAverage();
                    break;
                case ColorSensorMode.Ambient:
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
                case ColorSensorMode.Ambient:
                    val = CalculateRawAverageAsPct();
                    break;
                case ColorSensorMode.Color:
                    val = (int)ReadColor();
                    break;
                case ColorSensorMode.Reflection:
                    val = CalculateRawAverageAsPct();
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
                GetRawValues();
                return (int)(rawValues[RedIndex] + rawValues[BlueIndex] + rawValues[GreenIndex]) / 3;
            }
            else
            {
                try
                {
                    //brick.BrickPi.Sensor[(int)Port].Value;
                    return brick.get_sensor((byte)Port)[0];
                }
                catch (Exception)
                {
                    return -1;
                }
                 
            }
        }

        private int CalculateRawAverageAsPct()
        {
            //Need to find out what is the ADC resolution
            //1023 is probablt not the correct one
            return (CalculateRawAverage() * 100) / 1023;
        }

        //public string ReadTest()
        //{
        //    GetRawValues();
        //    string ret = "";
        //    for (int i = 0; i < rawValues.Length; i++)
        //        ret += " " + rawValues[i];
        //    ret += " " + brick.BrickPi.Sensor[(int)Port].Value;
        //    return ret;

        //}

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
                    //color = (Color)brick.BrickPi.Sensor[(int)Port].Value;
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

        public void SelectNextMode()
        {
            colorMode = ColorMode.Next();
            return;
        }

        public void SelectPreviousMode()
        {
            colorMode = ColorMode.Previous();
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
