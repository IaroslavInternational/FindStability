using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using Newtonsoft.Json;
using System;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Windows.Forms;

namespace StandApp
{
    public partial class ChartForm : Form
    {
        /* Делегаты */

        // Установка текста для давления на тензодатчик в другом потоке
        private delegate void TenzoSetter(string val);
        private TenzoSetter SetTenzo;   

        // Добавление значения к главному графику в другом потоке
        private delegate void MainChartSetter(double val);
        private MainChartSetter AddVal;      
        
        // Добавление значения ко 2му графику в другом потоке
        private delegate void ChartSetter(double val);
        private ChartSetter AddVal2;

        /************/

        /* Настройки */
  
        private const string kpa =      "кПа";           // Постфикс для давления в                        килопаскалях
        private const string kgramm =   "кг";            // Постфикс для давления на тензодатчик в         килограммах
        private const string gramm =    "гр.";           // Постфикс для давления на тензодатчик в         граммах

        private int AllowedPoints = 100;                 // Кол-во одновременно отрисованных точек на графике

        private bool IsExit = false;                     // Если программа закрывается

        private static string logPath = "log.txt";       // Имя файла с логом 
        private FileStream logFileStream;                // Поток работы с файлом

        private uint exp_num = 1;                        // Номер эксперимента
        private uint dLog_num = 1;                       // Номер динамичного лога

        private bool DynamicLog = false;                 // Если динамичный лог доступен
        private uint DynamicLogString = 0;               // Текущая строка динамичного лога

        /* Настроечные данные */

        private string sPortName;
        private int    sBaudRate;
        private int    sTimeout;
        private double sTenzoCalFactor1;
        private double sTenzoCalFactor2;      
        private double sCurrentCalFactor1;
        private double sCurrentCalFactor2;
        private double sVoltageCalFactor;
        private string sLogSplitter;
        private int    sWorkInterval_low;
        private int    sWorkInterval_high;
        private bool   sIsChartAnimationActive;
        private int    sWeightChartMax;
        private double sShoulder;
        private string sProp;
        private string sEngine;
        private string sEsc;

        /**********************/

        /*************/

        // Конструктор
        public ChartForm()
        {
            InitializeComponent();

            /* Объявление делегат */

            SetTenzo =     new TenzoSetter(SetNewTenzoPressure);
            AddVal =       new MainChartSetter(AddNewValueToMainChart);
            AddVal2 =      new ChartSetter(AddNewValueTo2MainChart);

            /**********************/

            /* Настройки главного графика */

            mainChart.AxisY.Add(
            new Axis
            {
                MinValue = 0,
                MaxValue = 300
            });

            mainChart.AxisX.Add(
            new Axis
            {
                MinValue = 0,
                MaxValue = 300
            });

            var SeriesCollection = new SeriesCollection
            {
                new ScatterSeries
                {
                    Title = "Tenzo",
                    Values = new ChartValues<ObservablePoint>
                    {
                        new ObservablePoint(0, 0),
                        new ObservablePoint(0, 300),
                        new ObservablePoint(300, 300),
                        new ObservablePoint(300, 0)
                    },
                    ScalesYAt = 0,
                    StrokeThickness = 1,
                    PointGeometry = DefaultGeometries.Diamond
                }
            };

            var SeriesCollection2 = new SeriesCollection
            {
                new ScatterSeries
                {
                    Title = "Mass center",
                    Values = new ChartValues<ObservablePoint> { },
                    ScalesYAt = 0,
                    StrokeThickness = 1,
                    PointGeometry = DefaultGeometries.Circle
                }
            };

            var r = new Random();

            foreach (var series in SeriesCollection2)
            {
                for (var i = 0; i < 20; i++)
                {
                    series.Values.Add(new ObservablePoint(r.Next(100, 200), r.Next(100, 200)));
                }

                for (var i = 0; i < 10; i++)
                {
                    series.Values.Add(new ObservablePoint(r.Next(50, 250), r.Next(50, 250)));
                }
            }

            // Объявления графика
            mainChart.Series = SeriesCollection;
            mainChart.Series.Add(SeriesCollection2[0]);

            /******************************/

            // Создание события приёма данных
            serialPortMain.DataReceived += SerialPortMain_DataReceived;
        }

        private void SetNewTenzoPressure(string val)
        {
            throw new NotImplementedException();
        }

