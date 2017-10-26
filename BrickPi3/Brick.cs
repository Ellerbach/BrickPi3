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

using BrickPi3.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Spi;
using static BrickPi3.SPIExceptions;

namespace BrickPi3
{
    public sealed class Brick
    {

        // To store the Sensor types as well as date to be sent when it's an I2C sensor
        SENSOR_TYPE[] SensorType = { SENSOR_TYPE.NONE, SENSOR_TYPE.NONE, SENSOR_TYPE.NONE, SENSOR_TYPE.NONE };
        byte[] I2CInBytes = { 0, 0, 0, 0 };

        //Initernals to initalize the SPI
        static private SpiDevice BrickPiSPI = null;
        private const int CHIP_SELECT_lINE = 1;
        private const string DEVICE_FRIENDLY_NAME = "SPI0";
        private BrickPiVoltage brickPiVoltage = new BrickPiVoltage();

        #region properties
        // used to store the SPI Address
        // used mainly when multiple bricks in a raw or not the default SPI address
        // up to 254 addresses supported
        public byte SPI_Address { get; set; }
        public BrickPiInfo BrickPi3Info { get; internal set; }
        public BrickPiVoltage BrickPi3Voltage
        {
            get
            {
                brickPiVoltage.Voltage3V3 = get_voltage_3v3();
                brickPiVoltage.Voltage5V = get_voltage_5v();
                brickPiVoltage.Voltage9V = get_voltage_9v();
                brickPiVoltage.VoltageBattery = get_voltage_battery();
                return brickPiVoltage;
            }
        }
        #endregion

        #region initi and reset
        public void InitSPI(byte spi_address = 1)
        {
            InitSPI_async(spi_address).Wait();
        }

        private async Task InitSPI_async(byte spi_address = 1)
        {
            try
            {
                SPI_Address = spi_address;
                var settings = new SpiConnectionSettings(CHIP_SELECT_lINE);
                settings.ClockFrequency = 500000;                              /* 500K                     */
                settings.Mode = SpiMode.Mode0;  //http://tightdev.net/SpiDev_Doc.pdf
                settings.DataBitLength = 8;
                settings.SharingMode = SpiSharingMode.Exclusive;
                // as the SPI is a static, checking if it has already be initialised
                if (BrickPiSPI == null)
                {
                    string aqs = SpiDevice.GetDeviceSelector();                     /* Get a selector string that will return all SPI controllers on the system */
                    var dis = await DeviceInformation.FindAllAsync(aqs);            /* Find the SPI bus controller devices with our selector string             */
                    BrickPiSPI = await SpiDevice.FromIdAsync(dis[0].Id, settings);    /* Create an SpiDevice with our bus controller and SPI settings             */
                    if (BrickPiSPI == null)
                    {
                        Debug.WriteLine(string.Format(
                            "SPI Controller {0} is currently in use by " +
                            "another application. Please ensure that no other applications are using SPI.",
                            dis[0].Id));
                        return;
                    }
                }
                BrickPi3Info = new BrickPiInfo();
                BrickPi3Info.Manufacturer = get_manufacturer();
                BrickPi3Info.Board = get_board();
                BrickPi3Info.HardwareVersion = get_version_hardware();
                BrickPi3Info.SoftwareVersion = get_version_firmware();
                BrickPi3Info.Id = get_id();
            }
            catch (Exception ex)
            {
                Debug.Write($"Exception: {ex.Message}");
            }
        }

        public void reset_all()
        {
            // Reset the BrickPi.Set all the sensors' type to NONE, set the motors to float, and motors' limits and constants to default, and return control of the LED to the firmware.
            // reset all sensors
            set_sensor_type((byte)(SENSOR_PORT.PORT_1) + (byte)(SENSOR_PORT.PORT_2) + (byte)(SENSOR_PORT.PORT_3) + (byte)(SENSOR_PORT.PORT_4), SENSOR_TYPE.NONE);
            // turn off all motors
            byte allmotors = (byte)(MOTOR_PORT.PORT_A) + (byte)(MOTOR_PORT.PORT_B) + (byte)(MOTOR_PORT.PORT_C) + (byte)(MOTOR_PORT.PORT_D);
            set_motor_power(allmotors, (byte)MOTOR_SPEED.FLOAT);
            // reset motor limits
            set_motor_limits(allmotors);
            // reset motor kP and kD constants
            set_motor_position_kp(allmotors);
            set_motor_position_kd(allmotors);
            // return the LED to the control of the FW
            set_led(255);
        }
        #endregion

        #region SPI transfer
        public byte[] spi_transfer_array(byte[] data_out)
        {
            //Conduct a SPI transaction
            //Keyword arguments:
            //data_out-- a list of bytes to send.The length of the list will determine how many bytes are transferred.
            //Returns a list of the bytes read.
            byte[] result = new byte[data_out.Length];
            BrickPiSPI.TransferFullDuplex(data_out, result);
            return result;
        }

