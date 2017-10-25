using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
