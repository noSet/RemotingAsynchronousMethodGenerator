using System;
using System.Diagnostics;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Threading;
using System.Threading.Tasks;
using Samples.Services;

namespace Samples.Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            for (int i = 0; i < 5; i++)
            {
                Thread.Sleep(100);
            }

            TcpChannel channel = new TcpChannel(0);
            ChannelServices.RegisterChannel(channel, false);

            Console.WriteLine("client start...");

            IEmployeeService employeeService = CreateAsyncProxy<IEmployeeService>();

            Debug.Assert(await employeeService.AddEmployeeAsync(new Employee() { Id = 1, Name = "zhangsan" }));
            Debug.Assert(!employeeService.AddEmployee(new Employee() { Id = 1, Name = "zhangsan" }));
            Debug.Assert(await employeeService.AddEmployeeAsync(new Employee() { Id = 2, Name = "lisi" }));

            Debug.Assert(employeeService.GetEmployee(1).Name == "zhangsan");
            Debug.Assert((await employeeService.GetEmployeeAsync(2)).Name == "lisi");

            DepartmentService departmentService = CreateAsyncProxy<DepartmentService>();
            string name = await departmentService.GetNameAsync();
            foreach (var employee in await departmentService.GetEmployeesByDepartmentNameAsync(name))
            {
                Console.WriteLine(employee.Name);
            }

            Console.ReadKey();
        }

        private static T CreateAsyncProxy<T>(string url = default)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                url = "tcp://localhost:8826/" + typeof(T).Name;
            }

            var proxy = (T)Activator.GetObject(typeof(T), url);

            return proxy;
        }
    }
}