        public int spi_read_32(SPI_MESSAGE_TYPE MessageType)
        {

            /*  Read a 32 - bit value over SPI
               Keyword arguments:
               MessageType-- the SPI message type
               Returns touple:
               value, error
            */
            int retVal = -1;
            byte[] outArray = { SPI_Address, (byte)MessageType, 0, 0, 0, 0, 0, 0 };
            byte[] reply = spi_transfer_array(outArray);

            if (reply[3] == 0xA5)
            {
                retVal = (int)(reply[4] << 24) | (reply[5] << 16) | (reply[6] << 8) | reply[7];
            }
            else
            {
                throw new IOError("No SPI response");
            }

            return retVal;
        }

        public int spi_read_16(SPI_MESSAGE_TYPE MessageType)
        {

            /*  Read a 16 - bit value over SPI
               Keyword arguments:
               MessageType-- the SPI message type
               Returns touple:
               value, error
            */
            int retVal = -1;
            byte[] outArray = { SPI_Address, (byte)MessageType, 0, 0, 0, 0, };
            byte[] reply = spi_transfer_array(outArray);

            if (reply[3] == 0xA5)
            {
                retVal = (int)(reply[4] << 8) | reply[5];
            }
            else
            {
                throw new IOError("No SPI response");
            }

            return retVal;
        }

        public void spi_write_8(SPI_MESSAGE_TYPE MessageType, int Value)
        {
            // Send a 8 - bit value over SPI
            // Keyword arguments:
            // MessageType-- the SPI message type
            // Value-- the value to be sent
            byte[] outArray = { SPI_Address, (byte)MessageType, (byte)(Value & 0xFF) };
            spi_transfer_array(outArray);
        }

        public void spi_write_16(SPI_MESSAGE_TYPE MessageType, int Value)
        {

            // Send a 16 - bit value over SPI
            // Keyword arguments:
            // MessageType-- the SPI message type
            // Value-- the value to be sent
            byte[] outArray = { SPI_Address, (byte)MessageType, (byte)((Value >> 8) & 0xFF), (byte)(Value & 0xFF) };
            spi_transfer_array(outArray);
        }

        public void spi_write_24(SPI_MESSAGE_TYPE MessageType, int Value)
        {
            //Send a 24 - bit value over SPI
            //  Keyword arguments:
            //    MessageType-- the SPI message type
            //Value-- the value to be sent
            byte[] outArray = { SPI_Address, (byte)MessageType, (byte)((Value >> 16) & 0xFF), (byte)((Value >> 8) & 0xFF), (byte)(Value & 0xFF) };
            spi_transfer_array(outArray);
        }

        public void spi_write_32(SPI_MESSAGE_TYPE MessageType, int Value)
        {

            // Send a 32 - bit value over SPI
            // Keyword arguments:
            // MessageType-- the SPI message type
            // Value-- the value to be sent
            byte[] outArray = { SPI_Address, (byte)MessageType, (byte)((Value >> 24) & 0xFF), (byte)((Value >> 16) & 0xFF), (byte)((Value >> 8) & 0xFF), (byte)(Value & 0xFF) };
            spi_transfer_array(outArray);
        }
        #endregion

