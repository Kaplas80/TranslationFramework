namespace TF.Core
{
    public interface ISupportedProject
    {
        string SaveProjectFilter { get; }

        bool ValidateInstallPath(string path);
    }
}
