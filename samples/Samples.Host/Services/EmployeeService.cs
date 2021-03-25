using System;
using System.Collections.Generic;
using System.Linq;
using Samples.Services;

namespace Samples.Host.Services
{
    public class EmployeeService : MarshalByRefObject, IEmployeeService
    {
        private readonly List<Employee> _employees = new List<Employee>();

        public bool AddEmployee(Employee employee)
        {
            if (string.IsNullOrWhiteSpace(employee.Name))
            {
                return false;
            }

            if (this._employees.Any(e => e.Id == employee.Id))
            {
                return false;
            }

            this._employees.Add(employee);
            return true;
        }

        public Employee GetEmployee(int id)
        {
            return this._employees.SingleOrDefault(e => e.Id == id);
        }
    }
}
