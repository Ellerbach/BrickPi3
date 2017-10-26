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
    /// Port used to select the ports for motors
    /// </summary>
    public enum MOTOR_PORT: byte
    {        
        PORT_A = 0x01,
        PORT_B = 0x02,
        PORT_C = 0x04,
        PORT_D = 0x08
    }

    /// <summary>
    /// Port used to select the ports for motors
    /// </summary>    
    public enum BrickPortMotor : byte
    {
        // Used to select the ports for motors
        PORT_A = 0x01,
        PORT_B = 0x02,
        PORT_C = 0x04,
        PORT_D = 0x08
    }

    /// <summary>
    /// flags -- 8-bits of bit-flags that indicate motor status:
    /// bit 0 -- LOW_VOLTAGE_FLOAT - The motors are automatically disabled because the battery voltage is too low
    /// bit 1 -- OVERLOADED - The motors aren't close to the target (applies to position control and dps speed control).
    /// </summary> 
    public enum MotorStatusFlags
    {
        ALL_OK = 0, LOW_VOLTAGE_FLOAT = 0x01, OVERLOADED = 0x02
    }
    /// <summary>
    /// Get the full status of the motor
    /// </summary>
    public class MotorStatus
    {
        public MotorStatusFlags Flags { get; set; }
        public int Speed { get; set; }
        public int Encoder { get; set; }
        public int Dps { get; set; }
    }

    /// <summary>
    /// Set quickly a speed for the motor
    /// </summary>
    public enum MOTOR_SPEED : byte
    {
        STOP = 0,
        FULL = 100,
        HALF = 50,
        // Motros in float mode constants
        // Actually any value great than 100 will float motors
        FLOAT = 128
    }

}
