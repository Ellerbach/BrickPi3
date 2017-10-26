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
    /// Sensor mode when using a NXT light sensor
    /// </summary>
    public enum LightMode
    {
        /// <summary>
        /// Use the lgith sensor to read reflected light
        /// </summary>
        Relection = SENSOR_TYPE.NXT_LIGHT_ON, //BrickSensorType.LIGHT_ON,

        /// <summary>
        /// Use the light sensor to detect the light intensity
        /// </summary>
        Ambient = SENSOR_TYPE.NXT_LIGHT_ON //BrickSensorType.LIGHT_OFF,
    };

    public sealed class NXTLightSensor : INotifyPropertyChanged, ISensor
    {
        private LightMode lightMode;
        private Brick brick = null;

        /// <summary>
        /// Initialize a NXT Light Sensor
        /// </summary>
        /// <param name="port">Sensor port</param>
        public NXTLightSensor(Brick brick, BrickPortSensor port) : this(brick, port, LightMode.Relection, 1000)
        { }

        /// <summary>
        /// Initialize a NXT Light Sensor
        /// </summary>
        /// <param name="port">Sensor port</param>
        /// <param name="mode">Light mode</param>
        public NXTLightSensor(Brick brick, BrickPortSensor port, LightMode mode) : this(brick, port, mode, 1000)
        { }

        /// <summary>
        /// Initialize a NXT Light Sensor
        /// </summary>
        /// <param name="port">Sensor port</param>
        /// <param name="mode">Light mode</param>
        /// <param name="timeout">Period in millisecond to check sensor value changes</param>
        public NXTLightSensor(Brick brick, BrickPortSensor port, LightMode mode, int timeout)
        {
            this.brick = brick;
            Port = port;
            lightMode = mode;
            CutOff = 512;
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

        /// <summary>
        /// This is used to change the level which indicate if the sensor
        /// is on something dark or clear
        /// </summary>
        public int CutOff { get; set; }

        public LightMode LightMode
        {
            get
            {
                return lightMode;
            }

            set
            {
                if (value != lightMode)
                {
                    lightMode = value;
                    //brick.BrickPi.Sensor[(int)Port].Type = (BrickSensorType)lightMode;
                    brick.set_sensor_type((byte)Port, (SENSOR_TYPE)lightMode);
                }
            }
        }

        public BrickPortSensor Port
        {
            get; internal set;
        }

        public void SelectNextMode()
        {
            LightMode = LightMode.Next();
            return;
        }

        public void SelectPreviousMode()
        {
            LightMode = LightMode.Previous();
            return;
        }

        public int NumberOfModes()
        {
            return Enum.GetNames(typeof(LightMode)).Length;
        }

        public string SelectedMode()
        {
            return LightMode.ToString();
        }

        public int ReadRaw()
        {
            //return brick.BrickPi.Sensor[(int)Port].Value;
            try
            {
                var ret = brick.get_sensor((byte)Port);
                return (ret[0] + (ret[1] >> 8));
            }
            catch (Exception)
            {
                return int.MaxValue;
            }
        }

        public string ReadAsString()
        {
            if (ReadRaw() > CutOff)
                return "Dark";
            return "Clear";
        }

        public string GetSensorName()
        {
            return "NXT Light";
        }
    }
}
