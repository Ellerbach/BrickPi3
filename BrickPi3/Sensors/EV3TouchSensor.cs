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

using BrickPi3.Models;
using System;
using System.ComponentModel;
using System.Threading;

namespace BrickPi3.Sensors
{
    public sealed class EV3TouchSensor : INotifyPropertyChanged, ISensor
    {
        private Brick brick = null;

        /// <summary>
        /// Initialise an EV3 Touch sensor
        /// </summary>
        /// <param name="port">Sensor port</param>
        public EV3TouchSensor(Brick brick, BrickPortSensor port):this(brick, port, 1000)
        { }
        /// <summary>
        /// Initialize an EV3 Touch Sensor
        /// </summary>
        /// <param name="port">Sensor port</param>
        /// <param name="timeout">Period in millisecond to check sensor value changes</param>
        public EV3TouchSensor(Brick brick, BrickPortSensor port, int timeout)
        {
            this.brick = brick;
            Port = port;
            //brick.BrickPi.Sensor[(int)Port].Type = BrickSensorType.EV3_TOUCH_0;
            brick.set_sensor_type((byte)Port, SENSOR_TYPE.EV3_TOUCH);
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
        /// Reads the sensor value as a string.
        /// </summary>
        /// <returns>The value as a string</returns>
        public string ReadAsString()
        {
            string s = "";
            if (IsPressed())
            {
                s = "Pressed";
            }
            else {
                s = "Not pressed";
            }
            return s;
        }

        /// <summary>
        /// Determines whether the touch sensor is pressed.
        /// </summary>
        /// <returns><c>true</c> if the sensor is pressed; otherwise, <c>false</c>.</returns>
        public bool IsPressed()
        {
            if (ReadRaw() > 0)
                return true;
            return false;
        }

        /// <summary>
        /// Reads the raw sensor value
        /// </summary>
        /// <returns>The raw.</returns>
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

        /// <summary>
        /// Return port
        /// </summary>
        public BrickPortSensor Port
        { get; internal set; }

        public string GetSensorName()
        {
            return "EV3 Touch";
        }

        public int NumberOfModes()
        {
            return 1;
        }

        public string SelectedMode()
        {
            return "Analog";
        }

        public void SelectNextMode()
        {
            return;
        }

        public void SelectPreviousMode()
        {
            return;
        }
    }
}
