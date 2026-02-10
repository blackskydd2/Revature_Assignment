using System;
using System.IO;

namespace GarbageCollectionDemo
{
    public class FileManager : IDisposable
    {
        private FileStream? _fileStream;
        private bool _disposed = false;

        public void OpenFile(string path)
        {
            _fileStream = new FileStream(path, FileMode.Open);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                _fileStream?.Dispose();
            }
            _disposed = true;
        }

        ~FileManager()
        {
            Dispose(false);
        }
    }
}
