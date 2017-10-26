# BrickPi3
Windows 10 IoT Core implementation for the excellent BrickPi3 from Dexter Industries running on RaspberryPi 2 or Raspeberry Pi 3. 

- [RaspberryPi setup](./README.md#setup-the-raspberrypi-running-windows-10-iot-core)
- [BrickPi3 requirements](./README.md#make-sure-you-have-a-brickpi3)
- [Know limitations](./README.md#known-limitations)
- [Using the driver](./README.md#how-to-use-the-driver)
  - [Accessing BrickPi3 information](./README.md#accessing-brickpi3-information)
  - [Using sensors](./README.md#using-sensors)
  - [Using motors](./README.md#using-motors)
    - [Making a motor moving depending on the position of another motor](./README.md#making-a-motor-moving-depending-on-the-position-of-another-motor)
    - [Setting power to motors](./README.md#setting-power-to-motors)
    - [Reading encoders](./README.md#reading-encoders)
    - [Setting motor speed with degree per seconds](./README.md#setting-motor-speed-with-degree-per-seconds)
- [Using high level classes](./README.md#how-to-use-the-high-level-classes)
  - [Using the Sensor classes](./README.md#using-the-sensor-classes)
  - [Using Motors](./README.md#using-motors)
  - [Compatibility between BrickPi and BrickPi3](./README.md#compatibility-between-brickpi-and-brickpi3)
- [Using GrovePi port](./README.md#using-grovepi-port)


## Setup the RaspberryPi running Windows 10 IoT Core
There is nothing specif to do for BrickPi3 to run on Windows 10 IoT Core. Just install [Windows 10 IoT Core either from the dedicated tool](https://developer.microsoft.com/en-us/windows/iot), either from the [Noob installation](https://www.raspberrypi.org/downloads/).

## Make sure you have a BrickPi3
There are multiple versions of BrickPi, this code is only working for BrickPi3. The main difference between the 2 BrickPi is the switch available close to the battery alimentation. If you have a switch, you have a BrickPi3.
If you have a previous version, you can find the code on https://github.com/ellerbach/BrickPi.

## Known limitations
This version include a pure driver so you can directly access to the raw results. High level classes has been created and most sensors has been tested. Still you may have questions or issues with some sensors.

Port of high level classes has been done like in the previous version. Idea is to get as much compatibility as possible with the previous version. Most sensors has been tested and seems to work. There are a few break compare to the previous version. They are documented below.

The drive port is from the Python code. Function names do not respect C# conventions for this reason.

Some sensors has not been tested but most of the NXT and EV3 has been.For I2C sensors, the code has not been tested at all. So far I only had issues with the NXT Ultrasound wich almost all the time return incorrect data. I suspect an issue with my sensor as all the others are working perfectly. Color sensors may return incorect data as well, not all modes and all conditions has been tested yet.

## How to use the driver
The main BrickPi3App contains a series of test showing how to use every elements of the driver.
Create a class and initialize it thru the ```InitSPI()``` function. It is recommended to reinialize everything when you're done.
```C#
Brick brick = new Brick();
brick.InitSPI();
// Do whatever you want, read sensors, set motors, etc
// once finished, call the reset function
brick.reset_all();
```
If you have multiple BrickPi3 and want to change the adress of a specific BrickPi3, use the ```set_address``` function. Be aware that once changed in the firmware it stays. By default the address 1.

### Accessing BrickPi3 information
There are informations like the board Id, manufacturer available. You can esilly access them like in the following code:
```C#
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
```

You can as well adjust the embedded led, here is an example:
```C#
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
```

And you can get the various voltage of the board including the battery voltage
```C#
//
// Get the voltage details
//
var voltage = brick.BrickPi3Voltage;
Debug.WriteLine($"3.3V: {voltage.Voltage3V3}");
Debug.WriteLine($"5V: {voltage.Voltage5V}");
Debug.WriteLine($"9V: {voltage.Voltage9V}");
Debug.WriteLine($"Battery voltage: {voltage.VoltageBattery}");
```

### Using sensors
To setup a sensor, you need first to set the type of sensor then you can read the data. The below example setup an NXT Touch sensor on port 1 and read the results in continue.

```C#
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
```

Please note that the function ```get_sensor``` returns an array of byte, it's up to you to interpret correctly the data out of this function. Please read the documentation on https://www.dexterindustries.com/BrickPi/brickpi3-technical-design-details/brickpi3-communication-protocol/ to have the full details of what every sensor return.

### Using motors
There are many ways you can use motors, either by setting the power, either by reading the encoder, either by setting a degree per second speed. Those 3 examples has been implemented in test so you can see how to use them.

#### Making a motor moving depending on the position of another motor
In this example, the motor on port D is used to set the position of the motor A. A simple NXT touch sensor is used to end the sequence when it is pressed.

You can see as well the MotorStatus classes containing all information on the motor. Flags are useful to understand if you have issues with the power or an overload of the motors. 

To reinitialize the encoder, simply set the offset to the current version like shown in the first 2 lines.

```C#
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
```

#### Setting power to motors
This simple example increase the speed of a motor and decrease it after. Use ```MOTOR_SPEED.FLOAT``` to float the motor. Setting speed at 0 do not have the exact same effect. It does block the motor. Float will just let the motor goes its own way.

```C#
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
```

#### Reading encoders
On top of the motor position example, here is another example to read the encoder of a motor. It's an easy way to understand if the motor is correctly plugged if any of the function you want to use is not working. Moving it will change the encoder value. Please note that the encoder value can be negative as well. The first line reset the encoder.
```C#
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
```

#### Setting motor speed with degree per seconds
Another way to contor the motors is to set a specific speed in degree per seconds. In the below example, no limit has been set but it's possible to setup a limit like in the motor position example. Setting up limits reduce the risk of overheat on the motors. 

```C#
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
```

## How to use the high level classes
There are high level classes like for the previous version to handle directly objects like NXT Touch sensors or EV3 Color sensor. There is as well Motor classes and Vehicule allowing to have a simpler way to manage than directly thru the low level driver.

### Using the Sensor classes
Using the sensor classes is straicht forward. Just reference a class and initialized it. Access properties and function. The ReadRaw(), ReadAsString() functions are common to all sensors, Value and ValueAsString properties as well. 
A changed property event on the properties is raised with a minimum period you can determined when creating the object or later if the property has changed. That make your life easier if you're building XAML UI and want to use property binding directly in your code.
Example creating a NXT Touch Sensor:
```C#
Brick brick = new Brick();
brick.InitSPI();

NXTTouchSensor touch = new NXTTouchSensor(brick, BrickPortSensor.PORT_S2);
Debug.WriteLine(string.Format("Raw: {0}, ReadASString: {1}, IsPressed: {2}, NumberNodes: {3}, SensorName: {4}", touch.ReadRaw(), touch.ReadAsString(), touch.IsPressed(), touch.NumberOfModes(), touch.GetSensorName()));
```
This will create an EV3 Touch Sensor on port S1 and will tell it to check changes in properties every 20 milliseconds.
```C#
EV3TouchSensor ev3Touch = new EV3TouchSensor(BrickPortSensor.PORT_S1, 20);
```
This allow to have as many sensors as you want as well as having multiple BrickPi3. Just pass to the sensor the Brick class and you're good to go.
All sensors will return a ```int.MaxValue``` in case of error when reading the data. Test the value when using it. This choise is to avoid having exceptions to handle when using those high level classes.

### Using Motors
Motors are as well really easy to use. You have functions Start(), Stop(), SetSpeed(speed) and GetSpeed() which as you can expect will start, stop, change the speed and give you the current speed. A speed property is available as well and will change the speed. 
Lego motors have an encoder which gives you the position in 0.5 degree precision. You can get access thru function GetTachoCount(). As the numbers can get big quite fast, you can reset this counter by using SetTachoCount(newnumber). A TachoCount property is available as well. This property like for sensors can raise an event on a minimum time base you can setup. This is usefull if you're binding this value to a XAML UI for example.

```C#
Brick brick = new Brick();
brick.InitSPI();

Motor motor = new Motor(brick, BrickPortMotor.PORT_D);
motor.SetSpeed(100); //speed goes from -100 to +100, others will float the motor
motor.Start();
motor.SetSpeed(motor.GetSpeed() + 10);
Debug.WriteLine(string.Format("Encoder: {0}", motor.GetTachoCount()));
Debug.WriteLine(string.Format("Encoder: {0}", motor.TachoCount)); //same as previous line
Debug.WriteLine(string.Format("Speed: {0}", motor.GetSpeed()));
Debug.WriteLine(string.Format("Speed: {0}", motor.Speed)); //same as previous line
motor.SetPolarity(Polarity.OppositeDirection); // change the direction
motor.Stop();
```
The vehicule class is working the same way. Just instance it and use the various functions to drive your vehicule.

### Compatibility between BrickPi and BrickPi3
In terms of code, the low level drivers are very different and not compatible.

That said, the high level classes like all sensors, motors and vehicule are compatible. The only difference is during the creation of the class, with BrickPi3, you have to pass the Brick class in the constructor while it was not necessary with the BrickPi version. Tthis is due to the fact that you can support lots of BrickPi3 while only 2 were supported for the BrickPi. 

Be aware as well that the color sensors do not return anymore the raw data for colors. So use ```ColorSensorMode.Green``` to return the full colors. 

## Using GrovePi port
The only supported sensors in GrovePi port are I2C sensors. You directly use the source and code from the GrovePi C# repository here: https://github.com/DexterInd/GrovePi/tree/master/Software/CSharp/Samples



This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.