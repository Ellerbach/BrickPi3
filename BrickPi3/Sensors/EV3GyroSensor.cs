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
    /// <summary>
    /// Sensor modes when using a EV3 Gyro sensor
    /// </summary>
    public enum GyroMode
    {
#pragma warning disable
        /// <summary>
        /// Result will be in degrees
        /// </summary>
        Angle = SENSOR_TYPE.EV3_GYRO_ABS, //BrickSensorType.EV3_GYRO_M0,
        /// <summary>
        /// Result will be in degrees per second
        /// </summary>
        AngularVelocity = SENSOR_TYPE.EV3_GYRO_DPS //BrickSensorType.EV3_GYRO_M1,
#pragma warning restore
    };

    public sealed class EV3GyroSensor : INotifyPropertyChanged, ISensor
    {
        private Brick brick = null;
        private GyroMode gmode;

        /// <summary>
        /// Initialize an EV3 Gyro Sensor
        /// </summary>
        /// <param name="port">Sensor port</param>
        public EV3GyroSensor(Brick brick, BrickPortSensor port) : this(brick, port, GyroMode.Angle)
        { }

        /// <summary>
        /// Initialize an EV3 Gyro Sensor
        /// </summary>
        /// <param name="port">Sensor port</param>
        /// <param name="mode">Gyro mode</param>
        public EV3GyroSensor(Brick brick, BrickPortSensor port, GyroMode mode) : this(brick, port, mode, 1000)
        { }

        /// <summary>
        /// Initialize an EV3 Gyro Sensor
        /// </summary>
        /// <param name="port">Sensor port</param>
        /// <param name="mode">Gyro mode</param>
        /// <param name="timeout">Period in millisecond to check sensor value changes</param>
        public EV3GyroSensor(Brick brick, BrickPortSensor port, GyroMode mode, int timeout)
        {
            this.brick = brick;
            Port = port;
            gmode = mode;
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
        public GyroMode Mode
        {
            get { return gmode; }
            set
            {
                if (gmode != value)
                {
                    gmode = value;
                    //brick.BrickPi.Sensor[(int)Port].Type = (BrickSensorType)gmode;
                    brick.set_sensor_type((byte)Port, (SENSOR_TYPE)gmode);
                }
            }
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
            switch (gmode)
            {
                case GyroMode.Angle:
                    s = Read().ToString() + " degree";
                    break;
                case GyroMode.AngularVelocity:
                    s = Read().ToString() + " deg/sec";
                    break;
            }
            return s;
        }

        /// <summary>
        /// Reset the sensor
        /// </summary>
        private async void Reset()
        {
            if (Mode == GyroMode.Angle)
            {
                Mode = GyroMode.AngularVelocity;
                //System.Threading.Thread.Sleep(100);
                await Task.Delay(100);
                Mode = GyroMode.Angle;
            }
            else
            {
                Mode = GyroMode.Angle;
                //System.Threading.Thread.Sleep(100);
                await Task.Delay(100);
                Mode = GyroMode.AngularVelocity;
            }
        }

        /// <summary>
        /// Get the number of rotations (a rotation is 360 degrees) - only makes sense when in angle mode
        /// </summary>
        /// <returns>The number of rotations</returns>
        public int RotationCount()
        {
            var ret = ReadRaw();
            if (ret == int.MaxValue)
                return ret;
            if (Mode == GyroMode.Angle)
            {
                //return brick.BrickPi.Sensor[(int)Port].Value / 360;
                return ret / 360;
            }
            return 0;
        }


        /// <summary>
        /// Read the gyro sensor value. The returned value depends on the mode. 
        /// </summary>
        public int Read()
        {
            var ret = ReadRaw();
            if (ret == int.MaxValue)
                return ret;
            if (Mode == GyroMode.Angle)
            {
                //return brick.BrickPi.Sensor[(int)Port].Value % 360;
                return ret % 360;
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
            try
            {
                //return brick.BrickPi.Sensor[(int)Port].Value;
                var ret = brick.get_sensor((byte)Port);
                return (ret[0] + (ret[1] >> 8));
            }
            catch (Exception)
            {
                return int.MaxValue;
            }           
        }

        public string GetSensorName()
        {
            return "EV3 Gyro";
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
            return Enum.GetNames(typeof(GyroMode)).Length;

        }

        public string SelectedMode()
        {
            return Mode.ToString();
        }
    }
}
