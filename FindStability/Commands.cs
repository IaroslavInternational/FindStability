using System;
using System.Text.RegularExpressions;

namespace StandApp
{
    // Команды
    public static class Commands
    {
        // Разделитель данных
        public const char SPLITTER = '|';

        // Команды от
        // АЦП с тензодатчика
        public struct HX711
        {
            public const string realPressure =   "[HX 711 pressure]";  // Давление на тензодатчике
            public const string momentPressure = "[HX 711 moment]";    // Момент (тензодатчик)
        }

        // Команды для Ардуино
        public struct Arduino
        {
            public const string shutdown =       "[Reset]";             // Спящий режим
            public const string testConnection = "[Test connection]";   // Проверка соединения
        }

        // Полученные команды
        public struct Response
        {
            public const string goodConnection = "[Connection succeded]";   // Успешное подключение
        }

        // Метод для удаления специальных символов
        public static string DeleteSpecSymb(string str)
        {
            return Regex.Replace(str, @"[\u0000-\u001F]", string.Empty);
        }

        // Метод для переноса в новый диапазон значений
        public static int Map(int x, int in_min, int in_max, int out_min, int out_max)
        {
            return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
        }

        public static string ProcessValue(string val, double koef)
        {
            return Convert.ToString((double.Parse(val, System.Globalization.CultureInfo.InvariantCulture) * koef));
        }    
        
        public static string ProcessTenzoValue(string val, double koef)
        {
            return Convert.ToString((int)(double.Parse(val, System.Globalization.CultureInfo.InvariantCulture) * koef));
        }
    }
}
