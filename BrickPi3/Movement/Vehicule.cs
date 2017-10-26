//////////////////////////////////////////////////////////
// This code has been originally created by Laurent Ellerbach
// It intend to make the excellent BrickPi3 from Dexter Industries working
// on a RaspberryPi 2 or 3 runing Windows 10 IoT Core in Universal
// Windows Platform.
// Credits:
// - Dexter Industries Code
// - MonoBrick for great inspiration regarding sensors implementation in C#
//
// This code is origianlly created for the original BrickPi
// see https://github.com/ellerbach/BrickPi
//
// This code is under https://opensource.org/licenses/ms-pl
//
//////////////////////////////////////////////////////////

using BrickPi3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BrickPi3.Movement
{
    public sealed class Vehicule
    {
        private Brick brick = null;
        private BrickPortMotor portleft;
        private BrickPortMotor portright;
        private bool directionOpposite = false;
        private int correctedDir = 1;

        /// <summary>
        /// Create a vehicule with 2 motors, one left and one right
        /// </summary>
        /// <param name="left">Motor port for left motor</param>
        /// <param name="right">Motor port for right motor</param>
        public Vehicule(Brick brick, BrickPortMotor left, BrickPortMotor right)
        {
            this.brick = brick;
            //brick.Start();
            portleft = left;
            portright = right;
        }

        /// <summary>
        /// Run backward at the specified speed
        /// </summary>
        /// <param name="speed">speed is between -255 and +255</param>
        public void Backward(int speed)
        {
            StartMotor((int)PortLeft, speed * correctedDir);
            StartMotor((int)PortRight, speed * correctedDir);
        }

        /// <summary>
        /// Run forward at the specified speed
        /// </summary>
        /// <param name="speed">speed is between -255 and +255</param>
        public void Forward(int speed)
        {
            Backward(-speed);
        }

        /// <summary>
        /// Turn the vehicule left by the specified number of degrees for each motor. So 360 will do 1 motor turn.
        /// You need to do some math to have the actual vehicule turning fully at 360. It depends of the reduction used.
        /// </summary>
        /// <param name="speed">speed is between -255 and +255</param>
        /// <param name="degrees">degrees to turn each motor</param>
        public void TurnLeft(int speed, int degrees)
        {
            RunMotorSyncDegrees(new BrickPortMotor[2] { portleft, PortRight }, new int[2] { -speed * correctedDir, speed * correctedDir }, new int[2] { degrees, degrees }).Wait();
        }

        /// <summary>
        /// Turn the vehicule right by the specified number of degrees for each motor. So 360 will do 1 motor turn.
        /// You need to do some math to have the actual vehicule turning fully at 360. It depends of the reduction used.
        /// </summary>
        /// <param name="speed">speed is between -255 and +255</param>
        /// <param name="degrees">degrees to turn each motor</param>
        public void TurnRight(int speed, int degrees)
        {
            RunMotorSyncDegrees(new BrickPortMotor[2] { portleft, PortRight }, new int[2] { speed * correctedDir, -speed * correctedDir }, new int[2] { degrees, degrees }).Wait();
        }

        /// <summary>
        /// Turn the vehicule left for a number of milliseconds
        /// </summary>
        /// <param name="speed">speed is between -255 and +255</param>
        /// <param name="timeout">number of milliseconds to run the motors</param>
        public void TrunLeftTime(int speed, int timeout)
        {
            RunMotorSyncTime(new BrickPortMotor[2] { portleft, portright }, new int[2] { -speed * correctedDir, speed * correctedDir }, timeout).Wait();
        }

        /// <summary>
        /// Turn the vehicule right for a number of milliseconds
        /// </summary>
        /// <param name="speed">speed is between -255 and +255</param>
        /// <param name="timeout">number of milliseconds to run the motors</param>
        public void TrunRightTime(int speed, int timeout)
        {
            RunMotorSyncTime(new BrickPortMotor[2] { portleft, portright }, new int[2] { speed * correctedDir, -speed * correctedDir }, timeout).Wait();
        }

        /// <summary>
        /// Stop the vehicule
        /// </summary>
        public void Stop()
        {
            StopMotor((int)PortLeft);
            StopMotor((int)PortRight);
        }

        /// <summary>
        /// Run backward for the specified number of milliseconds
        /// </summary>
        /// <param name="speed">speed is between -255 and +255</param>
        /// <param name="timeout">>number of milliseconds to run the motors</param>
        public void Backward(int speed, int timeout)
        {
            RunMotorSyncTime(new BrickPortMotor[2] { portleft, portright }, new int[2] { speed * correctedDir, speed * correctedDir }, timeout).Wait();
        }

        /// <summary>
        /// Run forward for the specified number of milliseconds
        /// </summary>
        /// <param name="speed">speed is between -255 and +255</param>
        /// <param name="timeout">>number of milliseconds to run the motors</param>
        public void Foreward(int speed, int timeout)
        {
            Backward(-speed, timeout);
        }

        /// <summary>
        /// Return the BrickPortMotor of the left motor 
        /// </summary>
        public BrickPortMotor PortLeft
        { get { return portleft; } }

        /// <summary>
        /// Return the BrickPortMotor of the right motor
        /// </summary>
        public BrickPortMotor PortRight
        { get { return portright; } }

        /// <summary>
        /// Is the vehicule has inverted direction, then true
        /// </summary>
        public bool DirectionOpposite
        {
            get
            {
                return directionOpposite;
            }

            set
            {
                directionOpposite = value;
                if (directionOpposite)
                    correctedDir = -1;
                else
                    correctedDir = 1;
            }
        }

        private Timer timer = null;
        private async Task RunMotorSyncTime(BrickPortMotor[] ports, int[] speeds, int timeout)
        {
            if ((ports == null) || (speeds == null))
                return;
            if (ports.Length != speeds.Length)
                return;
            //create a timer for the needed time to run
            if (timer == null)
                timer = new Timer(RunUntil, ports, TimeSpan.FromMilliseconds(timeout), Timeout.InfiniteTimeSpan);
            else
                timer.Change(TimeSpan.FromMilliseconds(timeout), Timeout.InfiniteTimeSpan);
            //initialize the speed and enable motors
            for (int i = 0; i < ports.Length; i++)
            {
                StartMotor((int)ports[i], speeds[i]);
            }
            bool nonstop = true;
            while (nonstop)
            {
                bool status = false;
                for (int i = 0; i < ports.Length; i++)
                {
                    status |= IsRunning(ports[i]);
                }
                nonstop = status;


            }
        }

        private void RunUntil(object state)
        {
            if (state == null)
                return;
            //stop all motors!
            BrickPortMotor[] ports = (BrickPortMotor[])state;
            for (int i = 0; i < ports.Length; i++)
            {
                StopMotor((int)ports[i]);
            }
            if (timer != null)
            {
                timer.Dispose();
                timer = null;
            }
        }

        private void StopMotor(int port)
        {
            //brick.BrickPi.Motor[port].Speed = 0;
            //brick.BrickPi.Motor[port].Enable = 0;
            brick.set_motor_power((byte)port, 0);
        }

        private void StartMotor(int port, int speed)
        {
            if (speed > 255)
                speed = 255;
            if (speed < -255)
                speed = -255;
            //brick.BrickPi.Motor[port].Speed = speed;
            //brick.BrickPi.Motor[port].Enable = 1;
            brick.set_motor_power((byte)port, speed);
        }

        private async Task RunMotorSyncDegrees(BrickPortMotor[] ports, int[] speeds, int[] degrees)
        {
            if ((ports == null) || (speeds == null) || degrees == null)
                return;
            if ((ports.Length != speeds.Length) && (degrees.Length != speeds.Length))
                return;
            //make sure we have only positive degrees
            for (int i = 0; i < degrees.Length; i++)
                if (degrees[i] < 0)
                    degrees[i] = -degrees[i];
            //initialize the speed and enable motors
            int[] initval = new int[ports.Length];
            for (int i = 0; i < ports.Length; i++)
            {
                try
                {
                    initval[i] = brick.get_motor_encoder((byte)ports[i]); //brick.BrickPi.Motor[(int)ports[i]].Encoder;
                    StartMotor((int)ports[i], speeds[i]);
                }
                catch (Exception)
                { }

            }
            bool nonstop = true;
            while (nonstop)
            {
                try
                {
                    bool status = false;
                    for (int i = 0; i < ports.Length; i++)
                    {
                        if (speeds[i] > 0)
                        {
                            //if (brick.BrickPi.Motor[(int)ports[i]].Encoder >= (initval[i] + degrees[i] * 2))
                            if (brick.get_motor_encoder((byte)ports[i]) >= (initval[i] + degrees[i] * 2))
                            {
                                StopMotor((int)ports[i]);
                            }
                        }
                        else
                        {
                            //if (brick.BrickPi.Motor[(int)ports[i]].Encoder <= (initval[i] - degrees[i] * 2))
                            if (brick.get_motor_encoder((byte)ports[i]) <= (initval[i] - degrees[i] * 2))
                            {
                                StopMotor((int)ports[i]);
                            }
                        }
                        status |= IsRunning(ports[i]);
                    }
                    nonstop = status;
                }
                catch (Exception)
                { }
            }


        }

        /// <summary>
        /// Return true if the vehicule is moving
        /// </summary>
        /// <returns>true if vehicule moving</returns>
        public bool IsRunning()
        {
            if (IsRunning(portleft) || IsRunning(portright))
                return true;
            return false;
        }
        private bool IsRunning(BrickPortMotor port)
        {
            //if (brick.BrickPi.Motor[(int)port].Enable == 0)
            try
            {
                if (brick.get_motor_status((byte)port).Speed == 0)
                    return false;
            }
            catch (Exception)
            { }
            return true;
        }
    }
}
