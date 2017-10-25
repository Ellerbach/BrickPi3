using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickPi3.Models
{
    public enum MOTOR_PORT : byte
    {
        // Used to select the ports for motors
        PORT_A = 0x01,
        PORT_B = 0x02,
        PORT_C = 0x04,
        PORT_D = 0x08
    }

    public enum MotorStatusFlags
    {
        // flags -- 8-bits of bit-flags that indicate motor status:
        // bit 0 -- LOW_VOLTAGE_FLOAT - The motors are automatically disabled because the battery voltage is too low
        // bit 1 -- OVERLOADED - The motors aren't close to the target (applies to position control and dps speed control).
        ALL_OK = 0, LOW_VOLTAGE_FLOAT = 0x01, OVERLOADED = 0x02
    }
    public class MotorStatus
    {
        //reply[4], speed, encoder, dps
        public MotorStatusFlags Flags { get; set; }
        public int Speed { get; set; }
        public int Encoder { get; set; }
        public int Dps { get; set; }
    }

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
