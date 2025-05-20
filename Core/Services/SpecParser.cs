using System.IO;
using CSVtoSPW.Core.Models;

namespace CSVtoSPW.Data.Parsers
{
    public static class SpecParser
    {
        public static List<SpecItem> Parse(string filePath, string delimeter)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Файл не найден");
                
            var items = new List<SpecItem>();
            
            foreach (string line in File.ReadLines(filePath))
            {
                if (string.IsNullOrWhiteSpace(line)) 
                    continue;

                string[] columns = line.Split(delimeter);
                
                if (columns.Length < 4)
                    throw new FormatException($"Некорректная строка: {line}");

                items.Add(new SpecItem(
                      position: columns[0].Trim(),
                          name: columns[1].Trim(),
                         count: columns[2].Trim(),
                    commentary: columns[3].Trim()
                ));
            }
            
            return items;
        }
    }
}