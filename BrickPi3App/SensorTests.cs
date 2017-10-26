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
using BrickPi3.Movement;
using BrickPi3.Sensors;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

namespace BrickPi3App
{
    public sealed partial class StartupTask : IBackgroundTask
    {

        private async Task TestMultipleSensorsTouchCSSoud()
        {
            NXTTouchSensor touch = new NXTTouchSensor(brick, BrickPortSensor.PORT_S2);
            EV3TouchSensor ev3Touch = new EV3TouchSensor(brick, BrickPortSensor.PORT_S1, 20);
            NXTSoundSensor sound = new NXTSoundSensor(brick, BrickPortSensor.PORT_S4);
            NXTColorSensor nxtlight = new NXTColorSensor(brick, BrickPortSensor.PORT_S3);
            RGBColor rgb;
            bool bwait = true;
            while (bwait)
            {
                Debug.WriteLine(string.Format("NXT Touch, Raw: {0}, ReadASString: {1}, IsPressed: {2}, NumberNodes: {3}, SensorName: {4}", touch.ReadRaw(), touch.ReadAsString(), touch.IsPressed(), touch.NumberOfModes(), touch.GetSensorName()));
                Debug.WriteLine(string.Format("EV3 Touch, Raw: {0}, ReadASString: {1}, IsPressed: {2}, NumberNodes: {3}, SensorName: {4}", ev3Touch.ReadRaw(), ev3Touch.ReadAsString(), ev3Touch.IsPressed(), ev3Touch.NumberOfModes(), ev3Touch.GetSensorName()));
                Debug.WriteLine(string.Format("NXT Sound, Raw: {0}, ReadASString: {1}, NumberNodes: {2}, SensorName: {3}", sound.ReadRaw(), sound.ReadAsString(), sound.NumberOfModes(), sound.GetSensorName()));
                Debug.WriteLine(string.Format("NXT Color Sensor, Raw: {0}, ReadASString: {1}, NumberNodes: {2}, SensorName: {3}",
                    nxtlight.ReadRaw(), nxtlight.ReadAsString(), nxtlight.NumberOfModes(), nxtlight.GetSensorName()));
                rgb = nxtlight.ReadRGBColor();
                Debug.WriteLine(string.Format("Color: {0}, Red: {1}, Green: {2}, Blue: {3}",
                    nxtlight.ReadColor(), rgb.Red, rgb.Green, rgb.Blue));
                //                Debug.WriteLine(string.Format("raw {0}", nxtlight.ReadTest()));
                await Task.Delay(300);
                if ((touch.IsPressed()) && ev3Touch.IsPressed())
                    bwait = false;
            }
        }

