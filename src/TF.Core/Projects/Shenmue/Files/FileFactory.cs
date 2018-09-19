
using TF.Core.Entities;

namespace TF.Core.Projects.Shenmue.Files
{
    public static class FileFactory
    {
        public static ITFFile GetFile(string fileName)
        {
            if (fileName.EndsWith(".sub"))
            {
                return new SubFile(fileName);
            }

            return null;
        }

        
    }
}
