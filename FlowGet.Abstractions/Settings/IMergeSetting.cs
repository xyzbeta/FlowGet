namespace FlowGet.Abstractions.Settings
{
    public interface IMergeSetting
    {
        string SelectedFormat { get; }
        bool ForcedMerger { get; }
        bool IsCleanUp { get; }
    }
}
