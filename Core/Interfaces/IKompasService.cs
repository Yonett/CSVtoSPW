using CSVtoSPW.Core.Models;
using System;

namespace CSVtoSPW.Core.Interfaces
{

    public interface IKompasService : IDisposable
    {
        public void CreateNewSpecification(string templatePath = null);
        public void AddPosition(SpecItem item);
        public void SaveDocument(string filePath);
        
        public bool IsConnected { get; }
        public string ActiveDocumentName { get; }
        
        public event Action<string> OperationCompleted;
        public event Action<Exception> ErrorOccurred;
    }
}