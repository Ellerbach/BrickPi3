﻿//////////////////////////////////////////////////////////
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
    /// IR channels
    /// </summary>
    public enum IRChannel
    {
#pragma warning disable
        One = 0, Two = 1, Three = 2, Four = 3
#pragma warning restore
    };


    /// <summary>
    /// Sensor mode when using a EV3 IR Sensor
    /// </summary>
    public enum IRMode
    {
        /// <summary>
        /// Use the IR sensor as a distance sensor
        /// </summary>
        Proximity = SENSOR_TYPE.EV3_INFRARED_PROXIMITY, //BrickSensorType.EV3_INFRARED_M0,

        /// <summary>
        /// Use the IR sensor to detect the location of the IR Remote
        /// </summary>
        Seek = SENSOR_TYPE.EV3_INFRARED_SEEK, //BrickSensorType.EV3_INFRARED_M1,

        /// <summary>
        /// Use the IR sensor to detect wich Buttons where pressed on the IR Remote
        /// </summary>
        Remote = SENSOR_TYPE.EV3_INFRARED_REMOTE //BrickSensorType.EV3_INFRARED_M2,
    };

    /// <summary>
    /// Class for IR beacon location.
    /// </summary>
    public sealed class BeaconLocation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MonoBrickFirmware.IO.IRBeaconLocation"/> class.
        /// </summary>
        /// <param name="location">Location.</param>
        /// <param name="distance">Distance.</param>
        public BeaconLocation(int location, int distance) { this.Location = location; this.Distance = distance; }

        /// <summary>
        /// Gets the location of the beacon ranging from minus to plus increasing clockwise when pointing towards the beacon
        /// </summary>
        /// <value>The location of the beacon.</value>
        public int Location { get; private set; } //was sbyte

        /// <summary>
        /// Gets the distance of the beacon in CM (0-100)
        /// </summary>
        /// <value>The distance to the beacon.</value>
        public int Distance { get; private set; } //was sbyte

    }

    public sealed class EV3IRSensor : INotifyPropertyChanged, ISensor
    {
        private Brick brick = null;
        private IRMode mode;

        /// <summary>
        /// Initialize an EV3 IR Sensor
        /// </summary>
        /// <param name="port">Sensor port</param>
        public EV3IRSensor(Brick brick, BrickPortSensor port) : this(brick, port, IRMode.Proximity, 1000)
        { }

        /// <summary>
        /// Initializes an EV3 IS Sensor
        /// </summary>
        /// <param name="mode">IR mode</param>
        public EV3IRSensor(Brick brick, BrickPortSensor port, IRMode mode) : this(brick, port, mode, 1000)
        { }

        /// <summary>
        /// Initialize an EV3 IR Sensor
        /// </summary>
        /// <param name="port">Sensor port</param>
        /// <param name="mode">IR mode</param>
        /// <param name="timeout">Period in millisecond to check sensor value changes</param>
        public EV3IRSensor(Brick brick, BrickPortSensor port, IRMode mode, int timeout)
        {
            this.brick = brick;
            Mode = mode;
            Port = port;
            Channel = IRChannel.One;
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
        /// Gets or sets the IR mode. 
        /// </summary>
        /// <value>The mode.</value>
        public IRMode Mode
        {
            get { return mode; }
            set
            {
                if (mode != value)
                {
                    mode = value;
                    //brick.BrickPi.Sensor[(int)Port].Type = (BrickSensorType)mode;
                    brick.set_sensor_type((byte)Port, (SENSOR_TYPE)mode);
                }
            }
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
                case IRMode.Proximity:
                    s = ReadDistance() + " cm";
                    break;
                case IRMode.Remote:
                    s = ReadRemoteCommand() + " on channel " + Channel;
                    break;
                case IRMode.Seek:
                    //BeaconLocation location = ReadBeaconLocation();
                    s = "Location: " + ReadBeaconLocation() + " Distance: TBD cm";
                    break;
            }
            return s;
        }

        /// <summary>
        /// Read the sensor value. The returned value depends on the mode. Distance in proximity mode. 
        /// Remote command number in remote mode. Beacon location in seek mode. 
        /// </summary>
        public int Read()
        {
            int value = 0;
            switch (Mode)
            {
                case IRMode.Proximity:
                    value = ReadDistance();
                    break;
                case IRMode.Remote:
                    value = (int)ReadRemoteCommand();
                    break;
                case IRMode.Seek:
                    value = (int)ReadBeaconLocation(); //if using class beacon: .Location
                    break;
            }
            return value;
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
                //depending on the sensor, number of returns are differents
                // SEEK = 8x8 bites, 4 x for each of the four channels, heading and distance
                // PROXIMITY = 1x8 bites
                // REMOTE = 4x8, button pressed per channel
                // SEEK
                int value = int.MaxValue;
                switch (mode)
                {
                    case IRMode.Proximity:
                        value = ret[0];
                        break;
                    case IRMode.Seek:
                        value = ret[0] + (ret[2] << 8) + (ret[4] << 16) + (ret[6] << 24);
                        break;
                    case IRMode.Remote:
                        value = ret[0] + (ret[1] << 8) + (ret[2] << 16) + (ret[3] << 24);
                        break;
                    default:
                        break;
                }
                return value;
            }
            catch (Exception)
            {
                return int.MaxValue;
            }            
        }

        /// <summary>
        /// Read the distance of the sensor in CM (0-100). This will change mode to proximity
        /// </summary>
        public int ReadDistance()
        {
            if (mode != IRMode.Proximity)
            {
                Mode = IRMode.Proximity;
            }
            //return brick.BrickPi.Sensor[(int)Port].Value;
            return ReadRaw();
        }

        /// <summary>
        /// Reads commands from the IR-Remote. This will change mode to remote
        /// </summary>
        /// <returns>The remote command.</returns>
        public byte ReadRemoteCommand()
        {
            if (Mode != IRMode.Remote)
            {
                Mode = IRMode.Remote;
            }
            //return (byte)((brick.BrickPi.Sensor[(int)Port].Value >> (int)Channel) & 0x0F);
            try
            {
                var ret = brick.get_sensor((byte)Port);
                return ret[(int)Channel];
            }
            catch (Exception)
            {
                return byte.MaxValue;
            }

        }

        /// <summary>
        /// Gets the beacon location. This will change the mode to seek
        /// </summary>
        /// <returns>The beacon location.</returns>
        public int ReadBeaconLocation()
        {
            var oldmode = Mode;
            if (Mode != IRMode.Seek)
            {
                Mode = IRMode.Seek;
            }
            //byte[] data = mUartSensor.ReadBytes(4 * 2);
            //return new BeaconLocation((sbyte)data[(int)Channel * 2], (sbyte)data[((int)Channel * 2) + 1]);
            //return brick.BrickPi.Sensor[(int)Port].Value;
            try
            {
                var ret = brick.get_sensor((byte)Port);
                if (Mode != oldmode)
                    Mode = oldmode;
                return (ret[(int)(Channel) * 2] + ret[(int)(Channel) * 2 + 1] >> 8);
            }
            catch (Exception)
            {
                if (Mode != oldmode)
                    Mode = oldmode;
                return int.MaxValue;
            }
        }

        /// <summary>
        /// Gets or sets the IR channel used for reading remote commands or beacon location
        /// </summary>
        /// <value>The channel.</value>
        public IRChannel Channel { get; set; }

        public BrickPortSensor Port
        {
            get; internal set;
        }

        public string GetSensorName()
        {
            return "EV3 IR";
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
            return Enum.GetNames(typeof(IRMode)).Length;

        }

        public string SelectedMode()
        {
            return Mode.ToString();
        }
    }
}
