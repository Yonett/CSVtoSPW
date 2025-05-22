using CSVtoSPW.Core.Models;
using CSVtoSPW.Core.Services;
using CSVtoSPW.Data.Exporters;
using CSVtoSPW.KOMPASIntegration;

namespace CSVtoSPW.ConsoleApp
{
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                var config = LoadConfig(args);

                //KOMPASFacade kompas = new KOMPASFacade();

                List<SpecItem> items = SpecParser.Parse(config.InputCsvPath, config.CsvDelimiter);

                using (var exporter = new SPWExporter())
                {
                    exporter.Export(
                        items: items,
                        outputPath: config.OutputSpwPath,
                        templatePath: config.TemplateLytPath,
                        maxLineLength: config.LineControlSize
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
            string configPath = args.Length > 0 ? args[0] : "config.json";
            return AppConfig.Load(configPath);
        }
    }
}