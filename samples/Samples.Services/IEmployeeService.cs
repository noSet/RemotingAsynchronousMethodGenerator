using RemotingAsynchronousMethodGenerator;

namespace Samples.Services
{
    [GenerateAsynchronousMethods]
    public interface IEmployeeService
    {
        Employee GetEmployee(int id);

        bool AddEmployee(Employee employee);
    }
}
