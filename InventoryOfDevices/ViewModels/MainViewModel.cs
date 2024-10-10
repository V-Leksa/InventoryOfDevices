using InventoryOfDevices.Infrastructure.Commands;
using InventoryOfDevices.Infrastructure.Commands.BaseCommand;
using InventoryOfDevices.Models;
using Microsoft.Office.Interop.Word;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Application = System.Windows.Application;
using Task = System.Threading.Tasks.Task;

namespace InventoryOfDevices.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public enum DeviceSearchCriteria
        {
            DeviceId,
            DeviceName,
            CategoryName
        }

        #region Поля

        private ObservableCollection<Device> _cachedDevices;
        private ObservableCollection<Device> _devices;
        private bool _sendJsonSuccess = false;
        private Device? _selectedDevice;
        private bool _isQuantityChecked;
        private bool _isCategoryChecked;
        private bool _isLocationChecked;
        //для управления потоками выполнения и вызовами методов
        private Dispatcher _dispatcher = Application.Current.Dispatcher;

        private string _devicesFilterText;
        private DeviceSearchCriteria _selectedSearchCriteria;
        private bool _isDeviceIdSelected;
        private bool _isDeviceNameSelected;
        #endregion

        #region Конструктор
        public MainViewModel(ObservableCollection<Device> diviceList)
        {
            _cachedDevices = diviceList;
            Devices = _cachedDevices;

            AddDeviceCommand = new LambdaCommand(OnAddDeviceExecuted, (object p) => true);//Добавление
            EditDeviceCommand = new LambdaCommand(OnEditDeviceExecuted, (object p) => true);//Редактирование

            DeleteDeviceCommand = new LambdaCommand(OnDeleteDeviceExecuted, CanDeleteDeviceExecute);//Удаление
            GenerateReportDeviceCommand = new LambdaCommand(OnGenerateReportDeviceExecuted, CanGenerateReportDeviceExecute);//Формирование отчета

            SelectedSearchCriteria = DeviceSearchCriteria.DeviceId;

            SearchCommand = new LambdaCommand(Search, CanSearchCommandExecute);//Поиск
            ResetSelectionCommand = new LambdaCommand(ResetSelection, (object p) => true);//Сброс выбора
            SearchCommand.CanExecuteChanged += (s, e) => OnPropertyChanged(nameof(SearchCommand));
            OnPropertyChanged(nameof(CanSearchCommandExecute));

            ExitCommand = new LambdaCommand(OnExitExecuted, (object p) => true);//Закрытие окна инвентаризации

            SendJsonCommand = new AsyncCommand(OnSendJsonExecutedAsync);

            MainWindow current = new MainWindow();
            current.DataContext = this;
            current.Show();
        }
        #endregion

        #region Команды
        public ICommand AddDeviceCommand { get; }
        public ICommand ExitCommand { get; }
        public ICommand SendJsonCommand { get; }
        public ICommand EditDeviceCommand { get; }
        public ICommand DeleteDeviceCommand { get; }
        public ICommand GenerateReportDeviceCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand ResetSelectionCommand { get; }
        #endregion

        #region Свойства

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

        public bool SendJsonSuccess
        {
            get => _sendJsonSuccess;
            set => Set(ref _sendJsonSuccess, value);
        }

        #region Выбор элемента
        public Device SelectedDevice
        {
            get => _selectedDevice;
            set => Set(ref _selectedDevice, value);
        }
        #endregion

        #region Радиобаттоны для отчета
        public bool IsQuantityChecked
        {
            get { return _isQuantityChecked; }
            set
            {
                _isQuantityChecked = value;
                OnPropertyChanged(nameof(IsQuantityChecked));
                OnPropertyChanged(nameof(IsCategoryChecked));
                OnPropertyChanged(nameof(IsLocationChecked));
            }
        }

        public bool IsCategoryChecked
        {
            get { return _isCategoryChecked; }
            set
            {
                _isCategoryChecked = value;
                OnPropertyChanged(nameof(IsQuantityChecked));
                OnPropertyChanged(nameof(IsCategoryChecked));
                OnPropertyChanged(nameof(IsLocationChecked));
            }
        }

        public bool IsLocationChecked
        {
            get { return _isLocationChecked; }
            set
            {
                _isLocationChecked = value;
                OnPropertyChanged(nameof(IsQuantityChecked));
                OnPropertyChanged(nameof(IsCategoryChecked));
                OnPropertyChanged(nameof(IsLocationChecked));
            }
        }
        #endregion

        public string SelectedCategory { get; set; }
        public string SelectedLocation { get; set; }

        #region Для поиска
        public string DevicesFilterText
        {
            get => _devicesFilterText;
            set
            {
                if (!Set(ref _devicesFilterText, value))
                    return;
                SearchCommand.Execute(null);
            }
        }
        public DeviceSearchCriteria SelectedSearchCriteria
        {
            get => _selectedSearchCriteria;
            set => Set(ref _selectedSearchCriteria, value);
        }

        public bool IsDeviceIdSelected
        {
            get => _isDeviceIdSelected;
            set
            {
                if (!Set(ref _isDeviceIdSelected, value))
                    return;
                SelectedSearchCriteria = value ? DeviceSearchCriteria.DeviceId : SelectedSearchCriteria;
                SearchCommand.Execute(null);
            }
        }
        public bool IsDeviceNameSelected
        {
            get => _isDeviceNameSelected;
            set
            {
                if (!Set(ref _isDeviceNameSelected, value))
                    return;
                SelectedSearchCriteria = value ? DeviceSearchCriteria.DeviceName : SelectedSearchCriteria;
                SearchCommand.Execute(null);
            }
        }

        private bool _isCategoryNameSelected;
        public bool IsCategoryNameSelected
        {
            get => _isCategoryNameSelected;
            set
            {
                if (!Set(ref _isCategoryNameSelected, value))
                    return;
                SelectedSearchCriteria = value ? DeviceSearchCriteria.CategoryName : SelectedSearchCriteria;
                SearchCommand.Execute(null);
            }
        }
        #endregion

        #endregion

        #region Добавление
        private void OnAddDeviceExecuted(object p)
        {
            var addDeviceWindow = new AddViewModel(_cachedDevices);
        }

        #endregion

        #region Закрытие окна инвентаризации
        private void OnExitExecuted(object p)
        {
            MessageBoxResult result = MessageBox.Show("Вы действительно хотите выйти?", "Уведомление", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes)
            {
                // Вызов команды отправки файла
                SendJsonCommand.Execute(null);
                Application.Current.Shutdown();
            }
            
        }
        #endregion

        #region Отправка данных на сервер - завершение инвентаризации

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

        #region Команда для открытия окна редактирования
        
        public void OnEditDeviceExecuted(object p)
        {
            if (SelectedDevice != null)
            {
                var editViewModel = new EditViewModel(SelectedDevice);
            }
        }

        #endregion

        #region Команда удаления данных
        public bool CanDeleteDeviceExecute(object p) => p is Device device && Devices.Contains(device);
        private void OnDeleteDeviceExecuted(object p)
        {
            Devices.Remove(SelectedDevice);

            SelectedDevice = Devices.FirstOrDefault(); // Установка первого элемента в качестве выбранного, если удаленный был выбранным

            //отправка пост запроса на сервер для удаления
            SendJsonCommand.Execute(null);
        }
        #endregion

        #region Команда формирование отчета
        
        public bool CanGenerateReportDeviceExecute(object p)
        {
            return IsQuantityChecked || IsCategoryChecked || IsLocationChecked;
        }
        
        private async void OnGenerateReportDeviceExecuted(object p)
        {
            // Получение выбранного радиобаттона
            bool isQuantityChecked = IsQuantityChecked;
            bool isCategoryChecked = IsCategoryChecked;
            bool isLocationChecked = IsLocationChecked;

            // Получение списка устройств
            List<Device> deviceList = Devices.ToList();

            // Сортировка и фильтрация устройств
            deviceList = SortAndFilterDevices(deviceList, isQuantityChecked, isCategoryChecked, isLocationChecked);

            await Task.Run(() =>
            {
                // Код, который нужно выполнить в фоновом потоке
                _dispatcher.Invoke(() =>
                {
                    // Код, который нужно выполнить в основном потоке
                    MessageBox.Show("Отчет формируется");
                });

                // Открытие диалогового окна для выбора места сохранения файла
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Word Document|*.docx",
                    DefaultExt = "docx",
                    FileName = "Отчет по устройствам"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    // Создание таблицы с заголовком
                    Microsoft.Office.Interop.Word.Application wordApp = new Microsoft.Office.Interop.Word.Application();
                    Microsoft.Office.Interop.Word.Document doc = wordApp.Documents.Add();

                    // Установка альбомной ориентации для документа
                    doc.PageSetup.Orientation = WdOrientation.wdOrientLandscape;

                    // Получение списка свойств Device для создания таблицы
                    var properties = typeof(Device).GetProperties();

                    // Создание таблицы с количеством столбцов равным количеству свойств Device + 1 (для штрих-кода)
                    Table table = doc.Tables.Add(doc.Range(), deviceList.Count, properties.Length);

                    // Установка выравнивания заголовков по центру
                    table.Rows[1].Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;

                    // Установка минимального левого отступа для таблицы
                    table.Range.ParagraphFormat.LeftIndent = 0;


                    // Заполнение заголовков таблицы
                    FillTableHeaders(table, new string[] {"Инвентарный номер", "Название", "Количество", "Местоположение", "Штрих-код", "Фамилия ответственного", "Имя ответственного", "Описание", "Категория", "Производитель"});

                    // Заполнение данных в таблицу
                    FillTableData(table, deviceList, properties);

                    try
                    {
                        // Сохранение отчета в Word
                        doc.SaveAs(saveFileDialog.FileName);
                        doc.Close();
                        wordApp.Quit();

                        _dispatcher.Invoke(() =>
                        {
                            // Код, который нужно выполнить в основном потоке
                            MessageBox.Show("Документ сохранен");

                        });
                    }
                    catch (Exception ex)
                    {
                        // Обработка ошибок сохранения
                        _dispatcher.Invoke(() =>
                        {
                            // Код, который нужно выполнить в основном потоке
                            MessageBox.Show(ex.Message);
                        });
                    }
                }
            });

        }

        private List<Device> SortAndFilterDevices(List<Device> devices, bool isQuantityChecked, bool isCategoryChecked, bool isLocationChecked)
        {
            if (isQuantityChecked)
            {
                devices = devices.Where(d => d.Quantity > 0).OrderBy(d => d.Quantity).ToList();
            }
            else if (isCategoryChecked)
            {
                devices = devices.OrderBy(d => d.CategoryName).ToList();
            }
            else if (isLocationChecked)
            {
                devices = devices.OrderBy(d => d.Location).ToList();
            }

            return devices;
        }

        private void FillTableHeaders(Table table, string[] headers)
        {
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = table.Cell(1, i + 1);
                cell.Range.Text = headers[i];
                cell.Range.Font.Bold = 1;
                cell.Range.Font.Size = 9;
                cell.Range.ParagraphFormat.LeftIndent = 0;
                cell.Borders[WdBorderType.wdBorderTop].LineStyle = WdLineStyle.wdLineStyleSingle;
                cell.Borders[WdBorderType.wdBorderBottom].LineStyle = WdLineStyle.wdLineStyleSingle;
                cell.Borders[WdBorderType.wdBorderLeft].LineStyle = WdLineStyle.wdLineStyleSingle;
                cell.Borders[WdBorderType.wdBorderRight].LineStyle = WdLineStyle.wdLineStyleSingle;
                cell.Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
            }
        }

        private void FillTableData(Table table, List<Device> deviceList, PropertyInfo[] properties)
        {
            int row = 2;
            foreach (var device in deviceList)
            {
                int col = 1;
                foreach (var property in properties)
                {
                    var cell = table.Cell(row, col);
                    cell.Range.Text = property.GetValue(device)?.ToString();
                    cell.Range.Font.Size = 8;
                    cell.Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphLeft;
                    cell.Borders[WdBorderType.wdBorderLeft].LineStyle = WdLineStyle.wdLineStyleSingle;
                    cell.Borders[WdBorderType.wdBorderRight].LineStyle = WdLineStyle.wdLineStyleSingle;
                    cell.Borders[WdBorderType.wdBorderTop].LineStyle = WdLineStyle.wdLineStyleSingle;
                    cell.Borders[WdBorderType.wdBorderBottom].LineStyle = WdLineStyle.wdLineStyleSingle;
                    col++;
                }

                row++;
            }
        }

        private void InsertBarcodeImage(Table table, int row, int col, string barcodeValue)
        {
            //var cell = table.Cell(row, col);

            //// Добавление пустой ячейки рядом со столбцом штрих-кода
            //var emptyCell = table.Cell(row, col + 1);
            //emptyCell.Range.Text = ""; // Пустой текст 

            //// Преобразование значения штрих-кода в числовой тип
            //if (long.TryParse(barcodeValue, out long barcodeNumber))
            //{
            //    // Генерация изображения штрих-кода и вставка в ячейку
            //    using (var barcodeImage = ZXing.BarcodeWriter.CreateBarcode(barcodeNumber.ToString()))
            //    {
            //        // Преобразование изображения в MemoryStream - временное хранение данных в памяти
            //        using (var memoryStream = new MemoryStream())
            //        {
            //            barcodeImage.Save(memoryStream);
            //            memoryStream.Position = 0;

            //            // Вставка изображения штрих-кода в ячейку
            //            var picture = emptyCell.Range.InlineShapes.AddPicture(memoryStream);
            //            picture.Width = 100; // Установка ширины изображения
            //        }
            //    }
            //}
            //else
            //{
            //    emptyCell.Range.Text = "Ошибка преобразования";
            //}
        }

        #endregion

        #region Поиск

        public bool CanSearchCommandExecute(object p) => true;

        private void Search(object parameter)
        {
            if (string.IsNullOrWhiteSpace(DevicesFilterText))
            {
                return;
            }
            string searchText = DevicesFilterText.ToLower(); // Преобразование текста поиска в нижний регистр

            switch (SelectedSearchCriteria)
            {
                case DeviceSearchCriteria.DeviceId:
                    Devices = new ObservableCollection<Device>(_cachedDevices.Where(d => d.DeviceId.ToString().ToLower().Contains(searchText)));
                    break;
                case DeviceSearchCriteria.DeviceName:
                    Devices = new ObservableCollection<Device>(_cachedDevices.Where(d => d.DeviceName.ToLower().Contains(searchText)));
                    break;
                case DeviceSearchCriteria.CategoryName:
                    Devices = new ObservableCollection<Device>(_cachedDevices.Where(d => d.CategoryName.ToLower().Contains(searchText)));
                    break;
            }
        }


        #region Сброс

        public bool CanResetSelectionCommandExecute(object p) => true;

        private void ResetSelection(object parameter)
        {
            IsDeviceIdSelected = false;
            IsDeviceNameSelected = false;
            IsCategoryNameSelected = false;
            SelectedSearchCriteria = DeviceSearchCriteria.DeviceId; // Установка значения по умолчанию
            DevicesFilterText = ""; // Сброс текста поиска
            Devices = new ObservableCollection<Device>(_cachedDevices);
        }

        #endregion

        #endregion

    }
}
