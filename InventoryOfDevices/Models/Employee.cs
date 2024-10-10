
namespace InventoryOfDevices.Models
{
    public class Employee
    {
        public string? Name {  get; set; }
        public string? SurName {  get; set; }
        public string? Login {  get; set; }
        public string? Password {  get; set; }

        public Employee(string? name, string? surName, string? login, string? password)
        {
            Name = name;
            SurName = surName;
            Login = login;
            Password = password;
        }

        public Employee()
        {
        }
    }

}
