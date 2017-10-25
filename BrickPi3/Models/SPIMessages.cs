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
    public enum SPI_MESSAGE_TYPE : byte
    {
        NONE = 0,
        GET_MANUFACTURER,
        GET_NAME,
        GET_HARDWARE_VERSION,
        GET_FIRMWARE_VERSION,
        GET_ID,
        SET_LED,
        GET_VOLTAGE_3V3,
        GET_VOLTAGE_5V,
        GET_VOLTAGE_9V,
        GET_VOLTAGE_VCC,
        SET_ADDRESS,
        SET_SENSOR_TYPE,
        GET_SENSOR_1,
        GET_SENSOR_2,
        GET_SENSOR_3,
        GET_SENSOR_4,
        I2C_TRANSACT_1,
        I2C_TRANSACT_2,
        I2C_TRANSACT_3,
        I2C_TRANSACT_4,
        SET_MOTOR_POWER,
        SET_MOTOR_POSITION,
        SET_MOTOR_POSITION_KP,
        SET_MOTOR_POSITION_KD,
        SET_MOTOR_DPS,
        SET_MOTOR_DPS_KP,
        SET_MOTOR_DPS_KD,
        SET_MOTOR_LIMITS,
        OFFSET_MOTOR_ENCODER,
        GET_MOTOR_A_ENCODER,
        GET_MOTOR_B_ENCODER,
        GET_MOTOR_C_ENCODER,
        GET_MOTOR_D_ENCODER,
        GET_MOTOR_A_STATUS,
        GET_MOTOR_B_STATUS,
        GET_MOTOR_C_STATUS,
        GET_MOTOR_D_STATUS
    };
}
