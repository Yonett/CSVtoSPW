using CSVtoSPW.Core.Models;
using CSVtoSPW.Core.Services;
using Kompas6API5;
using Kompas6Constants;
using KAPITypes;
using System.Runtime.InteropServices;

namespace CSVtoSPW.KOMPASIntegration
{
    public class KOMPASFacade : IDisposable
    {
        private KompasObject? _kompas;
        private bool _isDisposed;

        private ksSpcDocument _spcDocument;
        private ksSpecification _specification;

        public void Connect()
        {
            if (_kompas != null) return;

            try
            {
                _kompas = (KompasObject)Marshal2.GetActiveObject("KOMPAS.Application.5");
            }
            catch (COMException)
            {
                _kompas = (KompasObject)Activator.CreateInstance(Type.GetTypeFromProgID("KOMPAS.Application.5"));
            }
        }

        public void CreateSpecification(string layoutPath)
        {
            CheckConnection();
            ValidateLayoutFile(layoutPath);

            var docParam = (ksDocumentParam)_kompas.GetParamStruct(
                (short)StructType2DEnum.ko_DocumentParam);
            
            docParam.Init();
            docParam.type = (int)DocType.lt_DocSpc;

            var sheetParam = (ksSheetPar)docParam.GetLayoutParam();
            sheetParam.Init();
            sheetParam.layoutName = layoutPath;
            sheetParam.shtType = 1;

            _spcDocument = (ksSpcDocument)_kompas.SpcDocument();
            _spcDocument.ksCreateDocument(docParam);

            _specification = (ksSpecification)_spcDocument.GetSpecification();
        }

        public void AddPosition(SpecItem item)
        {
            CheckConnection();
            CheckActiveSpecification();

            var objParam = (ksSpcObjParam)_kompas.GetParamStruct((short)StructType2DEnum.ko_SpcObjParam);
            
            objParam.Init();
            objParam.blockNumber = 0;
            objParam.draw = 1;

            _specification.ksSpcObjectCreate("", 0, 1, 0, 0, 2);
            int rowId = _specification.ksSpcObjectEnd();

            _specification.ksSpcObjectEdit(rowId);
            _spcDocument.ksSetObjParam(rowId, objParam, ldefin2d.ALLPARAM);
            
            _specification.ksSetSpcObjectColumnText(4, 1, 0, item.FormattedPosition);
            _specification.ksSetSpcObjectColumnText(5, 1, 0, item.Name);
            _specification.ksSetSpcObjectColumnText(6, 1, 0, item.Count);
            _specification.ksSetSpcObjectColumnText(7, 1, 0, item.Commentary);
            
            _specification.ksSpcObjectEnd();
        }

        public void Save(string filePath)
        {
            CheckActiveSpecification();
            _spcDocument.ksSaveDocument(filePath);
        }

        private void CheckConnection()
        {
            if (_kompas == null)
                throw new InvalidOperationException("Сначала выполните Connect()");
        }

        private void CheckActiveSpecification()
        {
            if (_specification == null)
                throw new InvalidOperationException("Спецификация не создана");
        }

        private void ValidateLayoutFile(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"Файл стиля не найден: {path}");
        }

        public void Close()
        {
            _spcDocument?.ksCloseDocument();
            if (_kompas != null)
            {
                _kompas.Quit();
                _kompas = null;
            }
        }

        public void Dispose()
        {
            if (_isDisposed) return;

            try
            {
                _spcDocument?.ksCloseDocument();
                if (_kompas != null)
                {
                    _kompas.Quit();
                    _kompas = null;
                }
                if (_kompas != null)
                {
                    Marshal.ReleaseComObject(_kompas);
                    _kompas = null;
                }
            }
            finally
            {
                _isDisposed = true;
                GC.SuppressFinalize(this);
            }
        }
    }
}