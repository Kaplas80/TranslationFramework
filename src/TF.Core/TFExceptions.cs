using System;

namespace TF.Core
{
    public class TFUnknownFileTypeException : Exception
    {
        public TFUnknownFileTypeException(string message) : base(message)
        {
        }
    }

    public class TFChangedFileException : Exception
    {
        public TFChangedFileException(string message) : base(message)
        {
        }
    }
}
