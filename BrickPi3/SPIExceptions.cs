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

namespace BrickPi3
{
    public class SPIExceptions
    {
        public class FirmwareVersionError : Exception
        {
            public FirmwareVersionError() : base() { }
            public FirmwareVersionError(string msg) : base(msg) { }
            public FirmwareVersionError(string msg, Exception ex) : base(msg, ex) { }
        }
        // """Exception raised if the GoPiGo3 firmware needs to be updated"""


        public class SensorError : Exception
        {
            public SensorError() : base() { }
            public SensorError(string msg) : base(msg) { }
            public SensorError(string msg, Exception ex) : base(msg, ex) { }
        }
        //"""Exception raised if a sensor is not yet configured when trying to read it"""


        public class I2CError : Exception
        {
            public I2CError() : base() { }
            public I2CError(string msg) : base(msg) { }
            public I2CError(string msg, Exception ex) : base(msg, ex) { }
        }
        // """Exception raised if there was an error on an I2C bus"""


        public class ValueError : Exception
        {
            public ValueError() : base() { }
            public ValueError(string msg) : base(msg) { }
            public ValueError(string msg, Exception ex) : base(msg, ex) { }
        }

        public class IOError : Exception
        {
            public IOError() : base() { }
            public IOError(string msg) : base(msg) { }
            public IOError(string msg, Exception ex) : base(msg, ex) { }
        }

        public class RuntimeError : Exception
        {
            public RuntimeError() : base() { }
            public RuntimeError(string msg) : base(msg) { }
            public RuntimeError(string msg, Exception ex) : base(msg, ex) { }
        }
    }
}

