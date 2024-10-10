using InventoryOfDevices.Infrastructure.Commands;
using InventoryOfDevices.Models;
using InventoryOfDevices.Views.Windows;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;

namespace InventoryOfDevices.ViewModels
{
    public class AddViewModel : ViewModelBase, IDataErrorInfo
    {
        #region Поля

        private ObservableCollection<Device> _devices;
        private int _deviceId;
        private string _deviceName;
        private int _quantity;
        private string _location;
        private string _barcodeValue;
        private string _employeeSurname;
        private string _employeeName;
        private string _description;
        private ObservableCollection<string> _categories;
        private string _categoryName;
        private string _manufacturerName;
        private string _selectedCategory;
        private bool _sendJsonSuccess = false;

        #region Команды
        public ICommand ConfirmCommand { get; }
        public ICommand SendJsonCommand { get; }

        #endregion

        #endregion

        #region Конструктор

        public AddViewModel(ObservableCollection<Device> mainDeviceList)
        {
            Devices = mainDeviceList;

            // Создание нового ObservableCollection и добавление значений
            Categories = new ObservableCollection<string>
            {
            "Компьютер",
            "Мобильный телефон",
            "Планшет",
            "Клавиатура",
            "Монитор"
            };

            ConfirmCommand = new LambdaCommand(OnConfirmExecuted, CanConfirmExecute);

            AddDeviceWindow current = new AddDeviceWindow();
            current.DataContext = this;
            current.Show();

        }

        #endregion

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
            set => Set(ref _barcodeValue, value);

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

        #region Категория
        
        public ObservableCollection<string> Categories
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

        #endregion

        #region Производитель
        
        public string ManufacturerName
        {
            get => _manufacturerName;
            set => Set(ref _manufacturerName, value);

        }
        #endregion

        #region Выбранная категория
        public string SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #endregion

        public bool SendJsonSuccess
        {
            get => _sendJsonSuccess;
            set => Set(ref _sendJsonSuccess, value);
        }

        #region Валидация данных

        //словарь _errors, который будет хранить сообщения об ошибках для каждого поля
        private Dictionary<string, string> _errors = new Dictionary<string, string>();

        //реализация свойства Error из интерфейса IDataErrorInfo, которое возвращает сообщение об ошибках, если словарь _errors содержит ошибки.
        public string Error => _errors.Count > 0 ? "Ошибки в данных" : null;

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

            if (!IsBarcodeUnique(BarcodeValue))
            {
                _errors[nameof(BarcodeValue)] = "Штрих-код должен быть уникальным";
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

        private bool IsBarcodeUnique(string barcodeValue)
        {
            // Проверка, что штрих-код не используется в существующих устройствах
            return Devices == null || !Devices.Any(d => d.BarcodeValue == barcodeValue);
        }

        #endregion

        #region Команда для подтверждения добавления
        
        private bool CanConfirmExecute(object p)
        {
            return ValidateData();
        }
        private void OnConfirmExecuted(object p)
        {
            // Генерация уникального штрих-кода
            string BarcodeValue = GenerateBarcode();

            //Создание нового оборудования на основе введенных данных
            var newDevice = new Device 
            (   
                DeviceId,
                DeviceName,
                Quantity,
                Location,
                BarcodeValue, // Использование сгенерированного штрих-кода
                EmployeeSurname,
                EmployeeName,
                Description,
                SelectedCategory,
                ManufacturerName
            );

            // Проверка валидации данных
            if (!ValidateData())
                return;

            newDevice.CategoryName = SelectedCategory; // Установка выбранной категории в свойство CategoryName

            Devices.Add(newDevice);

            MessageBox.Show("Успешно");

            //запрос на обновление данных
            SendJsonCommand.Execute(null);
        }

        private string GenerateBarcode()
        {
            Random random = new Random();
            string newBarcode;
            bool isUnique = false;

            do
            {
                // Генерация штрих-кода частями
                string firstPart = random.Next(1000000, 9999999).ToString();
                string secondPart = random.Next(100000, 999999).ToString();
                newBarcode = $"{firstPart}{secondPart}";

                // Проверка уникальности штрих-кода
                isUnique = !Devices.Any(d => d.BarcodeValue == newBarcode);
            } 
            while (!isUnique);

            return newBarcode;
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
