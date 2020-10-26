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

namespace Victim
{
    class Program
    {
        public static List<DateTime> time = new List<DateTime>();
        static string password;
        public static void Main(string[] args)
        {
            Random r = new Random();

            // password of victim
            password = "";
            password = GetPassword(r);

            //port of victim
            int port = 0; //port 
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();
            port = GetPort(r, port, tcpConnInfoArray);
            Console.WriteLine("Server listening on port " + port + ", password is " + password);
            TcpListener TCPserver = new TcpListener(IPAddress.Any, port);
            TCPserver.Start();
            LoopClients(TCPserver);
        }

        private static void checkIfTenTimesInSecondSuccessConnection(string msg)
        {

            if (time.Count >= 10)
            {
                if (time[time.Count - 1].ToString().Equals(time[time.Count - 10].ToString()))
                {
                    Console.WriteLine(msg);
                    System.Environment.Exit(1);
                }
            }

        }

        private static void LoopClients(TcpListener TCPserver)
        {
            while (true)
            {
                TcpClient client = TCPserver.AcceptTcpClient();
                Thread t = new Thread(new ParameterizedThreadStart(HandleClient));
                t.Start(client);

            }
        }

        public static void HandleClient(object obj)
        {
            // retrieve client from parameter passed to thread
            TcpClient client = (TcpClient)obj;
            NetworkStream ns = client.GetStream();
            byte[] myWriteBuffer = Encoding.ASCII.GetBytes("Please enter your password\r\n");
            ns.Write(myWriteBuffer, 0, myWriteBuffer.Length);
            ns.Flush();
            byte[] bytes = new byte[client.ReceiveBufferSize];
            ns.Read(bytes, 0, bytes.Length);
            string msg = Encoding.ASCII.GetString(bytes).Replace("\0", string.Empty);
            ns.Flush();
            if (msg.Equals(password.Trim() + "\r\n"))
            {
                myWriteBuffer = Encoding.ASCII.GetBytes("Access granted\r\n");
                ns.Write(myWriteBuffer, 0, myWriteBuffer.Length);
                ns.Flush();
                bytes = new byte[1024];
                ns.Read(bytes, 0, bytes.Length);
                msg = Encoding.ASCII.GetString(bytes).Replace("\0", string.Empty);
                ns.Flush();
                time.Add(DateTime.Now);
                checkIfTenTimesInSecondSuccessConnection(msg);
            }

        }

        private static string GetPassword(Random r)
        {
            string password = "";
            for (int i = 0; i < 6; i++)
            {
                int num = r.Next(0, 26);
                password = password + (char)('a' + num);
            }
            return password;
        }

        private static int GetPort(Random r, int port, TcpConnectionInformation[] tcpConnInfoArray)
        {
            bool isAvailable = false;
            while (!isAvailable)
            {
                port = r.Next(0, 60000);
                isAvailable = true;
                foreach (TcpConnectionInformation tcpi in tcpConnInfoArray)
                {
                    if (tcpi.LocalEndPoint.Port == port)
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



