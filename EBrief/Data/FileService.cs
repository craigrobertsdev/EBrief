using Microsoft.Win32;

namespace EBrief.Data;
public class FileService : IFileService
{
    public void SaveFile(string fileName, string json)
    {
        var dialog = new SaveFileDialog();
        dialog.FileName = fileName;
        dialog.DefaultExt = ".court";
        dialog.Filter = "Court files|*.court";

        bool? result = dialog.ShowDialog();
        if (result == true)
        {
            Console.WriteLine(dialog.FileName);
            File.WriteAllText(dialog.FileName, json);
        }
    }
}
