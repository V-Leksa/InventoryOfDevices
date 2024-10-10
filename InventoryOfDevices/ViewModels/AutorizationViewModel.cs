using InventoryOfDevices.Infrastructure.Commands.BaseCommand;
using InventoryOfDevices.Models;
using InventoryOfDevices.Views.Windows; //для авторизации через сервер
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;

namespace InventoryOfDevices.ViewModels
{
    public class AutorizationViewModel : ViewModelBase
    {
        #region Поля

        private string _login;
        private string _password;
        public ICommand EnterCommand { get; }

        #endregion

        #region Конструктор
        public AutorizationViewModel()
        {
            EnterCommand = new AsyncCommand(OnEnterCommandExecutedAsync);
        }
        #endregion

        #region Свойства

        public string Login
        {
            get => _login;
            set => Set(ref _login, value);

        }

        public string Password
        {
            get => _password;
            set => Set(ref _password, value);

        }

        #endregion

        #region Словарь с логинами и паролями для входа
        Dictionary<string, string> loginPasswords = new Dictionary<string, string>
            {
               {"qwerty", "123456"},
               {"smit78", "Hypt651"},
               {"gndsomR21", "12hfd8Ybcdf"}
            };
        #endregion

        #region Генерация данных
        public ObservableCollection<Device> CreateTestData()
        {
            Random rnd = new Random();
            string[] devNames =
            {
              "Asus", "Samsung", "Apple", "Xaomi", "LG"
            };

            string[] locations =
            {
              "400", "401", "402", "403", "404", "405", "406", "407", "408", "409", "410", "Дом"
            };

            string[] еmployeeNames =
            {
              "Василий", "Александр", "Кирилл", "Иван", "Петр"
            };

            string[] еmployeeSurNames =
            {
              "Иванов", "Петров", "Сидоров", "Ермаков", "Васильев"
            };

            string[] descriptions =
            {
              "Мощное оборудование", "Высокочастотное", "Высокоскоростное", "Рабочая", "Сломано, требует ремнота"
            };

            string[] categoryNames =
            {
              "Компьютер", "Мобильный телефон", "Планшет", "Клавиатура", "Монитор"
            };

            string[] manufacturerName =
            {
              "Китай", "Германия", "Россия", "Казахстан", "Индонезия"
            };

            ObservableCollection<Device> devices = new ObservableCollection<Device>();

            int counter = 1;
            foreach (var item in devNames)
            {
                for (int i = 0; i < 10; i++)
                {
                    string currentLocation = locations[rnd.Next(0, locations.Length)];
                    string currentBarcode = $"{rnd.Next(1000000, 9999999)}{rnd.Next(100000, 999999)}";
                    string currentEmployeeName = еmployeeNames[rnd.Next(0, еmployeeNames.Length)];
                    string currentеmployeeSurName = еmployeeSurNames[rnd.Next(0, еmployeeSurNames.Length)];
                    string currentDescription = descriptions[rnd.Next(0, descriptions.Length)];
                    string currentCategoryName = categoryNames[rnd.Next(0, categoryNames.Length)];
                    string currentManufacturerName = manufacturerName[rnd.Next(0, manufacturerName.Length)];

                    devices.Add(new Device(counter, item, rnd.Next(3, 20),
                        currentLocation, currentBarcode, currentEmployeeName,
                        currentеmployeeSurName, currentDescription, currentCategoryName, currentManufacturerName));
                    counter++;
                }
            }

            return devices;
        }
        #endregion

        //#region Выполнение команды входа при обращении к серверу
        //private async Task OnEnterCommandExecutedAsync()
        //{
        //    Window autorizationViewModel = Application.Current.MainWindow;

        //    // Отправка логина и пароля на сервер для аутентификации
        //    bool isAuthenticated = await AuthenticateUser(_login, _password);

        //    if (isAuthenticated)
        //    {
        //        // Получение списка устройств с сервера
        //        var devices = await GetDevicesFromServer();
        //        var employee = await GetEmployeesFromServer();
        //        var categories = await GetCategoriesFromServer();

        //        //Отображение окна с устройствами
        //        DisplayWindow(devices);

        //        autorizationViewModel.Close();
        //    }
        //    else
        //    {
        //        MessageBox.Show("Неверный логин или пароль");
        //    }

