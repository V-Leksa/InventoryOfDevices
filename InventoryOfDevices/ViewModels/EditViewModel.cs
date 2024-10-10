using InventoryOfDevices.Infrastructure.Commands;
using InventoryOfDevices.Models;
using InventoryOfDevices.Services;
using InventoryOfDevices.Views.Windows;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;

namespace InventoryOfDevices.ViewModels
{
    public class EditViewModel : ViewModelBase, IDataErrorInfo
    {
        #region Поля

        private ObservableCollection<Device> _devices;
        private int _deviceId;
        private bool _sendJsonSuccess = false;
        private string _deviceName;
        private int _quantity;
        private string _location;
        private string _barcodeValue;
        private string _employeeSurname;
        private string _employeeName;
        private string _description;
        private string _categoryName;
        private string _manufacturerName;
        private string _selectedCategory;
        private Device _selectedDevice;
        //словарь _errors, который будет хранить сообщения об ошибках для каждого поля
        private Dictionary<string, string> _errors = new Dictionary<string, string>();
        //реализация свойства Error из интерфейса IDataErrorInfo, которое возвращает сообщение об ошибках, если словарь _errors содержит ошибки.
        public string Error => _errors.Count > 0 ? "Ошибки в данных" : null;

        #region Команды
        public ICommand SendJsonCommand { get; }
        public ICommand EditCommand { get; }
        #endregion

        #endregion

        #region Конструктор
        public EditViewModel(Device selectedDevice)
        {
            SelectedDevice = selectedDevice;

            //Инициализация свойств для редактирования данными выбранного устройства
            DeviceId = selectedDevice.DeviceId;
            DeviceName = selectedDevice.DeviceName;
            Quantity = selectedDevice.Quantity;
            Location = selectedDevice.Location;
            BarcodeValue = selectedDevice.BarcodeValue;
            EmployeeSurname = selectedDevice.EmployeeSurname;
            EmployeeName = selectedDevice.EmployeeName;
            Description = selectedDevice.Description;
            SelectedCategory = selectedDevice.CategoryName;
            ManufacturerName = selectedDevice.ManufacturerName;

            
            EditDeviceWindow current = new EditDeviceWindow();
            current.DataContext = this;
            current.Show();

            EditCommand = new LambdaCommand(OnEditExecuted, CanEditExecute);

        }
        #endregion

        #region Свойства

        public bool SendJsonSuccess
        {
            get => _sendJsonSuccess;
            set => Set(ref _sendJsonSuccess, value);
        }

        #region Коллекция оборудования

        public ObservableCollection<Device> Devices
        {
            get => _devices;
            set
            {
                Set(ref _devices, value);
                OnPropertyChanged(nameof(Devices));
            }

        }
        #endregion

        #region Свойства для добавления и редактирования

        #region Инвентарный номер

        public int DeviceId
        {
            get => _deviceId;
            set => Set(ref _deviceId, value);

        }
        #endregion

        #region Наименование оборудования

        public string DeviceName
        {
            get => _deviceName;
            set => Set(ref _deviceName, value);

        }
        #endregion

        #region Количество

        public int Quantity
        {
            get => _quantity;
            set => Set(ref _quantity, value);

        }
        #endregion

        #region Местоположение

        public string Location
        {
            get => _location;
            set => Set(ref _location, value);

        }

        #endregion

        #region Штрих-код

        public string BarcodeValue
        {
            get => _barcodeValue;
            set => Set(ref _location, value);

        }
        #endregion

        #region Фамилия сотрудника

        public string EmployeeSurname
        {
            get => _employeeSurname;
            set => Set(ref _employeeSurname, value);

        }
        #endregion

        #region Имя сотрудника

        public string EmployeeName
        {
            get => _employeeName;
            set => Set(ref _employeeName, value);

        }
        #endregion

        #region Описание

        public string Description
        {
            get => _description;
            set => Set(ref _description, value);

        }
        #endregion

        public List<string> Categories
        {
            get => _categories;
            set
            {
                _categories = value;
                OnPropertyChanged();
            }
        }

        public string CategoryName
        {
            get => _categoryName;
            set => Set(ref _categoryName, value);

        }

        #region Производитель

        public string ManufacturerName
        {
            get => _manufacturerName;
            set => Set(ref _manufacturerName, value);

        }
        #endregion

