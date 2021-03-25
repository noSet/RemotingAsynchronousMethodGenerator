using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using Samples.Host.Services;
using Samples.Services;

namespace Samples.Host
{
    class Program
    {
        static void Main(string[] args)
        {
            TcpChannel channel = new TcpChannel(8826);
            ChannelServices.RegisterChannel(channel, false);

            RemotingConfiguration.RegisterWellKnownServiceType(typeof(EmployeeService), nameof(IEmployeeService), WellKnownObjectMode.Singleton);
            RemotingConfiguration.RegisterWellKnownServiceType(typeof(DepartmentService), nameof(DepartmentService), WellKnownObjectMode.Singleton);

            Console.WriteLine("host start...");
            Console.ReadKey();
        }
    }
}