        #region Borad elements
        public string get_manufacturer()
        {
            /*
            Read the 20 charactor BrickPi3 manufacturer name
            Returns touple:
            BrickPi3 manufacturer name string, error
            */
            string retVal = string.Empty;


            byte[] outArray = {SPI_Address, (byte)SPI_MESSAGE_TYPE.GET_MANUFACTURER,
                  0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            byte[] reply = spi_transfer_array(outArray);
            if (reply[3] != 0xA5)
            {
                throw new IOError("No SPI response");
            }
            else
            {
                for (int ndx = 4; ndx < 24; ++ndx)
                {
                    if (reply[ndx] != 0)
                    {
                        retVal += (char)reply[ndx];
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return retVal;
        }

        public string get_board()
        {

            /*Read the 20 charactor BrcikPi3 board name
              Returns touple:
              BrcikPi3 board name string, error
              */
            string retVal = string.Empty;

            byte[] outArray = {SPI_Address, (byte)SPI_MESSAGE_TYPE.GET_NAME,
                    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            byte[] reply = spi_transfer_array(outArray);

            if (reply[3] == 0xA5)
            {
                for (int ndx = 4; ndx < 24; ++ndx)
                {
                    if (reply[ndx] != 0)
                    {
                        retVal += (char)(reply[ndx]);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            else
            {
                throw new IOError("No SPI response");
            }
            return retVal;
        }

        public string get_version_hardware()
        {

            /*Read the hardware version
              Returns touple:
              hardware version, error
              */
            string retVal = string.Empty;
            int version = spi_read_32(SPI_MESSAGE_TYPE.GET_HARDWARE_VERSION);
            retVal = string.Format("{0}.{1}.{2}", (version / 1000000), ((version / 1000) % 1000), (version % 1000));
            return retVal;
        }

        public string get_id()
        {

            /*  Read the 128 - bit BrcikPi3 hardware serial number

               Returns touple:
               serial number as 32 char HEX formatted string, error

             */
            string retVal = string.Empty;
            byte[] outArray = {SPI_Address, (byte)SPI_MESSAGE_TYPE.GET_ID,
                     0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            byte[] reply = spi_transfer_array(outArray);

            if (reply[3] == 0xA5)
            {
                retVal = reply[4].ToString("X2") + reply[5].ToString("X2") + reply[6].ToString("X2") +
                         reply[7].ToString("X2") + reply[8].ToString("X2") + reply[9].ToString("X2") +
                         reply[10].ToString("X2") + reply[11].ToString("X2") +
                         reply[12].ToString("X2") + reply[13].ToString("X2") + reply[14].ToString("X2") +
                         reply[15].ToString("X2") + reply[16].ToString("X2") + reply[17].ToString("X2") +
                         reply[18].ToString("X2") + reply[19].ToString("X2");
            }
            else
            {
                throw new IOError("No SPI response");
            }
            return retVal;
        }

        public string get_version_firmware()
        {
            /*Read the firmware version
              Returns touple:
              firmware version, error
              */
            string retVal = string.Empty;
            int version = spi_read_32(SPI_MESSAGE_TYPE.GET_FIRMWARE_VERSION);
            retVal = string.Format("{0}.{1}.{2}", (version / 1000000), ((version / 1000) % 1000), (version % 1000));
            return retVal;
        }

        public void set_address(byte address, string id)
        {
            //Set the SPI address of the BrickPi3
            //Keyword arguments:
            //address -- the new SPI address to use(1 to 255)
            //id -- the BrickPi3's unique serial number ID (so that the address can be set while multiple BrickPi3s are stacked on a Raspberry Pi).
            byte[] id_arr = new byte[16];
            if (id.Length != 32)
            {
                if (id != "")
                    throw new IOError("brickpi3.set_address error: wrong serial number id length. Must be a 32-digit hex string.");

            }
            if (id.Length == 32)
            {
                var isok = false;
                for (int i = 0; i < 16; i++)
                {
                    isok = byte.TryParse(id.Substring(i * 2, i * 2 + 1), System.Globalization.NumberStyles.HexNumber, null as IFormatProvider, out id_arr[i]);
                    if (!isok)
                        break;
                }
                if (!isok)
                    throw new IOError("brickpi3.set_address error: unknown serial number id problem. Make sure to use a valid 32-digit hex string serial number.");
            }
            byte[] outArray = new byte[19];
            outArray[0] = 0;
            outArray[1] = (byte)SPI_MESSAGE_TYPE.SET_ADDRESS;
            outArray[2] = address;
            for (int i = 0; i < 16; i++)
                outArray[3 + i] = id_arr[i];
            var ret = spi_transfer_array(outArray);
            if (ret[3] == 0xA5)
                SPI_Address = address;
        }

        public void set_led(byte percent)
        {
            /*  Set an LED
              Keyword arguments:
              c# note:  byte values are always >= 0 and <= 255, so no range checking is made.
              */

            byte[] outArray = { SPI_Address, (byte)SPI_MESSAGE_TYPE.SET_LED, percent };
            var reply = spi_transfer_array(outArray);
        }

        public double get_voltage_3v3()
        {
            // Get the 3.3v circuit voltage
            // Returns:
            // 3.3v circuit voltage
            var value = spi_read_16(SPI_MESSAGE_TYPE.GET_VOLTAGE_3V3);
            return (value / 1000.0);
        }

        public double get_voltage_5v()
        {
            // Get the 5v circuit voltage
            // Returns:
            // 5v circuit voltage
            var value = spi_read_16(SPI_MESSAGE_TYPE.GET_VOLTAGE_5V);
            return (value / 1000.0);
        }

        public double get_voltage_9v()
        {
            // Get the 9v circuit voltage
            // Returns:
            // 9v circuit voltage
            var value = spi_read_16(SPI_MESSAGE_TYPE.GET_VOLTAGE_9V);
            return (value / 1000.0);
        }

        public double get_voltage_battery()
        {
            // Get the battery voltage
            // Returns:
            // battery voltage
            var value = spi_read_16(SPI_MESSAGE_TYPE.GET_VOLTAGE_VCC);
            return (value / 1000.0);
        }

        #endregion

        #region Sensors
        public byte[] get_sensor(byte port)
        {
            //    Read a sensor value
            //Keyword arguments:
            //port-- The sensor port(one at a time).PORT_1, PORT_2, PORT_3, or PORT_4.
            //Returns the value(s) for the specified sensor.
            //    The following sensor types each return a single value:
            //        NONE---------------------- - 0
            //        TOUCH---------------------- 0 or 1(released or pressed)
            //        NXT_TOUCH------------------ 0 or 1(released or pressed)
            //        EV3_TOUCH------------------ 0 or 1(released or pressed)
            //        NXT_ULTRASONIC------------ - distance in CM
            //        NXT_LIGHT_ON  --------------reflected light
            //       NXT_LIGHT_OFF --------------ambient light
            //      NXT_COLOR_RED --------------red reflected light
            //        NXT_COLOR_GREEN------------ green reflected light
            //       NXT_COLOR_BLUE -------------blue reflected light
            //        NXT_COLOR_OFF-------------- ambient light
            //        EV3_GYRO_ABS-------------- - absolute rotation position in degrees
            //        EV3_GYRO_DPS ---------------rotation rate in degrees per second
            //       EV3_COLOR_REFLECTED --------red reflected light
            //        EV3_COLOR_AMBIENT---------- ambient light
            //        EV3_COLOR_COLOR------------ detected color
            //        EV3_ULTRASONIC_CM---------- distance in CM
            //       EV3_ULTRASONIC_INCHES ------distance in inches
            //      EV3_ULTRASONIC_LISTEN ------0 or 1(no other ultrasonic sensors or another ultrasonic sensor detected)
            //        EV3_INFRARED_PROXIMITY---- - distance 0 - 100 %
            //    The following sensor types each return a list of values
            //        CUSTOM ---------------------Pin 1 ADC(5v scale from 0 to 4095), Pin 6 ADC(3.3v scale from 0 to 4095), Pin 5 digital, Pin 6 digital
            //     I2C ------------------------the I2C bytes read
            //    NXT_COLOR_FULL -------------detected color, red light reflected, green light reflected, blue light reflected, ambient light
            //        EV3_GYRO_ABS_DPS---------- - absolute rotation position in degrees, rotation rate in degrees per second
            //        EV3_COLOR_RAW_REFLECTED ----red reflected light, unknown value(maybe a raw ambient value ?)
            //        EV3_COLOR_COLOR_COMPONENTS - red reflected light, green reflected light, blue reflected light, unknown value(maybe a raw value ?)
            //        EV3_INFRARED_SEEK---------- a list for each of the four channels.For each channel heading(-25 to 25), distance(-128 or 0 to 100)
            //       EV3_INFRARED_REMOTE-------- a list for each of the four channels.For each channel red up, red down, blue up, blue down, boadcast

            byte port_index = 0;

            SPI_MESSAGE_TYPE message_type = SPI_MESSAGE_TYPE.NONE;
            if (port == (byte)SENSOR_PORT.PORT_1)
            {
                message_type = SPI_MESSAGE_TYPE.GET_SENSOR_1;
                port_index = 0;
            }
            else if (port == (byte)SENSOR_PORT.PORT_2)
            {
                message_type = SPI_MESSAGE_TYPE.GET_SENSOR_2;
                port_index = 1;
            }
            else if (port == (byte)SENSOR_PORT.PORT_3)
            {
                message_type = SPI_MESSAGE_TYPE.GET_SENSOR_3;
                port_index = 2;
            }
            else if (port == (byte)SENSOR_PORT.PORT_4)
            {
                message_type = SPI_MESSAGE_TYPE.GET_SENSOR_4;
                port_index = 3;
            }
            else
                throw new IOError("get_sensor error. Must be one sensor port at a time. PORT_1, PORT_2, PORT_3, or PORT_4.");

            List<byte> outArray = new List<byte>();
            byte[] reply;

            if (SensorType[port_index] == SENSOR_TYPE.CUSTOM)
            {
                outArray.AddRange(new byte[] { SPI_Address, (byte)message_type, 0, 0, 0, 0, 0, 0, 0, 0 });
                reply = spi_transfer_array(outArray.ToArray());
                if (reply[3] == 0xA5)
                {
                    if ((reply[4] == (int)SensorType[port_index]) && (reply[5] == (int)SENSOR_STATE.VALID_DATA))
                        return new byte[] { (byte)(((reply[8] & 0x0F) << 8) | reply[9]), (byte)(((reply[8] >> 4) & 0x0F) | (reply[7] << 4)), (byte)(reply[6] & 0x01), (byte)((reply[6] >> 1) & 0x01) };
                    else
                        throw new SensorError("get_sensor error: Invalid sensor data");
                }
                else
                    throw new IOError("get_sensor error: No SPI response");
            }
            else if (SensorType[port_index] == SENSOR_TYPE.I2C)
            {
                outArray.AddRange(new byte[] { SPI_Address, (byte)message_type, 0, 0, 0, 0 });
                for (int b = 0; b < I2CInBytes[port_index]; b++)
                    outArray.Add(0);
                reply = spi_transfer_array(outArray.ToArray());
                if (reply[3] == 0xA5)
                {
                    if ((reply[4] == (int)SensorType[port_index]) && (reply[5] == (int)SENSOR_STATE.VALID_DATA) && ((reply.Length - 6) == I2CInBytes[port_index]))
                    {
                        List<byte> values = new List<byte>();
                        for (int b = 6; b < I2CInBytes[port_index]; b++)
                            values.Add(reply[b]);
                        return values.ToArray();
                    }
                    else
                        throw new SensorError("get_sensor error: Invalid sensor data");
                }
                else
                    throw new IOError("get_sensor error: No SPI response");
            }
            else if ((SensorType[port_index] == SENSOR_TYPE.TOUCH)
                || (SensorType[port_index] == SENSOR_TYPE.NXT_TOUCH)
                || (SensorType[port_index] == SENSOR_TYPE.EV3_TOUCH)
                || (SensorType[port_index] == SENSOR_TYPE.NXT_ULTRASONIC)
                || (SensorType[port_index] == SENSOR_TYPE.EV3_COLOR_REFLECTED)
                || (SensorType[port_index] == SENSOR_TYPE.EV3_COLOR_AMBIENT)
                || (SensorType[port_index] == SENSOR_TYPE.EV3_COLOR_COLOR)
                || (SensorType[port_index] == SENSOR_TYPE.EV3_ULTRASONIC_LISTEN)
                || (SensorType[port_index] == SENSOR_TYPE.EV3_INFRARED_PROXIMITY))
            {
                outArray.AddRange(new byte[] { SPI_Address, (byte)message_type, 0, 0, 0, 0, 0 });
                reply = spi_transfer_array(outArray.ToArray());
                if (reply[3] == 0xA5)
                {
                    // if (reply[4] == self.SensorType[port_index] or (self.SensorType[port_index] == self.SENSOR_TYPE.TOUCH and (reply[4] == self.SENSOR_TYPE.NXT_TOUCH or reply[4] == self.SENSOR_TYPE.EV3_TOUCH))) and reply[5] == self.SENSOR_STATE.VALID_DATA

                    if (((reply[4] == (int)SensorType[port_index]) || ((SensorType[port_index] == SENSOR_TYPE.TOUCH) && ((reply[4] == (int)SENSOR_TYPE.NXT_TOUCH)
                        || (reply[4] == (int)SENSOR_TYPE.EV3_TOUCH)))) && (reply[5] == (int)SENSOR_STATE.VALID_DATA))
                        return new byte[] { reply[6] };
                    else
                        throw new SensorError("get_sensor error: Invalid sensor data");
                }
                else
                    throw new IOError("get_sensor error: No SPI response");
            }
            else if (SensorType[port_index] == SENSOR_TYPE.NXT_COLOR_FULL)
            {
                outArray.AddRange(new byte[] { SPI_Address, (byte)message_type, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
                reply = spi_transfer_array(outArray.ToArray());
                if (reply[3] == 0xA5)
                {
                    if ((reply[4] == (int)SensorType[port_index]) && (reply[5] == (int)SENSOR_STATE.VALID_DATA))

                        return new byte[] { reply[6], (byte)((reply[7] << 2) | ((reply[11] >> 6) & 0x03)), (byte)((reply[8] << 2) | ((reply[11] >> 4) & 0x03)),
                            (byte)((reply[9] << 2) | ((reply[11] >> 2) & 0x03)), (byte)((reply[10] << 2) | (reply[11] & 0x03)) };
                    else

                        throw new SensorError("get_sensor error: Invalid sensor data");
                }
                else

                    throw new IOError("get_sensor error: No SPI response");
            }
            else if ((SensorType[port_index] == SENSOR_TYPE.NXT_LIGHT_ON)
              || (SensorType[port_index] == SENSOR_TYPE.NXT_LIGHT_OFF)
              || (SensorType[port_index] == SENSOR_TYPE.NXT_COLOR_RED)
              || (SensorType[port_index] == SENSOR_TYPE.NXT_COLOR_GREEN)
              || (SensorType[port_index] == SENSOR_TYPE.NXT_COLOR_BLUE)
              || (SensorType[port_index] == SENSOR_TYPE.NXT_COLOR_OFF)
              || (SensorType[port_index] == SENSOR_TYPE.EV3_GYRO_ABS)
              || (SensorType[port_index] == SENSOR_TYPE.EV3_GYRO_DPS)
              || (SensorType[port_index] == SENSOR_TYPE.EV3_ULTRASONIC_CM)
              || (SensorType[port_index] == SENSOR_TYPE.EV3_ULTRASONIC_INCHES))
            {
                outArray.AddRange(new byte[] { SPI_Address, (byte)message_type, 0, 0, 0, 0, 0, 0 });

                reply = spi_transfer_array(outArray.ToArray());

                if (reply[3] == 0xA5)

                {
                    if ((reply[4] == (int)SensorType[port_index]) && (reply[5] == (int)SENSOR_STATE.VALID_DATA))
                    {
                        var value = (int)((reply[6] << 8) | reply[7]);
                        if (((SensorType[port_index] == SENSOR_TYPE.EV3_GYRO_ABS)
                        || (SensorType[port_index] == SENSOR_TYPE.EV3_GYRO_DPS))
                        && ((value & 0x8000) > 0))
                            value = value - 0x10000;
                        else if ((SensorType[port_index] == SENSOR_TYPE.EV3_ULTRASONIC_CM)
                          || (SensorType[port_index] == SENSOR_TYPE.EV3_ULTRASONIC_INCHES))
                            value = value / 10;
                        //convert back the value to a byte array
                        return new byte[] { (byte)((value >> 8) & 0xFF), (byte)(value & 0xFF) };
                    }
                    else
                        throw new SensorError("get_sensor error: Invalid sensor data");
                }
                else
                    throw new IOError("get_sensor error: No SPI response");
            }
            else if ((SensorType[port_index] == SENSOR_TYPE.EV3_COLOR_RAW_REFLECTED)
              || (SensorType[port_index] == SENSOR_TYPE.EV3_GYRO_ABS_DPS))
            {
                outArray.AddRange(new byte[] { SPI_Address, (byte)message_type, 0, 0, 0, 0, 0, 0, 0, 0 });
                reply = spi_transfer_array(outArray.ToArray());
                if (reply[3] == 0xA5)
                {
                    if ((reply[4] == (int)SensorType[port_index]) && (reply[5] == (int)SENSOR_STATE.VALID_DATA))
                    {
                        ushort[] results = new ushort[] { (ushort)((reply[6] << 8) | reply[7]), (ushort)((reply[8] << 8) | reply[9]) };
                        if (SensorType[port_index] == SENSOR_TYPE.EV3_GYRO_ABS_DPS)
                            //TODO: check it is necessary to do the convertion and not just pass the byte array and do it later
                            for (int r = 0; r < results.Length; r++)
                                if (results[r] >= 0x8000)
                                    results[r] = (ushort)(results[r] - 0x10000);
                        //convert back the value to a byte array
                        //we know the length is 2
                        return new byte[] { (byte)((results[1] >> 8) & 0xFF), (byte)(results[1] & 0xFF), (byte)((results[0] >> 8) & 0xFF), (byte)(results[0] & 0xFF) };
                    }
                    else
                        throw new SensorError("get_sensor error: Invalid sensor data");
                }
                else
                    throw new IOError("get_sensor error: No SPI response");
            }
            else if ((SensorType[port_index] == SENSOR_TYPE.EV3_COLOR_COLOR_COMPONENTS))
            {
                outArray.AddRange(new byte[] { SPI_Address, (byte)message_type, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
                reply = spi_transfer_array(outArray.ToArray());
                if (reply[3] == 0xA5)
                {
                    if ((reply[4] == (int)SensorType[port_index]) || (reply[5] == (int)SENSOR_STATE.VALID_DATA))

                        return new byte[] { reply[6], reply[7], reply[8], reply[9], reply[10], reply[11], reply[12], reply[13] };
                    else

                        throw new SensorError("get_sensor error: Invalid sensor data");
                }
                else
                    throw new IOError("get_sensor error: No SPI response");
            }
            else if (SensorType[port_index] == SENSOR_TYPE.EV3_INFRARED_SEEK)

            {
                outArray.AddRange(new byte[] { SPI_Address, (byte)message_type, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });

                reply = spi_transfer_array(outArray.ToArray());

                if (reply[3] == 0xA5)
                {
                    if ((reply[4] == (int)SensorType[port_index]) && (reply[5] == (int)SENSOR_STATE.VALID_DATA))

                    {//create a simple table of bytes, not a byte byte arrays like in Python
                        byte[] results = new byte[] { (reply[6]), (reply[7]), (reply[8]), (reply[9]), (reply[10]), (reply[11]), (reply[12]), (reply[13]) };
                        //No convertion done at this level, convertion should be done when receiving the results
                        //for c in range(len(results)):
                        //    for v in range(len(results[c])):
                        //        if results[c][v] >= 0x80:
                        //             results[c][v] = results[c][v] - 0x100
                        return results;
                    }
                    else
                        throw new SensorError("get_sensor error: Invalid sensor data");
                }
                else
                    throw new IOError("get_sensor error: No SPI response");
            }
            else if (SensorType[port_index] == SENSOR_TYPE.EV3_INFRARED_REMOTE)
            {
                outArray.AddRange(new byte[] { SPI_Address, (byte)message_type, 0, 0, 0, 0, 0, 0, 0, 0 });
                reply = spi_transfer_array(outArray.ToArray());
                if (reply[3] == 0xA5)
                {
                    if ((reply[4] == (int)SensorType[port_index]) && (reply[5] == (int)SENSOR_STATE.VALID_DATA))
                    {
                        byte[] results = new byte[] { 0, 0, 0, 0 };
                        for (int r = 0; r < results.Length; r++)
                        {
                            var value = reply[6 + r];
                            if (value == 1)
                                results[r] = 0b10000;
                            else if (value == 2)
                                results[r] = 0b01000;
                            else if (value == 3)
                                results[r] = 0b00100;
                            else if (value == 4)
                                results[r] = 0b00010;
                            else if (value == 5)
                                results[r] = 0b10100;
                            else if (value == 6)
                                results[r] = 0b10010;
                            else if (value == 7)
                                results[r] = 0b01100;
                            else if (value == 8)
                                results[r] = 0b01010;
                            else if (value == 9)
                                results[r] = 0b00001;
                            else if (value == 10)
                                results[r] = 0b11000;
                            else if (value == 11)
                                results[r] = 0b00110;
                            else
                                results[r] = 0b00000;
                        }
                        return results;
                    }
                    else
                        throw new SensorError("get_sensor error: Invalid sensor data");
                }
                else
                    throw new IOError("get_sensor error: No SPI response");
            }
            throw new IOError("get_sensor error: Sensor not configured or not supported.");
        }

        public void set_sensor_type(byte port, SENSOR_TYPE type, int[] param = null)
        {
            // Set the sensor type
            // Keyword arguments:
            // port-- The sensor port(s).PORT_1, PORT_2, PORT_3, and / or PORT_4.
            // type-- The sensor type
            // params = 0-- the parameters needed for some sensor types.
            // params is used for the following sensor types:
            //CUSTOM-- a 16 - bit integer used to configure the hardware.
            //I2C-- a list of settings:
            //    params[0]-- Settings / flags
            //    params[1] -- target Speed in microseconds(0-255). Realistically the speed will vary.
            //    if SENSOR_I2C_SETTINGS_SAME flag set in I2C Settings:
            //        params[2] -- Delay in microseconds between transactions.
            //        params[3] -- Address
            //        params[4] -- List of bytes to write
            //        params[5] -- Number of bytes to read

            for (int p = 0; p < 4; p++)
            {
                if ((port & (1 << p)) > 0)

                    SensorType[p] = type;
            }
            List<byte> outArray = new List<byte>();
            if (type == SENSOR_TYPE.CUSTOM)
            {
                outArray.AddRange(new byte[] { SPI_Address, (byte)SPI_MESSAGE_TYPE.SET_SENSOR_TYPE, port, (byte)type, (byte)((param[0] >> 8) & 0xFF), (byte)(param[0] & 0xFF) });
            }
            else if (type == SENSOR_TYPE.I2C)
            {
                if (param.Length >= 2)
                {
                    outArray.AddRange(new byte[] { SPI_Address, (byte)SPI_MESSAGE_TYPE.SET_SENSOR_TYPE, port, (byte)type, (byte)param[0], (byte)param[1] });  //# Settings, SpeedUS

                    if ((param[0] == (int)SENSOR_I2C_SETTINGS.SAME) && (param.Length >= 6))
                    {
                        outArray.Add((byte)((param[2] >> 24) & 0xFF)); // # DelayUS
                        outArray.Add((byte)((param[2] >> 16) & 0xFF));
                        outArray.Add((byte)((param[2] >> 8) & 0xFF));
                        outArray.Add((byte)(param[2] & 0xFF));
                        outArray.Add((byte)(param[3] & 0xFF));     //   # Address
                        outArray.Add((byte)(param[5] & 0xFF)); //   # InBytes

                        for (int p = 0; p < 4; p++)
                        {
                            if ((port & (1 << p)) > 0)

                                I2CInBytes[p] = (byte)(param[5] & 0xFF);
                        }
                        //TODO: fix this part, not sure and no example of usage
                        //outArray.append(len(params[4]))
                        outArray.Add((byte)param[4]); //    # OutBytes
                                                      //outArray.extend(params[4]) 
                        outArray.Add((byte)param[4]);         // # OutArray
                    }
                }
            }
            else
                outArray.AddRange(new byte[] { SPI_Address, (byte)SPI_MESSAGE_TYPE.SET_SENSOR_TYPE, port, (byte)type });

            spi_transfer_array(outArray.ToArray());
        }
        #endregion

        #region motors
        public void set_motor_power(byte port, int power)
        {
            // Set the motor power in percent
            // Keyword arguments:
            // port-- The Motor port(s).PORT_A, PORT_B, PORT_C, and / or PORT_D.
            // power-- The power from - 100 to 100, or -128 for float
            power = power > 127 ? (byte)127 : power;
            power = power < -128 ? -128 : power;
            byte bPower = (byte)(power & 0xFF);
            byte[] outArray = { SPI_Address, (byte)SPI_MESSAGE_TYPE.SET_MOTOR_POWER, port, bPower };
            var ret = spi_transfer_array(outArray);
        }

        public void set_motor_position(byte port, int position)
        {
            // Set the motor target position in degrees
            // Keyword arguments:
            // port -- The motor port(s). PORT_A, PORT_B, PORT_C, and/or PORT_D.
            // position -- The target position
            byte[] outArray = { SPI_Address, (byte)SPI_MESSAGE_TYPE.SET_MOTOR_POSITION, port, (byte)((position >> 24) & 0xFF), (byte)((position >> 16) & 0xFF), (byte)((position >> 8) & 0xFF), (byte)(position & 0xFF) };

            var ret = spi_transfer_array(outArray);
        }

        public void set_motor_position_kp(byte port, byte kp = 25)
        {
            //    Set the motor target position KP constant
            //    If you set kp higher, the motor will be more responsive to errors in position, at the cost of perhaps overshooting and oscillating.
            //    kd slows down the motor as it approaches the target, and helps to prevent overshoot.
            //    In general, if you increase kp, you should also increase kd to keep the motor from overshooting and oscillating.
            //    Keyword arguments:
            //    port -- The motor port(s). PORT_A, PORT_B, PORT_C, and/or PORT_D.
            //    kp -- The KP constant (default 25)
            byte[] outArray = { SPI_Address, (byte)SPI_MESSAGE_TYPE.SET_MOTOR_POSITION_KP, port, kp };
            var ret = spi_transfer_array(outArray);
        }

        public void set_motor_position_kd(byte port, byte kd = 70)
        {
            //    Set the motor target position KD constant
            //    If you set kp higher, the motor will be more responsive to errors in position, at the cost of perhaps overshooting and oscillating.
            //    kd slows down the motor as it approaches the target, and helps to prevent overshoot.
            //    In general, if you increase kp, you should also increase kd to keep the motor from overshooting and oscillating.
            //    Keyword arguments:
            //    port -- The motor port(s). PORT_A, PORT_B, PORT_C, and/or PORT_D.
            //    kd -- The KD constant (default 70)
            byte[] outArray = { SPI_Address, (byte)SPI_MESSAGE_TYPE.SET_MOTOR_POSITION_KD, port, kd };
            var ret = spi_transfer_array(outArray);
        }

        public void set_motor_dps(byte port, int dps)
        {
            //    Set the motor target speed in degrees per second
            //    Keyword arguments:
            //    port -- The motor port(s). PORT_A, PORT_B, PORT_C, and/or PORT_D.
            //    dps -- The target speed in degrees per second
            byte[] outArray = { SPI_Address, (byte)SPI_MESSAGE_TYPE.SET_MOTOR_DPS, port, (byte)((dps >> 8) & 0xFF), (byte)(dps & 0xFF) };
            var ret = spi_transfer_array(outArray);
        }

        public void set_motor_limits(byte port, byte power = 0, int dps = 0)
        {
            //    Set the motor speed limit
            //    Keyword arguments:
            //    port -- The motor port(s). PORT_A, PORT_B, PORT_C, and/or PORT_D.
            //    power -- The power limit in percent (0 to 100), with 0 being no limit (100)
            //    dps -- The speed limit in degrees per second, with 0 being no limit
            byte[] outArray = { SPI_Address, (byte)SPI_MESSAGE_TYPE.SET_MOTOR_LIMITS, port, power, (byte)((dps >> 8) & 0xFF), (byte)(dps & 0xFF) };
            var ret = spi_transfer_array(outArray);
        }

        public MotorStatus get_motor_status(byte port)
        {
            //Read a motor status
            //Keyword arguments:
            //port -- The motor port (one at a time). PORT_A, PORT_B, PORT_C, or PORT_D.
            //Returns a list:
            //    flags -- 8-bits of bit-flags that indicate motor status:
            //        bit 0 -- LOW_VOLTAGE_FLOAT - The motors are automatically disabled because the battery voltage is too low
            //        bit 1 -- OVERLOADED - The motors aren't close to the target (applies to position control and dps speed control).
            //    power -- the raw PWM power in percent (-100 to 100)
            //    encoder -- The encoder position
            //    dps -- The current speed in Degrees Per Second
            MotorStatus motorStatus = new MotorStatus();

            SPI_MESSAGE_TYPE message_type = SPI_MESSAGE_TYPE.NONE;
            if (port == (byte)MOTOR_PORT.PORT_A)
                message_type = SPI_MESSAGE_TYPE.GET_MOTOR_A_STATUS;
            else if (port == (byte)MOTOR_PORT.PORT_B)
                message_type = SPI_MESSAGE_TYPE.GET_MOTOR_B_STATUS;
            else if (port == (byte)MOTOR_PORT.PORT_C)
                message_type = SPI_MESSAGE_TYPE.GET_MOTOR_C_STATUS;
            else if (port == (byte)MOTOR_PORT.PORT_D)
                message_type = SPI_MESSAGE_TYPE.GET_MOTOR_D_STATUS;
            else
            {
                throw new IOError("get_motor_status error. Must be one motor port at a time. PORT_A, PORT_B, PORT_C, or PORT_D.");
            }
            byte[] outArray = { SPI_Address, (byte)message_type, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            var reply = spi_transfer_array(outArray);
            if (reply[3] == 0xA5)
            {
                motorStatus.Speed = reply[5];
                if ((motorStatus.Speed & 0x80) > 0)
                    motorStatus.Speed = -motorStatus.Speed;
                motorStatus.Encoder = (reply[6] << 24) | (reply[7] << 16) | (reply[8] << 8) | reply[9];
                //negative should be managed
                //if ((motorStatus.Encoder & 0x80000000) > 0)
                //    motorStatus.Encoder = motorStatus.Encoder - 0x100000000;
                motorStatus.Dps = ((reply[10] << 8) | reply[11]);
                if ((motorStatus.Dps & 0x8000) > 0)
                    motorStatus.Dps = motorStatus.Dps - 0x10000;
                motorStatus.Flags = (MotorStatusFlags)reply[4];
            }
            else
            {
                throw new IOError("No SPI response");
            }

            return motorStatus;
        }

        public void offset_motor_encoder(byte port, int position)
        {
            //Offset a motor encoder
            //Keyword arguments:
            //port -- The motor port(s). PORT_A, PORT_B, PORT_C, and/or PORT_D.
            //offset -- The encoder offset
            //Zero the encoder by offsetting it by the current position
            byte[] outArray = new byte[] { SPI_Address, (byte)SPI_MESSAGE_TYPE.OFFSET_MOTOR_ENCODER, port, (byte)((position >> 24) & 0xFF), (byte)((position >> 16) & 0xFF), (byte)((position >> 8) & 0xFF), (byte)(position & 0xFF) };
            spi_transfer_array(outArray);
        }
        public int get_motor_encoder(byte port)
        {
            //Read a motor encoder in degrees
            //Keyword arguments:
            //port -- The motor port (one at a time). PORT_A, PORT_B, PORT_C, or PORT_D.
            //Returns the encoder position in degrees
            SPI_MESSAGE_TYPE message_type;
            if (port == (byte)MOTOR_PORT.PORT_A)
                message_type = SPI_MESSAGE_TYPE.GET_MOTOR_A_ENCODER;
            else if (port == (byte)MOTOR_PORT.PORT_B)
                message_type = SPI_MESSAGE_TYPE.GET_MOTOR_B_ENCODER;
            else if (port == (byte)MOTOR_PORT.PORT_C)
                message_type = SPI_MESSAGE_TYPE.GET_MOTOR_C_ENCODER;
            else if (port == (byte)MOTOR_PORT.PORT_D)
                message_type = SPI_MESSAGE_TYPE.GET_MOTOR_D_ENCODER;
            else
                throw new IOError("get_motor_encoder error. Must be one motor port at a time. PORT_A, PORT_B, PORT_C, or PORT_D.");


            var encoder = spi_read_32(message_type);
            if ((encoder & 0x80000000) > 0)
                encoder = (int)(encoder - 0x100000000);
            return encoder;
        }
        #endregion

    }
}
