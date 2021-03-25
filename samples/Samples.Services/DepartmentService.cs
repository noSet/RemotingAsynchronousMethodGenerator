using System;
using System.Collections.Generic;
using RemotingAsynchronousMethodGenerator;

namespace Samples.Services
{
    [GenerateAsynchronousMethods]
    public class DepartmentService : MarshalByRefObject
    {
        public IEnumerable<Employee> GetEmployeesByDepartmentName(string name)
        {
            return new[] {
                new  Employee (){ Id = 1, Name = "zhangsan" },
                new  Employee (){ Id = 2, Name = "lisi" },
            };
        }

        public string GetName()
        {
            return "Finance dept.";
        }
    }
}