        private async Task TestEV3Color()
        {
            //brick.Stop();
            //brick.SetTimeout(250);
            EV3ColorSensor nxtlight = new EV3ColorSensor(brick, BrickPortSensor.PORT_S2, ColorSensorMode.Green);
            EV3TouchSensor touch = new EV3TouchSensor(brick, BrickPortSensor.PORT_S1);
            //brick.Stop();
            //brick.SetupSensors();
            RGBColor rgb;
            await Task.Delay(5000);
            for (int i = 0; i < nxtlight.NumberOfModes(); i++)
            {
                int count = 0;
                while ((count < 100) && !touch.IsPressed())
                {
                    //Debug.WriteLine(string.Format("NXT Touch, Raw: {0}, ReadASString: {1}, IsPressed: {2}, NumberNodes: {3}, SensorName: {4}", touch.ReadRaw(), touch.ReadAsString(), touch.IsPressed(), touch.NumberOfModes(), touch.GetSensorName()));
                    //Debug.WriteLine(string.Format("EV3 Touch, Raw: {0}, ReadASString: {1}, IsPressed: {2}, NumberNodes: {3}, SensorName: {4}", ev3Touch.ReadRaw(), ev3Touch.ReadAsString(), ev3Touch.IsPressed(), ev3Touch.NumberOfModes(), ev3Touch.GetSensorName()));
                    //Debug.WriteLine(string.Format("NXT Sound, Raw: {0}, ReadASString: {1}, NumberNodes: {2}, SensorName: {3}", sound.ReadRaw(), sound.ReadAsString(), sound.NumberOfModes(), sound.GetSensorName()));

                    //brick.UpdateValues();
                    Debug.WriteLine(string.Format("EV3 Color Sensor, Raw: {0}, ReadASString: {1}",
                        nxtlight.ReadRaw(), nxtlight.ReadAsString()));
                    rgb = nxtlight.ReadRGBColor();
                    Debug.WriteLine(string.Format("Color: {0}, Red: {1}, Green: {2}, Blue: {3}",
                        nxtlight.ReadColor(), rgb.Red, rgb.Green, rgb.Blue));
                    //brick.Stop();
                    //brick.Start();
                    //nxtlight.ColorMode = ColorSensorMode.Ambient;
                    await Task.Delay(1000);
                    //if ((touch.IsPressed()) && ev3Touch.IsPressed())
                    count++;
                    //nxtlight.ColorMode = ColorSensorMode.Color;
                }
                //if (nxtlight.ColorMode == ColorSensorMode.Reflection)
                //    nxtlight.ColorMode = ColorSensorMode.Color;
                //else
                //    nxtlight.ColorMode = ColorSensorMode.Reflection;
                nxtlight.SelectNextMode();
                //brick.SetupSensors();
                await Task.Delay(5000);
            }

        }
        //EV3IRSensor
        private async Task TestIRSensor()
        {

            EV3IRSensor ultra = new EV3IRSensor(brick, BrickPortSensor.PORT_S4, IRMode.Remote);

            int count = 0;
            while (count < 100)
            {
                Debug.WriteLine(string.Format("NXT ultra, Remote: {0}, ReadAsString: {1}, NumberNodes: {2}, SensorName: {3}",
                    ultra.Value, ultra.ReadAsString(), ultra.Mode, ultra.GetSensorName()));
                await Task.Delay(300);
                count++;
            }
            ultra.Mode = IRMode.Proximity;
            count = 0;
            while (count < 10)
            {
                Debug.WriteLine(string.Format("NXT ultra, Remote: {0}, ReadAsString: {1}, NumberNodes: {2}, SensorName: {3}",
                    ultra.Value, ultra.ReadAsString(), ultra.Mode, ultra.GetSensorName()));
                await Task.Delay(300);
                count++;
            }
            ultra.Mode = IRMode.Seek;
            count = 0;
            while (count < 10)
            {
                Debug.WriteLine(string.Format("NXT ultra, Remote: {0}, ReadAsString: {1}, NumberNodes: {2}, SensorName: {3}",
                    ultra.Value, ultra.ReadAsString(), ultra.Mode, ultra.GetSensorName()));
                await Task.Delay(300);
                count++;
            }

        }

        //TODO build test for EV3 Ultra Sound

        private async Task TestNXTUS()
        {
            NXTUltraSonicSensor ultra = new NXTUltraSonicSensor(brick, BrickPortSensor.PORT_S4);
            for (int i = 0; i < ultra.NumberOfModes(); i++)
            {
                int count = 0;
                while (count < 50)
                {
                    Debug.WriteLine(string.Format("NXT US, Distance: {0}, ReadAsString: {1}, Selected mode: {2}",
                        ultra.ReadDistance(), ultra.ReadAsString(), ultra.SelectedMode()));
                    await Task.Delay(2000);
                    count++;
                }
                ultra.SelectNextMode();
            }
        }

        private void TestTouch()
        {
            EV3TouchSensor touch = new EV3TouchSensor(brick, BrickPortSensor.PORT_S1);
            //NXTTouchSensor touch = new NXTTouchSensor(brick, BrickPortSensor.PORT_S2);
            int count = 0;
            while (count < 100)
            {
                Debug.WriteLine(string.Format("NXT Touch, IsPRessed: {0}, ReadAsString: {1}, Selected mode: {2}",
                    touch.IsPressed(), touch.ReadAsString(), touch.SelectedMode()));
                Task.Delay(300).Wait(); ;
            }

        }

