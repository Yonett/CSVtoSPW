using CSVtoSPW.Core.Models;
using CSVtoSPW.KOMPASIntegration;

namespace CSVtoSPW.Data.Exporters
{
    public class SPWExporter
    {
        private readonly KOMPASFacade _kompas;

        public SPWExporter(KOMPASFacade kompas)
        {
            _kompas = kompas;
        }

        public SPWExporter() : this(new KOMPASFacade()) { }


        public void Export(List<SpecItem> items, string outputPath, string templatePath, int maxLineLength)
        {
            try
            {
                _kompas.Connect();
                _kompas.CreateSpecification(templatePath);

                foreach (var item in items)
                {
                    item.FormatPosition();
                    item.AddLineBreaks(maxLineLength);
                    _kompas.AddPosition(item);
                }

                _kompas.Save(outputPath);
                _kompas.Close();
            }
            catch (Exception ex)
            {
                throw new SpcExportException("Ошибка экспорта", ex);
            }
        }
    }

    public class SpcExportException : Exception
    {
        public SpcExportException(string message, Exception inner) 
            : base(message, inner) { }
    }
}