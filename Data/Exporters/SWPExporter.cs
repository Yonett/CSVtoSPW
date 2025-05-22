using CSVtoSPW.Core.Models;
using CSVtoSPW.Core.Interfaces;
using CSVtoSPW.KOMPASIntegration;
using System;
using System.Collections.Generic;

namespace CSVtoSPW.Data.Exporters
{
    public class SPWExporter
    {
        private readonly IKompasService _kompasService;

        public SPWExporter(IKompasService kompasService)
        {
            _kompasService = kompasService;
        }

        public SPWExporter() : this(new KOMPASFacade()) { }


        public void Export(List<SpecItem> items, string outputPath, string templatePath)
        {
            try
            {
                _kompasService.Connect();
                _kompasService.CreateSpecification(templatePath);

                foreach (var item in items)
                {
                    _kompasService.AddPosition(item);
                }

                _kompasService.Save(outputPath);
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