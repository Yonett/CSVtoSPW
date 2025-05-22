using CSVtoSPW.Core.Models;
using CSVtoSPW.Data.Parsers;
using CSVtoSPW.KOMPASIntegration;
using System;
using System.IO;

namespace CSVtoSPW.ConsoleApp
{
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                // 1. Загрузка конфигурации
                var config = LoadConfig(args);
                
                // 2. Парсинг CSV
                var parser = new SpecParser(config.CsvDelimiter);
                List<SpecItem> items = parser.Parse(config.InputCsvPath);

                // 3. Экспорт в KOMPAS
                using (var exporter = new SpcExporter())
                {
                    exporter.Export(
                        items: items,
                        outputPath: config.OutputSpwPath,
                        templatePath: config.TemplateLytPath,
                        lineControlSize: config.LineControlSize,
                        groupMappings: config.GroupMappings
                    );
                }

                Console.WriteLine($"Спецификация успешно создана: {config.OutputSpwPath}");
                return 0;
            }
            catch (FileNotFoundException ex)
            {
                Console.Error.WriteLine($"Ошибка: {ex.Message}");
                return 1;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Критическая ошибка: {ex.Message}");
                Console.Error.WriteLine(ex.StackTrace);
                return 2;
            }
        }

        private static AppConfig LoadConfig(string[] args)
        {
            // Приоритет: аргументы командной строки > config.json
            string configPath = args.Length > 0 ? args[0] : "config.json";
            return AppConfig.Load(configPath);
        }
    }
}