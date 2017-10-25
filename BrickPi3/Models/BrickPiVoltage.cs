using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickPi3.Models
{
    public class BrickPiVoltage
    {
        public double Voltage3V3 { get; set; } 
        public double Voltage5V { get; set; }
        public double Voltage9V { get; set; }
        public double VoltageBattery { get; set; }
    }
}
