using System.IO;

namespace EBrief.Helpers;
public static class FileHelpers
{
    public static string AppDataPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EBrief");
    public static string DocumentPath => Path.Combine(AppDataPath, "documents");
    public static string GetMimeType(string fileExtension)
    {
        return fileExtension switch
        {
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".txt" => "text/plain",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".pdf" => "application/pdf",
            _ => "application/octet-stream", // Default MIME type for unknown file types
        };
    }
}
