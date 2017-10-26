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

    /// <summary>
	/// Sensor mode when using a Sonar sensor
	/// </summary>
    public enum UltraSonicMode
    {
        /// <summary>
        /// Result will be in centimeter
        /// </summary>
        Centimeter = SENSOR_TYPE.EV3_ULTRASONIC_CM, //BrickSensorType.EV3_US_M0, 

        /// <summary>
        /// Result will be in centi-inch
        /// </summary>
        Inch = SENSOR_TYPE.EV3_ULTRASONIC_INCHES, //BrickSensorType.EV3_US_M1, 

        /// <summary>
        /// Sensor is in listen mode
        /// </summary>
        Listen = SENSOR_TYPE.EV3_ULTRASONIC_LISTEN //BrickSensorType.EV3_US_M2, 
    };
    public sealed class NXTUltraSonicSensor: INotifyPropertyChanged, ISensor
    {
        private Brick brick = null;
        private UltraSonicMode sonarMode;

        /// <summary>
        /// Initialize a NXT Ultrasonic sensor
        /// </summary>
        /// <param name="port">Sensor port</param>
        public NXTUltraSonicSensor(Brick brick, BrickPortSensor port):this(brick,port, UltraSonicMode.Centimeter, 1000)
        { }

        /// <summary>
        /// Initialize a NXT Ultrasonic sensor
        /// </summary>
        /// <param name="port">Sensor port</param>
        /// <param name="mode">Ultrasonic mode</param>
        public NXTUltraSonicSensor(Brick brick, BrickPortSensor port, UltraSonicMode mode):this(brick, port, mode, 1000)
        { }

        /// <summary>
        /// Initialize a NXT Ultrasonic sensor
        /// </summary>
        /// <param name="port">Sensor port</param>
        /// <param name="mode">Ultrasonic mode</param>
        /// <param name="timeout">Period in millisecond to check sensor value changes</param>
        public NXTUltraSonicSensor(Brick brick, BrickPortSensor port, UltraSonicMode mode, int timeout)
        {
            this.brick = brick;
            Port = port;
            if (UltraSonicMode.Listen == mode)
                mode = UltraSonicMode.Centimeter;
            sonarMode = mode;
            //brick.BrickPi.Sensor[(int)Port].Type = (BrickSensorType)BrickSensorType.ULTRASONIC_CONT;
            brick.set_sensor_type((byte)Port, SENSOR_TYPE.NXT_ULTRASONIC);
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
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
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

        public BrickPortSensor Port
        {
            get; internal set;
        }
        private int value;
        private string valueAsString;

        /// <summary>
        /// Return the raw value of the sensor
        /// </summary>
        public int Value
        {
            //return the stored value, this sensor can't be read too often
            get { return value; }
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
            //return the stored value, this sensor can't be read too often
            get { return valueAsString; }
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

        /// <summary>
        /// Gets or sets the sonar mode.
        /// </summary>
        /// <value>
        /// The sonar mode 
        /// </value>
        public UltraSonicMode Mode
        {
            get { return sonarMode; }
            set { sonarMode = value; }
        }

        public string GetSensorName()
        {
            return "NXT Ultrasonic";
        }

        public string ReadAsString()
        {
            string s = ReadDistance().ToString();
            if (Mode == UltraSonicMode.Inch)
                s = s + " inch";
            else
                s = s + " cm";
            return s;

        }

        /// <summary>
        /// Read the distance in either centiinches or centimeter
        /// </summary>
        /// <returns>Distance as a float</returns>
        public float ReadDistance()
        {
            //int reading = brick.BrickPi.Sensor[(int)Port].Value;
            int reading = value; //ReadRaw();
            if (reading == int.MaxValue)
                return reading;
            if (Mode == UltraSonicMode.Inch)
                return (reading * 39370) / 100;
            return reading;
        }

        /// <summary>
        /// The raw value from the sensor
        /// </summary>
        /// <returns>Value as a int</returns>
        public int ReadRaw()
        {
            //return brick.BrickPi.Sensor[(int)Port].Value;
            try
            {
                var ret = brick.get_sensor((byte)Port);
                return ret[0];
            }
            catch (Exception)
            {
                return int.MaxValue;
            }
        }

        public void SelectNextMode()
        {
            Mode = Mode.Next();
            if (Mode == UltraSonicMode.Listen)
                Mode = Mode.Next();
            return;
        }

        public void SelectPreviousMode()
        {
            Mode = Mode.Previous();
            if (Mode == UltraSonicMode.Listen)
                Mode = Mode.Previous();
            return;
        }

        public int NumberOfModes()
        {
            return Enum.GetNames(typeof(UltraSonicMode)).Length - 1;//listen mode not supported
        }

        public string SelectedMode()
        {
            return Mode.ToString();
        }
    }
}
