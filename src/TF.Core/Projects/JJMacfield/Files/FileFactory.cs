
using TF.Core.Entities;

namespace TF.Core.Projects.JJMacfield.Files
{
    public static class FileFactory
    {
        public static ITFFile GetFile(string fileName)
        {
            if (fileName.EndsWith(".txt"))
            {
                return new TxtFile(fileName);
            }

            return null;
        }

        public static ITFFile GetFile(string fileName, byte[] fileContent)
        {
            if (fileName.EndsWith(".txt"))
            {
                return new TxtFile(fileName);
            }

            return null;
        }
    }
}
