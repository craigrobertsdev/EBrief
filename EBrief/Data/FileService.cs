using Microsoft.Win32;

namespace EBrief.Data;
public class FileService : IFileService
{
    public void SaveFile(string fileName, string json)
    {
        SaveFileDialog dialog = new();
        dialog.FileName = fileName;
        dialog.DefaultExt = ".court";
        dialog.Filter = "Court files|*.court";

        bool? result = dialog.ShowDialog();
        if (result == true)
        {
            File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName), json);
        }
    }
}
