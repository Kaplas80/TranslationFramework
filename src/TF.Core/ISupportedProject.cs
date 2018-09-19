namespace TF.Core
{
    public interface ISupportedProject
    {
        string SaveProjectFilter { get; }

        string OpenProjectFilter { get; }
    }
}
