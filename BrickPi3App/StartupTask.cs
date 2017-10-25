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

using System;
using Windows.ApplicationModel.Background;
using BrickPi3;
using BrickPi3.Models;
using System.Diagnostics;
using System.Threading.Tasks;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace BrickPi3App
{
    public sealed class StartupTask : IBackgroundTask
    {
        private BackgroundTaskDeferral deferral;
        private Brick brick = new Brick();
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            deferral = taskInstance.GetDeferral();

            brick.InitSPI();
            try
            {
                TestBrickDetails();
                TestSensors();
                TestRunMotors();
                TestMotorEncoder();
                TestMotorDPS();
                TestMotorPosition();
            }
            catch (Exception ex)
            {

                Debug.WriteLine($"Exception: {ex.Message}");
            }
            brick.reset_all();

        }

        private void TestMotorPosition()
        {
            //
            // Test motor position
            //
            brick.offset_motor_encoder((byte)MOTOR_PORT.PORT_D, brick.get_motor_encoder((byte)MOTOR_PORT.PORT_D));
            brick.offset_motor_encoder((byte)MOTOR_PORT.PORT_A, brick.get_motor_encoder((byte)MOTOR_PORT.PORT_A));
            brick.set_motor_position_kd((byte)MOTOR_PORT.PORT_A);
            brick.set_motor_position_kp((byte)MOTOR_PORT.PORT_A);
            // Float motor D
            brick.set_motor_power((byte)MOTOR_PORT.PORT_D, (byte)MOTOR_SPEED.FLOAT);
            // set some limits
            brick.set_motor_limits((byte)MOTOR_PORT.PORT_A, 50, 200);
            brick.set_sensor_type((byte)SENSOR_PORT.PORT_1, SENSOR_TYPE.NXT_TOUCH);
            //run until we press the button on port2
            while (brick.get_sensor((byte)SENSOR_PORT.PORT_1)[0] == 0)
            {
                var target = brick.get_motor_encoder((byte)MOTOR_PORT.PORT_D);
                brick.set_motor_position((byte)MOTOR_PORT.PORT_A, target);
                var status = brick.get_motor_status((byte)MOTOR_PORT.PORT_A);
                Debug.WriteLine($"Motor A Target Degrees Per Second: {target}; Motor A speed: {status.Speed}; DPS: {status.Dps}; Encoder: {status.Encoder}; Flags: {status.Flags}");
                Task.Delay(20).Wait();
            }

        }

        private void TestMotorDPS()
        {
            //
            // Test Mortor Degree Per Second (DPS)
            //
            brick.offset_motor_encoder((byte)MOTOR_PORT.PORT_D, brick.get_motor_encoder((byte)MOTOR_PORT.PORT_D));
            brick.offset_motor_encoder((byte)MOTOR_PORT.PORT_A, brick.get_motor_encoder((byte)MOTOR_PORT.PORT_A));
            // Float motor D
            brick.set_motor_power((byte)MOTOR_PORT.PORT_D, (byte)MOTOR_SPEED.FLOAT);
            brick.set_sensor_type((byte)SENSOR_PORT.PORT_1, SENSOR_TYPE.NXT_TOUCH);
            //run until we press the button on port2
            while (brick.get_sensor((byte)SENSOR_PORT.PORT_1)[0] == 0)
            {
                var target = brick.get_motor_encoder((byte)MOTOR_PORT.PORT_D);
                brick.set_motor_dps((byte)MOTOR_PORT.PORT_A, target);
                var status = brick.get_motor_status((byte)MOTOR_PORT.PORT_A);
                Debug.WriteLine($"Motor A Target Degrees Per Second: {target}; Motor A speed: {status.Speed}; DPS: {status.Dps}; Encoder: {status.Encoder}; Flags: {status.Flags}");
                Task.Delay(20).Wait();
            }
        }

        private void TestMotorEncoder()
        {
            //
            // Test Motor encoders
            //         
            // Reset first the position
            brick.offset_motor_encoder((byte)MOTOR_PORT.PORT_D, brick.get_motor_encoder((byte)MOTOR_PORT.PORT_D));
            for (int i = 0; i < 100; i++)
            {
                var encodermotor = brick.get_motor_encoder((byte)MOTOR_PORT.PORT_D);
                Debug.WriteLine($"Encoder: {encodermotor}");
                Task.Delay(200).Wait();
            }
        }

        private void TestBrickDetails()
        {
            //
            // Get the details abourt the brick
            //
            var brickinfo = brick.BrickPi3Info;
            Debug.WriteLine($"Manufacturer: {brickinfo.Manufacturer}");
            Debug.WriteLine($"Board: {brickinfo.Board}");
            Debug.WriteLine($"Hardware version: {brickinfo.HardwareVersion}");
            var hdv = brickinfo.GetHardwareVersion();
            for (int i = 0; i < hdv.Length; i++)
                Debug.WriteLine($"Hardware version {i}: {hdv[i]}");
            Debug.WriteLine($"Software version: {brickinfo.SoftwareVersion}");
            var swv = brickinfo.GetSoftwareVersion();
            for (int i = 0; i < swv.Length; i++)
                Debug.WriteLine($"Software version {i}: {swv[i]}");
            Debug.WriteLine($"Id: {brickinfo.Id}");
            //
            // Testing Led
            //
            for (int i = 0; i < 10; i++)
            {
                brick.set_led((byte)(i * 10));
                Task.Delay(500).Wait();
            }
            for (int i = 0; i < 10; i++)
            {
                brick.set_led((byte)(100 - i * 10));
                Task.Delay(500).Wait();
            }
            brick.set_led(255);
            //
            // Get the voltage details
            //
            var voltage = brick.BrickPi3Voltage;
            Debug.WriteLine($"3.3V: {voltage.Voltage3V3}");
            Debug.WriteLine($"5V: {voltage.Voltage5V}");
            Debug.WriteLine($"9V: {voltage.Voltage9V}");
            Debug.WriteLine($"Battery voltage: {voltage.VoltageBattery}");
        }

        private void TestSensors()
        {
            //
            // Setting a sencor and reading values
            //
            Debug.WriteLine($"{SENSOR_TYPE.EV3_ULTRASONIC_CM.ToString()}");
            brick.set_sensor_type((byte)SENSOR_PORT.PORT_3, SENSOR_TYPE.EV3_ULTRASONIC_CM);
            for (int i = 0; i < 100; i++)
            {
                Debug.WriteLine($"Iterration {i}");
                try
                {
                    var sensordata = brick.get_sensor((byte)SENSOR_PORT.PORT_3);
                    for (int j = 0; j < sensordata.Length; j++)
                        Debug.WriteLine($"Sensor value {j}: {sensordata[j]}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Exception: {ex.Message}");
                }
                Task.Delay(200).Wait();

            }
            Debug.WriteLine($"{SENSOR_TYPE.EV3_TOUCH.ToString()}");
            brick.set_sensor_type((byte)SENSOR_PORT.PORT_4, SENSOR_TYPE.EV3_TOUCH);
            for (int i = 0; i < 100; i++)
            {
                Debug.WriteLine($"Iterration {i}");
                try
                {
                    var sensordata = brick.get_sensor((byte)SENSOR_PORT.PORT_4);
                    for (int j = 0; j < sensordata.Length; j++)
                        Debug.WriteLine($"Sensor value {j}: {sensordata[j]}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Exception: {ex.Message}");
                }
                Task.Delay(200).Wait();
            }
            Debug.WriteLine($"{SENSOR_TYPE.NXT_TOUCH.ToString()}");
            brick.set_sensor_type((byte)SENSOR_PORT.PORT_1, SENSOR_TYPE.NXT_TOUCH);
            for (int i = 0; i < 100; i++)
            {
                Debug.WriteLine($"Iterration {i}");
                try
                {
                    var sensordata = brick.get_sensor((byte)SENSOR_PORT.PORT_1);
                    for (int j = 0; j < sensordata.Length; j++)
                        Debug.WriteLine($"Sensor value {j}: {sensordata[j]}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Exception: {ex.Message}");
                }
                Task.Delay(200).Wait();

            }
            Debug.WriteLine($"{SENSOR_TYPE.EV3_COLOR_COLOR.ToString()}");
            brick.set_sensor_type((byte)SENSOR_PORT.PORT_2, SENSOR_TYPE.EV3_COLOR_COLOR);
            for (int i = 0; i < 100; i++)
            {
                Debug.WriteLine($"Iterration {i}");
                try
                {
                    var sensordata = brick.get_sensor((byte)SENSOR_PORT.PORT_2);
                    for (int j = 0; j < sensordata.Length; j++)
                        Debug.WriteLine($"Sensor value {j}: {sensordata[j]}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Exception: {ex.Message}");
                }
                Task.Delay(200).Wait();

            }
        }

        private void TestRunMotors()
        {
            //
            // Testing motors
            //
            // Acceleration to full speed, float and decreasing speed to stop
            for (int i = 0; i < 10; i++)
            {
                brick.set_motor_power((byte)MOTOR_PORT.PORT_D, (byte)(i * 10));
                Task.Delay(1000).Wait();
            }
            brick.set_motor_power((byte)MOTOR_PORT.PORT_D, (byte)MOTOR_SPEED.FLOAT);
            Task.Delay(1000).Wait();
            for (int i = 0; i < 10; i++)
            {
                brick.set_motor_power((byte)MOTOR_PORT.PORT_D, (byte)(100 - i * 10));
                Task.Delay(1000).Wait();
            }
            brick.set_motor_power((byte)MOTOR_PORT.PORT_D, (byte)MOTOR_SPEED.FLOAT);
        }
    }
}
