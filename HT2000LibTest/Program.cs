using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HT2000Lib;

namespace HT2000LibTest
{
    class Program
    {
        static HT2000Manager Manager;
        static HT2000ManagerLibUsb ManagerLU;
        static HT2000ManagerDevNet ManagerDN;

        static void Main(string[] args)
        {
            /*ManagerDN = new HT2000ManagerDevNet();
            if (!ManagerDN.OpenDevice())
            {
                Console.WriteLine("Failed to open device.");
            }
            else
            {
                Console.WriteLine("Opened device successfully.");
            }

            var state = ManagerDN.GetState();
            if (state == null)
            {
                Console.WriteLine("State was null.");
            }
            else
            {
                Manager_StateChanged(null, new HT2000EventArgs(state));
            }*/

            var m = new HT2000ManagerNative();

            while (true)
            {
                Console.WriteLine("Press enter to try...");
                Console.ReadLine();

                HT2000DataPoint[] points = m.DumpData();
                if (points == null)
                {
                    Console.WriteLine("No points returned.");
                }
                else
                {
                    Console.WriteLine($"Got {points.Length} points!");
                    var sb = new StringBuilder();
                    foreach (var p in points)
                    {
                        sb.Append(p.Temperature);
                        sb.Append("\t");
                        sb.Append(p.Humidity);
                        sb.Append("\t");
                        sb.AppendLine(p.CO2.ToString());
                    }
                    File.WriteAllText("data.tsv", sb.ToString());
                }

                //var state = m.GetState();
                //Manager_StateChanged(null, new HT2000EventArgs(state));
            }

            Console.ReadLine();

            /*ManagerLU = new HT2000ManagerLibUsb();
            var state = ManagerLU.GetState();
            if (state == null)
            {
                Console.WriteLine("State was null.");
            }
            else
            {
                Manager_StateChanged(null, new HT2000EventArgs(state));
            }

            Console.ReadLine();*/

            /*Manager = new HT2000Manager();
            Manager.DeviceAttached += Manager_DeviceAttached;
            Manager.DeviceRemoved += Manager_DeviceRemoved;
            Manager.StateChanged += Manager_StateChanged;
            if (Manager.OpenDevice())
            {
                Console.WriteLine("Device opened OK.");
                while (true)
                {
                    var state = Manager.GetState();
                    if (state == null)
                    {
                        Console.WriteLine("State was null.");
                    }
                    else
                    {
                        Manager_StateChanged(null, new HT2000EventArgs(state));
                    }
                    Thread.Sleep(1000);
                }
            }
            else
            {
                Console.WriteLine("Failed to open device.");
            }*/
        }

        private static void Manager_StateChanged(object sender, HT2000EventArgs e)
        {
            //Console.WriteLine($"Stage changed: CO2 = {e.State.CO2Level}, Temp = {e.State.Temperature}, Hum = {e.State.Humidity}");
            Console.WriteLine("Stage changed: " + e.State.ToString());
        }

        private static void Manager_DeviceAttached(object sender, EventArgs e)
        {
            Console.WriteLine("Device attached.");
        }

        private static void Manager_DeviceRemoved(object sender, EventArgs e)
        {
            Console.WriteLine("Device removed.");
        }
    }
}