        //}
        //#endregion

        #region Выполнение команды при генерации данных

        private async Task OnEnterCommandExecutedAsync()
        {
            Window autorizationViewModel = Application.Current.MainWindow;

            if (loginPasswords.ContainsKey(Login) && loginPasswords[Login] == Password)
            {
                DisplayWindow(CreateTestData());
                autorizationViewModel.Close();
            }
            else
            {
                MessageBox.Show("Неверный логин или пароль");
            }

        }
        #endregion

        #region Отправка на сервер введенных логина и пароля
        private async Task<bool> AuthenticateUser(string login, string password)
        {
            using (HttpClient client = new HttpClient())
            {
                // Сериализация данных в JSON строку
                string[] employee = { login, password };
                string jsonData = JsonSerializer.Serialize(employee);

                // Отправка логина и пароля на сервер для аутентификации
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                var response = await client.PostAsync("http://192.168.10.197:8080/authetificate", content);

                return response.IsSuccessStatusCode;
            }
        }
        #endregion

        #region Запрос на сервер списка оборудования
        private async Task<ObservableCollection<Device>> GetDevicesFromServer()
        {
            using (HttpClient client = new HttpClient())
            {
                // Получение списка устройств с сервера
                var response = await client.GetAsync("http://192.168.10.197:8080/Device");

                if (response.IsSuccessStatusCode)
                {
                    // Десериализация JSON данных в коллекцию устройств
                    string jsonData = await response.Content.ReadAsStringAsync();
                    var devices = JsonSerializer.Deserialize<ObservableCollection<Device>>(jsonData);

                    // Преобразование данных в соответствии со структурой класса Device
                    foreach (var device in devices)
                    {
                        //device.CategoryName = device.Category?.CategoryName;
                        //device.ManufacturerName = device.Manufacturer?.ManufacturerName;
                        //device.EmployeeName = device.Employee?.Name;
                        //device.EmployeeSurname = device.Employee?.SurName;
                        //device.BarcodeValue = device?.Barcode;

                        //// Удаление ненужных свойств
                        //device.Category = null;
                        //device.Manufacturer = null;
                        //device.Employee = null;
                        //device.Barcode = null;
                    }

                    return devices;
                }
                else
                {
                    MessageBox.Show("Ошибка при получении данных с сервера");
                    return new ObservableCollection<Device>();
                }
            }
        }
        #endregion

        #region Запрос на сервер списка категорий
        private async Task<ObservableCollection<Category>> GetCategoriesFromServer()
        {
            using (HttpClient client = new HttpClient())
            {
                // Получение списка категорий с сервера
                var response = await client.GetAsync("http://192.168.10.197:8080/Categories");

                if (response.IsSuccessStatusCode)
                {
                    // Десериализация JSON данных в коллекцию категорий
                    string jsonData = await response.Content.ReadAsStringAsync();
                    var categories = JsonSerializer.Deserialize<ObservableCollection<Category>>(jsonData);
                    return categories;
                }
                else
                {
                    MessageBox.Show("Ошибка при получении данных о категориях с сервера");
                    return new ObservableCollection<Category>();
                }
            }
        }
        #endregion

        #region Запрос на сервер списка сотрудников

        private async Task<ObservableCollection<Employee>> GetEmployeesFromServer()
        {
            using (HttpClient client = new HttpClient())
            {
                // Получение списка сотрудников с сервера
                var response = await client.GetAsync("http://192.168.10.197:8080/Employees");

                if (response.IsSuccessStatusCode)
                {
                    // Десериализация JSON данных в коллекцию сотрудников
                    string jsonData = await response.Content.ReadAsStringAsync();
                    var employees = JsonSerializer.Deserialize<ObservableCollection<Employee>>(jsonData);
                    return employees;
                }
                else
                {
                    MessageBox.Show("Ошибка при получении данных о сотрудниках с сервера");
                    return new ObservableCollection<Employee>();
                }
            }
        }
        #endregion

        #region Отображение главного окна с полученным списком оборудования
        private async void DisplayWindow(ObservableCollection<Device> devices)
        {
            
            MainViewModel nextWindow = new MainViewModel(devices);

        }
        #endregion
    }
}
