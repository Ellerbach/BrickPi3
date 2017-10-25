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

using System.Collections.Generic;

namespace BrickPi3.Models
{
    public class BrickPiInfo
    {
        public string Manufacturer { get; set; }
        public string Board { get; set; }
        public string HardwareVersion { get; set; }
        public string SoftwareVersion { get; set; }
        public string Id { get; set; }
        public int[] GetHardwareVersion()
        {
            return GetVersionsFromString(HardwareVersion);
        }
        public int[] GetSoftwareVersion()
        {
            return GetVersionsFromString(SoftwareVersion);
        }

        private int[] GetVersionsFromString(string toconvert)
        {
            if (toconvert == "")
                return null;
            var split = toconvert.Split('.');
            List<int> ret = new List<int>();
            foreach (var elem in split)
                ret.Add(int.Parse(elem));
            return ret.ToArray();
        }
    }
}