        public string SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropertyChanged();
            }
        }

        public Device SelectedDevice
        {
            get => _selectedDevice;
            set => Set(ref _selectedDevice, value);
        }
        #endregion


        #region Категория
        private List<string> _categories = new List<string>
        {
            "Компьютер",
            "Мобильный телефон",
            "Планшет",
            "Клавиатура",
            "Монитор"
        };

        #endregion


        #endregion

        #region Выполнение команды редактирования
        private bool CanEditExecute(object p)
        {
            return ValidateData();
        }
        private void OnEditExecuted(object parameter)
        {
            // Проверка валидации данных
            if (!ValidateData())
                return;

            // Обновление свойств выбранного оборудования
            UpdateDeviceProperties();

            // Сохранение изменений
            SaveChanges(SelectedDevice);

            // Отправка события об изменении для обновления в главной модели
            DeviceEdited?.Invoke(SelectedDevice);

            MessageBox.Show("Успешно");

            SendJsonCommand.Execute(null);
        }
        // Событие для обновления данных в главной модели
        public event Action<Device> DeviceEdited;

        private void SaveChanges(Device device)
        {
            // Перезапись данных в JSON файл
            JsonWriterService jsonWriter = new JsonWriterService();
            jsonWriter.SaveToJson(Devices, "Device.json");
        }

        #endregion

        #region Обновление всех свойств выбранного оборудования
        private void UpdateDeviceProperties()
        {
            // Проверка валидации данных
            if (!ValidateData())
            {
                return;
            }

            SelectedDevice.DeviceId = DeviceId;
            SelectedDevice.DeviceName = DeviceName;
            SelectedDevice.Quantity = Quantity;
            SelectedDevice.Location = Location;
            SelectedDevice.BarcodeValue = BarcodeValue;
            SelectedDevice.EmployeeSurname = EmployeeSurname;
            SelectedDevice.EmployeeName = EmployeeName;
            SelectedDevice.Description = Description;
            SelectedDevice.CategoryName = SelectedCategory;
            SelectedDevice.ManufacturerName = ManufacturerName;

        }
        #endregion

        #region Валидация данных

        //индексатор, который позволяет получить сообщение об ошибке для конкретного поля по его имени.
        public string this[string columnName]
        {
            get
            {
                if (_errors.TryGetValue(columnName, out string error))
                    return error;
                return null;
            }
        }

        //Проверяет каждое поле на пустоту или некорректные значения и добавляет сообщения об ошибках в словарь _errors.
        //Возвращает true, если ошибок нет, и false, если есть ошибки.
        private bool ValidateData()
        {
            _errors.Clear();
            bool isValid = true;

            // Проверка на уникальность инвентарного номера DeviceId
            if (Devices.Any(d => d.DeviceId == DeviceId))
            {
                _errors[nameof(DeviceId)] = "Инвентарный номер уже существует";
                isValid = false;
            }

            // Проверка, чтобы инвентарный номер не был равен 0
            if (DeviceId == 0)
            {
                _errors[nameof(DeviceId)] = "Инвентарный номер не может быть равен 0";
                isValid = false;
            }

            // Проверка, чтобы инвентарный номер был числом
            if (!int.TryParse(DeviceId.ToString(), out _))
            {
                _errors[nameof(DeviceId)] = "Инвентарный номер должен быть числом";
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(DeviceName))
            {
                _errors[nameof(DeviceName)] = "Название не заполнено";
                isValid = false;
            }

            // Проверка, чтобы количество было числом
            if (!int.TryParse(Quantity.ToString(), out _))
            {
                _errors[nameof(Quantity)] = "Количество должно быть числом";
                isValid = false;
            }

            if (Quantity <= 0)
            {
                _errors[nameof(Quantity)] = "Количество должно быть больше 0";
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(Location))
            {
                _errors[nameof(Location)] = "Местоположение не заполнено";
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(EmployeeSurname))
            {
                _errors[nameof(EmployeeSurname)] = "Фамилия не заполнена";
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(EmployeeName))
            {
                _errors[nameof(EmployeeName)] = "Имя не заполнено";
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(Description))
            {
                _errors[nameof(Description)] = "Описание не заполнено";
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(SelectedCategory))
            {
                _errors[nameof(SelectedCategory)] = "Категория не заполнена";
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(ManufacturerName))
            {
                _errors[nameof(ManufacturerName)] = "Производитель не заполнен";
                isValid = false;
            }

            if (!isValid)
            {
                string errorMessage = "Пожалуйста, исправьте следующие ошибки:\n\n";
                foreach (var error in _errors)
                {
                    errorMessage += $"{error.Value}\n";
                }
                MessageBox.Show(errorMessage, "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return isValid;

        }

        #endregion

        #region Отправка данных на сервер при добавлении

        private async Task OnSendJsonExecutedAsync()
        {
            // Сериализация данных в JSON строку
            string json = JsonSerializer.Serialize(_devices);

            // Создание строки с типом данных JSON
            string contentType = "application/json";

            // Создание объекта ByteArrayContent для отправки файла
            HttpContent content = new StringContent(json);


            // Отправка POST запроса на сервер с JSON данными
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.PostAsync("http://192.168.10.197:8080/UpdateDevices", content);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    MessageBox.Show("Файл успешно отправлен на сервер. Ответ от сервера: " + responseContent);
                    _sendJsonSuccess = true;
                }
                else
                {
                    MessageBox.Show("Ошибка при отправке файла на сервер. Код ошибки: " + response.StatusCode);
                    _sendJsonSuccess = false;
                }
            }

        }

        #endregion

    }

}