        // При загрузке формы
        private void ChartForm_Load(object sender, EventArgs e)
        {
          /*  string rawData = "";
            bool IsFileExisting = false;

            try
            {
                // чтение из файла
                using (FileStream fstream = File.OpenRead("connection settings.json"))
                {
                    byte[] array = new byte[fstream.Length];

                    fstream.Read(array, 0, array.Length);
                    rawData = System.Text.Encoding.Default.GetString(array);

                    IsFileExisting = true;
                }
            }
            catch (FileNotFoundException ex)
            {
                IsFileExisting = false;

                Errors.ShowMessage(Errors.checkSettings);
            }

            if (IsFileExisting)
            {
                ConnectionData data = JsonConvert.DeserializeObject<ConnectionData>(rawData);

                sPortName = data.PortName;
                sBaudRate = data.BaudRate;
                sTimeout = data.Timeout;
                sTenzoCalFactor1 = data.TenzoCalFactor1;
                sTenzoCalFactor2 = data.TenzoCalFactor2;
                sCurrentCalFactor1 = data.CurrentCalFactor1;
                sCurrentCalFactor2 = data.CurrentCalFactor2;
                sVoltageCalFactor = data.VoltageCalFactor;
                sLogSplitter = data.LogSplitter;
                sWorkInterval_low = data.WorkInterval_low;
                sWorkInterval_high = data.WorkInterval_high;
                sIsChartAnimationActive = data.IsChartAnimationActive;
                sWeightChartMax = data.WeightChartMax;
                sShoulder = data.Shoulder;
                sProp = data.prop;
                sEngine = data.engine;
                sEsc = data.esc;
                
                // Установка заддержки отправки данных
                serialPortMain.WriteTimeout = sTimeout;

                mainChart.DisableAnimations = !sIsChartAnimationActive;

                try
                {
                    serialPortMain.PortName = sPortName;
                    serialPortMain.BaudRate = sBaudRate;

                    if (!serialPortMain.IsOpen)
                    {
                        serialPortMain.Open();
                        serialPortMain.WriteLine("[Work mode]");
                    }
                    else
                    {
                        Errors.ShowMessage(Errors.portIsBusy);
                    }
                }
                catch (IOException ex)
                {
                    Errors.ShowMessage(Errors.deletedPort);
                }
            }*/
        }

        // Добавить лог
        private void AddLog(string message)
        {            
            byte[] array = System.Text.Encoding.Default.GetBytes(message);
            logFileStream.Write(array, 0, array.Length);
        }

        // Отключить отображение всех графиков и очистить график
        private void DisableAllCharts()
        {
            mainChart.Series[1].Values.Clear();
        }

        // Установить единицы измерения для главного графика
        private void SetDataPostfix(string pf)
        {
            mainChart.Series[0].LabelPoint = point => point.Y + " " + pf;
        }

        // Добавить новое значение на график 
        private void AddNewValueToMainChart(double val)
        {
            if (mainChart.Series[1].Values.Count == AllowedPoints)
            {
                mainChart.Series[1].Values.RemoveAt(0);
            }

            mainChart.Series[1].Values.Add(val);
        }
        
        // Добавить новое значение на 2й график 
        private void AddNewValueTo2MainChart(double val)
        {
            if (mainChart.Series[1].Values.Count == AllowedPoints)
            {
                mainChart.Series[1].Values.RemoveAt(0);
            }

            mainChart.Series[1].Values.Add(val);
        }

        // Установить номер строки в логе
        private void SetStringNumber()
        {
            DynamicLogString++;
            AddLog(DynamicLogString.ToString() + ". ");
        }

        // При получении данных с МК
        private void SerialPortMain_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (!serialPortMain.IsOpen && !IsExit)
                return;

            try
            {
                var data = serialPortMain.ReadLine();   // Данные
                data = Commands.DeleteSpecSymb(data);   // Очистка от спец. символов

                // 0 - Команда | 1 - значение
                string[] arr_data = data.Split(Commands.SPLITTER);

                // Если не информация
                if (arr_data.Length != 1)
                {
                    string command = arr_data[0];   // Команда
                    string value = arr_data[1];     // Значение

                    if (command == Commands.HX711.realPressure) // Если тензодатчик
                    {
                        if (DynamicLog)
                        {

                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        // При закрытии формы
        private void ChartForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            IsExit = true;
            DynamicLog = false;

            // Если порт открыт
            if (serialPortMain.IsOpen)
            {
                serialPortMain.WriteLine(Commands.Arduino.shutdown);    // Перевести систему в спящий режим
                serialPortMain.Close();                                 // Отключить порт
            }
        }

        // Включить динамичный лог
        private void logAcceptBtn_Click(object sender, EventArgs e)
        {
            if (DynamicLog)
            {              
                DynamicLog = false;
                DynamicLogString = 0;

                DateTime dateTime = DateTime.Now;

                AddLog("Конец:\n");
                AddLog(dateTime.ToString("dd.MM.yyyy") + "\n" + dateTime.ToString("HH:mm:ss") + "\n");

                AddLog("Динамичный лог " + (dLog_num - 1) + " окончен.\n");

                logFileStream.Close();
            }
            else
            {
                logFileStream = new FileStream(logPath, FileMode.Append);

                DynamicLog = true;

                AddLog("Динамичный лог " + dLog_num + "\n");

                DateTime dateTime = DateTime.Now;

                AddLog("Начало:\n");
                AddLog(dateTime.ToString("dd.MM.yyyy") + "\n" + dateTime.ToString("HH:mm:ss") + "\n");

                AddLog("Двигатель: " + sEngine + "\n");
                AddLog("Регулятор: " + sEsc + "\n");
                AddLog("Пропеллер: " + sProp + "\n");

                dLog_num++;
            }
        }

        // При нажатии на кнопку для включения анимации графика
        private void включитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mainChart.DisableAnimations = false;
        }

        // При нажатии на кнопку для выключения анимации графика
        private void выключитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mainChart.DisableAnimations = true;
        }

        /*************************/
    }
}
