namespace Yootek
{
    public interface IAppFolders
    {
        string TempFileDownloadFolder { get; }

        string SampleProfileImagesFolder { get; }

        string WebLogsFolder { get; set; }
        string TempUploadFolder { get; set; }
    }
}