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
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

namespace BrickPi3App
{
    public sealed partial class StartupTask : IBackgroundTask
    {
        private async Task TestMotor1Motor()
        {
            Motor motor = new Motor(brick, BrickPortMotor.PORT_D);
            motor.SetSpeed(10);
            motor.Start();
            Stopwatch stopwatch = Stopwatch.StartNew();
            long initialTick = stopwatch.ElapsedTicks;
            double desiredTicks = 10000.0 / 1000.0 * Stopwatch.Frequency;
            double finalTick = initialTick + desiredTicks;
            while (stopwatch.ElapsedTicks < finalTick)
            {
                Debug.WriteLine(string.Format("Encoder: {0}", motor.GetTachoCount()));
                await Task.Delay(200);
                motor.SetSpeed(motor.GetSpeed() + 10);

            }
            motor.SetPolarity(Polarity.OppositeDirection);
            desiredTicks = 10000.0 / 1000.0 * Stopwatch.Frequency;
            finalTick = stopwatch.ElapsedTicks + desiredTicks;
            while (stopwatch.ElapsedTicks < finalTick)
            {
                Debug.WriteLine(string.Format("Encoder: {0}", motor.GetTachoCount()));
                await Task.Delay(200);
                motor.SetSpeed(motor.GetSpeed() + 10);
            }
            desiredTicks = 10000.0 / 1000.0 * Stopwatch.Frequency;
            finalTick = stopwatch.ElapsedTicks + desiredTicks;
            int pos = 0;
            while (stopwatch.ElapsedTicks < finalTick)
            {
                Debug.WriteLine(string.Format("Encoder: {0}", motor.GetTachoCount()));
                await Task.Delay(2000);
                motor.SetTachoCount(pos);
            }
            motor.Stop();

        }
        
        private async Task TestMotor()
        {
            Motor[] motor = new Motor[3];
            motor[0] = new Motor(brick, BrickPortMotor.PORT_D);
            motor[1] = new Motor(brick, BrickPortMotor.PORT_A);
            motor[2] = new Motor(brick, BrickPortMotor.PORT_C);
            for (int i = 0; i < motor.Length; i++)
            {
                motor[i].SetSpeed(0);
                motor[i].Start();
            }
            Stopwatch stopwatch = Stopwatch.StartNew();
            long initialTick = stopwatch.ElapsedTicks;
            double desiredTicks = 10000.0 / 1000.0 * Stopwatch.Frequency;
            double finalTick = initialTick + desiredTicks;
            while (stopwatch.ElapsedTicks < finalTick)
            {
                for (int i = 0; i < motor.Length; i++)
                {
                    Debug.WriteLine(string.Format("Encoder motor {0}: {1}", i, motor[i].GetTachoCount()));
                    motor[i].SetSpeed(motor[i].GetSpeed() + 1);
                }
                await Task.Delay(200);
            }
            Debug.WriteLine("End speed increase");
            for (int i = 0; i < motor.Length; i++)
            {
                motor[i].SetPolarity(Polarity.OppositeDirection);
            }
            Debug.WriteLine("End of inverting rotation");
            desiredTicks = 10000.0 / 1000.0 * Stopwatch.Frequency;
            finalTick = stopwatch.ElapsedTicks + desiredTicks;
            while (stopwatch.ElapsedTicks < finalTick)
            {
                for (int i = 0; i < motor.Length; i++)
                {
                    Debug.WriteLine(string.Format("Encoder motor {0}: {1}", i, motor[i].GetTachoCount()));
                    motor[i].SetSpeed(motor[i].GetSpeed() + 5);
                }
                await Task.Delay(200);

            }
            Debug.WriteLine("End speed decrease");
            desiredTicks = 10000.0 / 1000.0 * Stopwatch.Frequency;
            finalTick = stopwatch.ElapsedTicks + desiredTicks;
            int pos = 0;
            while (stopwatch.ElapsedTicks < finalTick)
            {
                for (int i = 0; i < motor.Length; i++)
                {
                    Debug.WriteLine(string.Format("Encoder motor {0}: {1}", i, motor[i].GetTachoCount()));
                    motor[i].SetTachoCount(pos);
                }
                await Task.Delay(1000);

            }
            Debug.WriteLine("End encoder offset test");
            for (int i = 0; i < motor.Length; i++)
            {
                motor[i].Stop();
            }
            Debug.WriteLine("All motors stoped");
        }

        private void TestMotorEvents()
        {
            Motor motor = new Motor(brick, BrickPortMotor.PORT_D, 500);
            motor.PropertyChanged += Motor_PropertyChanged;
            Stopwatch stopwatch = Stopwatch.StartNew();
            long initialTick = stopwatch.ElapsedTicks;
            double desiredTicks = 10000.0 / 1000.0 * Stopwatch.Frequency;
            double finalTick = initialTick + desiredTicks;
            while (stopwatch.ElapsedTicks < finalTick)
            {
                //do nothing
            }
        }

        private void Motor_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Debug.WriteLine($"Endoer changed: {e.PropertyName}; {((Motor)sender).TachoCount}");
        }

        private async Task TestVehicule()
        {
            Vehicule veh = new Vehicule(brick, BrickPortMotor.PORT_A, BrickPortMotor.PORT_D);
            veh.DirectionOpposite = true;
            veh.Backward(30, 5000);
            veh.Foreward(30, 5000);
            veh.TrunLeftTime(30, 5000);
            veh.TrunRightTime(30, 5000);
            veh.TurnLeft(30, 180);
            veh.TurnRight(30, 180);
        }
    }
}