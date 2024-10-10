
namespace InventoryOfDevices.Models
{
    public class Device
    {
        public int DeviceId { get; set; }
        public string? DeviceName { get; set; }
        public int Quantity { get; set; }
        public string Location { get; set; }
        public string BarcodeValue { get; set; }
        public string? EmployeeName { get; set; }
        public string? EmployeeSurname { get; set; }
        public string? Description { get; set; }
        public string? CategoryName { get; set; }
        public string? ManufacturerName { get; set; }

        public Device(int deviceId, string? deviceName, int quantity, string location, string barcodeValue, string? employeeName, string? employeeSurname, string? description, string? categoryName, string? manufacturerName)
        {
            DeviceId = deviceId;
            DeviceName = deviceName;
            Quantity = quantity;
            Location = location;
            BarcodeValue = barcodeValue;
            EmployeeName = employeeName;
            EmployeeSurname = employeeSurname;
            Description = description;
            CategoryName = categoryName;
            ManufacturerName = manufacturerName;
        }

    }
}