        private async Task TestNXTLight()
        {
            //NXTTouchSensor touch = new NXTTouchSensor(BrickPortSensor.PORT_S2);
            //EV3TouchSensor ev3Touch = new EV3TouchSensor(BrickPortSensor.PORT_S1);
            //NXTSoundSensor sound = new NXTSoundSensor(BrickPortSensor.PORT_S4);
            NXTLightSensor nxtlight = new NXTLightSensor(brick, BrickPortSensor.PORT_S4);
            int count = 0;
            while (count < 100)
            {
                //Debug.WriteLine(string.Format("NXT Touch, Raw: {0}, ReadASString: {1}, IsPressed: {2}, NumberNodes: {3}, SensorName: {4}", touch.ReadRaw(), touch.ReadAsString(), touch.IsPressed(), touch.NumberOfModes(), touch.GetSensorName()));
                //Debug.WriteLine(string.Format("EV3 Touch, Raw: {0}, ReadASString: {1}, IsPressed: {2}, NumberNodes: {3}, SensorName: {4}", ev3Touch.ReadRaw(), ev3Touch.ReadAsString(), ev3Touch.IsPressed(), ev3Touch.NumberOfModes(), ev3Touch.GetSensorName()));
                //Debug.WriteLine(string.Format("NXT Sound, Raw: {0}, ReadASString: {1}, NumberNodes: {2}, SensorName: {3}", sound.ReadRaw(), sound.ReadAsString(), sound.NumberOfModes(), sound.GetSensorName()));
                Debug.WriteLine(string.Format("NXT Color Sensor, Raw: {0}, ReadASString: {1}, NumberNodes: {2}, SensorName: {3}",
                    nxtlight.ReadRaw(), nxtlight.ReadAsString(), nxtlight.NumberOfModes(), nxtlight.GetSensorName()));
                Debug.WriteLine(string.Format("Color: {0}, ",
                    nxtlight.ReadRaw()));

                await Task.Delay(300);
                //if ((touch.IsPressed()) && ev3Touch.IsPressed())
                count++;
            }
            count = 0;
            nxtlight.SelectNextMode();
            while (count < 100)
            {
                //Debug.WriteLine(string.Format("NXT Touch, Raw: {0}, ReadASString: {1}, IsPressed: {2}, NumberNodes: {3}, SensorName: {4}", touch.ReadRaw(), touch.ReadAsString(), touch.IsPressed(), touch.NumberOfModes(), touch.GetSensorName()));
                //Debug.WriteLine(string.Format("EV3 Touch, Raw: {0}, ReadASString: {1}, IsPressed: {2}, NumberNodes: {3}, SensorName: {4}", ev3Touch.ReadRaw(), ev3Touch.ReadAsString(), ev3Touch.IsPressed(), ev3Touch.NumberOfModes(), ev3Touch.GetSensorName()));
                //Debug.WriteLine(string.Format("NXT Sound, Raw: {0}, ReadASString: {1}, NumberNodes: {2}, SensorName: {3}", sound.ReadRaw(), sound.ReadAsString(), sound.NumberOfModes(), sound.GetSensorName()));
                Debug.WriteLine(string.Format("NXT Color Sensor, Raw: {0}, ReadASString: {1}, NumberNodes: {2}, SensorName: {3}",
                    nxtlight.ReadRaw(), nxtlight.ReadAsString(), nxtlight.NumberOfModes(), nxtlight.GetSensorName()));
                Debug.WriteLine(string.Format("Color: {0}, ",
                    nxtlight.ReadRaw()));

                await Task.Delay(300);
                //if ((touch.IsPressed()) && ev3Touch.IsPressed())
                count++;
            }
        }

        private async Task TestNXTCS()
        {
            NXTColorSensor nxtlight = new NXTColorSensor(brick, BrickPortSensor.PORT_S4);
            RGBColor rgb;
            bool bwait = true;
            while (bwait)
            {
                Debug.WriteLine(string.Format("NXT Color Sensor, Raw: {0}, ReadASString: {1}, NumberNodes: {2}",
                    nxtlight.ReadRaw(), nxtlight.ReadAsString(), nxtlight.SelectedMode()));
                rgb = nxtlight.ReadRGBColor();
                Debug.WriteLine(string.Format("Color: {0}, Red: {1}, Green: {2}, Blue: {3}",
                    nxtlight.ReadColor(), rgb.Red, rgb.Green, rgb.Blue));
                //                Debug.WriteLine(string.Format("raw {0}", nxtlight.ReadTest()));
                await Task.Delay(300);
            }


        }
    }
}
