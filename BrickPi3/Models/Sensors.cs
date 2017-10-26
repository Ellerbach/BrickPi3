/*# https://www.dexterindustries.com/GoPiGo/
# https://github.com/DexterInd/GoPiGo3
#
# Copyright (c) 2017 Dexter Industries
# Released under the MIT license (http://choosealicense.com/licenses/mit/).
# For more information see https://github.com/DexterInd/GoPiGo3/blob/master/LICENSE.md
# Driver written by Laurent Ellerbach, more information on https://github.com/ellerbach/BrickPi3
#
# C# drivers for the BrickPi3
*/

namespace BrickPi3.Models
{
    /// <summary>
    /// Sensor ports 1, 2, 3 and 4
    /// </summary>
    public enum SENSOR_PORT : byte
    {
        // Used to select the ports for sensors
        PORT_1 = 0x01,
        PORT_2 = 0x02,
        PORT_3 = 0x04,
        PORT_4 = 0x08,
    }

    /// <summary>
    /// Sensor ports 1, 2, 3 and 4
    /// </summary>
    public enum BrickPortSensor
    {
        PORT_S1 = 0x01,
        PORT_S2 = 0x02,
        PORT_S3 = 0x04,
        PORT_S4 = 0x08
    }

    /// <summary>
    /// MID_CLOCK = 0x01,   Send the clock pulse between reading and writing. Required by the NXT US sensor.
    /// PIN1_9V = 0x02,     9v pullup on pin 1
    /// SAME = 0x04,        Keep performing the same transaction e.g. keep polling a sensor
    /// </summary>
    public enum SENSOR_I2C_SETTINGS : byte
    {
        MID_CLOCK = 0x01,   //Send the clock pulse between reading and writing. Required by the NXT US sensor.
        PIN1_9V = 0x02,     //9v pullup on pin 1
        SAME = 0x04,        //Keep performing the same transaction e.g. keep polling a sensor
        ALLOW_STRETCH_ACK,
        ALLOW_STRETCH_ANY,
    }

    /// <summary>
    /// All type of supported sensors
    /// </summary>
    public enum SENSOR_TYPE : byte
    {
        NONE = 1,
        I2C,
        CUSTOM,
        TOUCH,
        NXT_TOUCH,
        EV3_TOUCH,
        NXT_LIGHT_ON,
        NXT_LIGHT_OFF,
        NXT_COLOR_RED,
        NXT_COLOR_GREEN,
        NXT_COLOR_BLUE,
        NXT_COLOR_FULL,
        NXT_COLOR_OFF,
        NXT_ULTRASONIC,
        EV3_GYRO_ABS,
        EV3_GYRO_DPS,
        EV3_GYRO_ABS_DPS,
        EV3_COLOR_REFLECTED,
        EV3_COLOR_AMBIENT,
        EV3_COLOR_COLOR,
        EV3_COLOR_RAW_REFLECTED,
        EV3_COLOR_COLOR_COMPONENTS,
        EV3_ULTRASONIC_CM,
        EV3_ULTRASONIC_INCHES,
        EV3_ULTRASONIC_LISTEN,
        EV3_INFRARED_PROXIMITY,
        EV3_INFRARED_SEEK,
        EV3_INFRARED_REMOTE
    }

    /// <summary>
    /// Maind state for data when returned by any of the get_ function
    /// Used internally by the brick engine
    /// </summary>
    public enum SENSOR_STATE : byte
    {
        VALID_DATA = 0,
        NOT_CONFIGURED,
        CONFIGURING,
        NO_DATA,
        I2C_ERROR
    }
    /// <summary>
    ///     Flags for use with SENSOR_TYPE.CUSTOM
    /// PIN1_9V
    ///     Enable 9V out on pin 1 (for LEGO NXT Ultrasonic sensor).
    /// PIN5_OUT
    ///     Set pin 5 state to output.Pin 5 will be set to input if this flag is not set.
    /// PIN5_STATE
    ///    If PIN5_OUT is set, this will set the state to output high, otherwise the state will
    ///    be output low.If PIN5_OUT is not set, this flag has no effect.
    /// PIN6_OUT
    ///    Set pin 6 state to output.Pin 6 will be set to input if this flag is not set.
    /// PIN6_STATE
    ///    If PIN6_OUT is set, this will set the state to output high, otherwise the state will
    ///    be output low.If PIN6_OUT is not set, this flag has no effect.
    /// PIN1_ADC
    ///    Enable the analog/digital converter on pin 1 (e.g. for NXT analog sensors).
    /// PIN6_ADC
    ///     Enable the analog/digital converter on pin 6.
    /// </summary>
    public enum SENSOR_CUSTOM
    {
        PIN1_9V = 0x0002,
        PIN5_OUT = 0x0010,
        PIN5_STATE = 0x0020,
        PIN6_OUT = 0x0100,
        PIN6_STATE = 0x0200,
        PIN1_ADC = 0x1000,
        PIN6_ADC = 0x4000
    }
}
