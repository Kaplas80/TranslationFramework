using TF.Core.Entities;

namespace TF.Core.Projects.ToCS.Files
{
    public static class FileFactory
    {
        public static ITFFile GetFile(string fileName)
        {
            if (fileName.EndsWith(".tbl"))
            {
                return new TblFile(fileName);
            }

            return null;
        }

        public static ITFFile GetFile(string fileName, byte[] fileContent)
        {
            if (fileName.EndsWith(".tbl"))
            {
                return new TblFile(fileName);
            }

            return null;
        }
    }
}
