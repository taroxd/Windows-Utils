using System;
using System.Linq;
using System.Net.NetworkInformation;

namespace GetInterfaceIP
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.Error.WriteLine("Usage: GetInterfaceIP InterfaceName");
                return 1;
            }

            string search_interface_name = args[0];

            var all_ni = NetworkInterface.GetAllNetworkInterfaces();
            var named_ni = all_ni.FirstOrDefault(ni => ni.Name == search_interface_name);

            if (named_ni == null)
            {
                Console.Error.WriteLine("Name {0} not found.", search_interface_name);
                Console.Error.WriteLine("All possible values are:");
                foreach (var ni in all_ni)
                {
                    Console.Error.WriteLine(ni.Name);
                }
                return 1;
            }

            string ip = named_ni.GetIPProperties().UnicastAddresses
                .Select(a => a.Address)
                .Where(a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                .Select(a => a.ToString())
                .FirstOrDefault();

            if (ip == null)
            {
                Console.Error.WriteLine("IP not found.");
                return 1;
            }

            Console.WriteLine(ip);
            return 0;
        }
    }
}
