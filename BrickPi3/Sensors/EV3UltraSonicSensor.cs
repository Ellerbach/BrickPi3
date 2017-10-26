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
    public sealed class EV3UltraSonicSensor : INotifyPropertyChanged, ISensor
    {
        private Brick brick = null;
        private UltraSonicMode mode;

        /// <summary>
        /// Initialize an EV3 Ulrasonic sensor
        /// </summary>
        /// <param name="port">Sensor port</param>
        public EV3UltraSonicSensor(Brick brick, BrickPortSensor port) : this(brick, port, UltraSonicMode.Centimeter, 1000)
        { }

        /// <summary>
        /// Initialize an EV3 Ultrasonic sensor
        /// </summary>
        /// <param name="port">Sensor mode</param>
        /// <param name="usmode">Ultrasonic mode</param>
        public EV3UltraSonicSensor(Brick brick, BrickPortSensor port, UltraSonicMode usmode) : this(brick, port, usmode, 1000)
        { }

        /// <summary>
        /// Initialize an EV3 Ultrasonic Sensor
        /// </summary>
        /// <param name="port">Sensor port</param>
        /// <param name="usmode">Ultrasonic mode</param>
        /// <param name="timeout">Period in millisecond to check sensor value changes</param>
        public EV3UltraSonicSensor(Brick brick, BrickPortSensor port, UltraSonicMode usmode, int timeout)
        {
            this.brick = brick;
            Port = port;
            if (UltraSonicMode.Listen == mode)
                mode = UltraSonicMode.Centimeter;
            mode = usmode;
            //brick.BrickPi.Sensor[(int)Port].Type = (BrickSensorType)BrickSensorType.EV3_US_M0;
            brick.set_sensor_type((byte)Port, (SENSOR_TYPE)usmode);
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

        /// <summary>
        /// Gets or sets the Gyro mode. 
        /// </summary>
        /// <value>The mode.</value>
        public UltraSonicMode Mode
        {
            get { return mode; }
            set
            {
                if (mode != value)
                {
                    mode = value;
                    //brick.BrickPi.Sensor[(int)Port].Type = GetEV3Type(mode);
                    brick.set_sensor_type((byte)Port, GetEV3Type(mode));
                }
            }
        }

        private SENSOR_TYPE GetEV3Type(UltraSonicMode usmode)
        {
            //switch (usmode)
            //{
            //    case UltraSonicMode.Centimeter:
            //        return BrickSensorType.EV3_US_M0;
            //    case UltraSonicMode.Inch:
            //        return BrickSensorType.EV3_US_M1;
            //    case UltraSonicMode.Listen:
            //        return BrickSensorType.EV3_US_M2;
            //    default:
            //        return BrickSensorType.EV3_US_M0;
            //}
            return (SENSOR_TYPE)usmode;
        }


        public BrickPortSensor Port
        {
            get; internal set;
        }

        /// <summary>
        /// Reads the sensor value as a string.
        /// </summary>
        /// <returns>The value as a string</returns>
        public string ReadAsString()
        {
            string s = "";
            switch (mode)
            {
                case UltraSonicMode.Centimeter:
                    s = Read().ToString() + " cm";
                    break;
                case UltraSonicMode.Inch:
                    s = Read().ToString() + " inch";
                    break;
                case UltraSonicMode.Listen:
                    s = Read().ToString();
                    break;
            }
            return s;
        }

        /// <summary>
        /// Read the sensor value. Result depends on the mode
        /// </summary>
        /// <returns>Value as a int</returns>
        public int Read()
        {
            var ret = ReadRaw();
            if (ret == int.MaxValue)
                return ret;
            if (Mode == UltraSonicMode.Listen)
            {
                //if (brick.BrickPi.Sensor[(int)Port].Value != 0)
                if(ret!=0)
                    return 1;
                return 0;
            }
            //return brick.BrickPi.Sensor[(int)Port].Value;
            return ret;
        }

        /// <summary>
        /// Read the sensor value
        /// </summary>
        /// <returns>Value as a int</returns>
        public int ReadRaw()
        {
            //return brick.BrickPi.Sensor[(int)Port].Value;
            try
            {
                var ret = brick.get_sensor((byte)Port);
                switch (mode)
                {
                    case UltraSonicMode.Centimeter:
                    case UltraSonicMode.Inch:
                        return (ret[0] + (ret[1] >> 8));
                        break;
                    case UltraSonicMode.Listen:
                        return ret[0];
                        break;
                }
            }
            catch (Exception)
            {
            }
            return int.MaxValue;
        }

        public string GetSensorName()
        {
            return "EV3 Ultrasonic";
        }

        public void SelectNextMode()
        {
            Mode = Mode.Next();
            return;
        }

        public void SelectPreviousMode()
        {
            Mode = Mode.Previous();
            return;
        }

        public int NumberOfModes()
        {
            return Enum.GetNames(typeof(UltraSonicMode)).Length;

        }

        public string SelectedMode()
        {
            return Mode.ToString();
        }
    }
}
