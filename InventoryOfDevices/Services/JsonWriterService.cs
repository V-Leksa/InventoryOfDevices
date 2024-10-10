using InventoryOfDevices.Models;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Windows;

namespace InventoryOfDevices.Services
{
    public class JsonWriterService
    {
        public void SaveToJson(ObservableCollection<Device> devices, string filePath)
        {
            try
            {
                string json = System.Text.Json.JsonSerializer.Serialize(devices, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filePath, json);
                MessageBox.Show("Файл успешно записан.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при записи в файл: " + ex.Message);
            }
        }

        public void SaveToBackupJson(ObservableCollection<Device> devices)
        {
            // Получение имени и пути резервного файла
            string backupDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Backup");
            if (!Directory.Exists(backupDirectory))
            {
                Directory.CreateDirectory(backupDirectory);
            }
            string backupFileName = $"backup_{DateTime.Now:yyyyMMdd_HHmmss}.json";
            string backupFilePath = Path.Combine(backupDirectory, backupFileName);

            SaveToJson(devices, backupFilePath);
        }
        
    }
}
