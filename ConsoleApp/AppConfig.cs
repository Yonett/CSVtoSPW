using System.IO;
using System.Text.Json;

namespace CSVtoSPW.ConsoleApp
{
    public class AppConfig
    {
        // Пути к файлам
        public string InputCsvPath { get; set; }
        public string OutputSpwPath { get; set; }
        public string TemplateLytPath { get; set; }

        // Настройки формата
        public char CsvDelimiter { get; set; } = ';';
        public int LineControlSize { get; set; } = 8;

        // Группировка
        public Dictionary<string, (string Name, string Comment)> GroupMappings { get; set; }

        /// <summary> Загружает конфиг из JSON-файла </summary>
        public static AppConfig Load(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Конфигурационный файл не найден", filePath);

            var json = File.ReadAllText(filePath);
            var config = JsonSerializer.Deserialize<AppConfig>(json);

            Validate(config);
            return config;
        }

        private static void Validate(AppConfig config)
        {
            if (string.IsNullOrEmpty(config.InputCsvPath))
                throw new InvalidDataException("Не указан InputCsvPath");

            if (!File.Exists(config.TemplateLytPath))
                throw new FileNotFoundException("Шаблон спецификации не найден", config.TemplateLytPath);
        }
    }
}