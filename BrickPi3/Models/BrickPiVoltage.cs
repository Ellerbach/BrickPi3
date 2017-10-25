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
    public class BrickPiVoltage
    {
        public double Voltage3V3 { get; set; } 
        public double Voltage5V { get; set; }
        public double Voltage9V { get; set; }
        public double VoltageBattery { get; set; }
    }
}
