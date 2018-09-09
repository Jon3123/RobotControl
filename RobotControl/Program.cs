using System;
using System.Net;
using System.Net.Sockets;
using SharpDX.XInput;
using System.Text;
using System.Threading;

namespace RobotController
{
    class Program
    {
        public static double point_direction(int x1, int y1, int x2, int y2)
        {
            double degrees = 0;
            float adjacent;
            adjacent = x2 - x1;
            float opposite;
            opposite = y2 - y1;
            degrees = Math.Atan2(opposite,adjacent);
            degrees = -degrees * (180/Math.PI);
            if (opposite < 0)
                degrees = 360-degrees;
            return Math.Abs(degrees);

        }
  
        public static int getSpeed(double x , double y)
        {
            int speed = 0;
            speed = (int)(Math.Sqrt(x * x + y * y));
            return speed;
        }
        public static void sendMessage(UdpClient udp , string type , int data)
        {
            string packet = '~'+ "X" + type + data.ToString();
            byte[] bytes = Encoding.ASCII.GetBytes(packet);
            udp.Send(bytes, bytes.Length);
        }
        static void Main(string[] args)
        {
            Boolean running = true;
            Console.WriteLine("STARTING PROGRAM");
            Console.Write("CONNECTING CONTROLLER ... ");
            XInputController c = new XInputController();
            Console.WriteLine((c.connected) ? "CONNECTED!" : "CONTROLLER FAILED TO CONNECT!");
            if (c.connected == false)
            {
                Console.WriteLine("ENDING PROGRAM");
                return;
            }
            Console.Write("TRYING TO CONNECT TO ARDUINO ... ");
            UdpClient udp = new UdpClient(80);
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse("192.168.4.1"),80);
            udp.Connect(ep);
            Console.WriteLine("POSSIBLY A SUCCESS.");
            
            while (running)
            {

                int degree = (int)point_direction(0, 0, (int)c.leftThumb.x, (int)c.leftThumb.y);
                int speed = getSpeed(c.leftThumb.x, c.leftThumb.y);
                //Console.WriteLine(value);
                if (Math.Floor(c.leftThumb.x) == 0 && Math.Floor(c.leftThumb.y) == 0)
                {
                    sendMessage(udp, "s", 1);
                }
                else
                {
                    sendMessage(udp, "l", degree);
                    sendMessage(udp, "n", speed);
                    //Console.WriteLine(degree + " " + speed);
                }
              
                c.Update();
                Thread.Sleep(10);
            }
        }
    }
}
class Point
{
    public double x;
    public double y;
    public Point(float a, float b)
    {
        x = a;
        y = b;
    }
}
class XInputController
{
    Controller controller;
    Gamepad gamepad;
    public bool connected = false;
    public int deadband = 4000;
    public Point leftThumb = new Point(0, 0), rightThumb = new Point(0, 0);

    public float leftTrigger, rightTrigger;

    public XInputController()
    {
        controller = new Controller(UserIndex.One);
        connected = controller.IsConnected;
    }

    // Call this method to update all class values
    public void Update()
    {
        if (!connected)
            return;

        gamepad = controller.GetState().Gamepad;
        leftThumb.x = (Math.Abs((float)gamepad.LeftThumbX) < deadband) ? 0 : (float)gamepad.LeftThumbX / short.MinValue * -100;
        leftThumb.y = (Math.Abs((float)gamepad.LeftThumbY) < deadband) ? 0 : (float)gamepad.LeftThumbY / short.MaxValue * 100;
        rightThumb.y = (Math.Abs((float)gamepad.RightThumbX) < deadband) ? 0 : (float)gamepad.RightThumbX / short.MaxValue * 100;
        rightThumb.x = (Math.Abs((float)gamepad.RightThumbY) < deadband) ? 0 : (float)gamepad.RightThumbY / short.MaxValue * 100;
        leftTrigger = gamepad.LeftTrigger;
        rightTrigger = gamepad.RightTrigger;
    }
}