using InventoryOfDevices.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using System.Text.Encodings.Web;

namespace InventoryOfDevices.Services
{
    public class JsonFileReader
    {
        public static ObservableCollection<Device> GetDataFromJson()
        {
            List<Device> equipments;
            List<Category> categories;

            JsonSerializerOptions options = new JsonSerializerOptions
            {
                NumberHandling = JsonNumberHandling.AllowReadingFromString,
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            using (StreamReader reader = new StreamReader("Device.json"))
            {
                string json = reader.ReadToEnd();
                equipments = JsonSerializer.Deserialize<List<Device>>(json, options);
            }

            ObservableCollection<Device> temp = new ObservableCollection<Device>(equipments);

            return temp;
        }
    }
}
