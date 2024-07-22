﻿using EBrief.Shared.Data;
using EBrief.Shared.Helpers;
using EBrief.Shared.Models;
using EBrief.Shared.Models.Data;
using EBrief.Shared.Models.UI;
using Microsoft.Win32;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace EBrief.Data;
public class FileService : IFileService
{
    private readonly IDataAccess _dataAccess;
    public FileService(IDataAccess dataAccess)
    {
        _dataAccess = dataAccess;
    }
    public async Task SaveFile(CourtList courtList)
    {
        var dialog = new SaveFileDialog
        {
            FileName = $"Court {courtList.CourtRoom} {courtList.CourtCode} - {courtList.CourtDate.Day} {courtList.CourtDate:MMM} {courtList.CourtDate.Year}.court",
            DefaultExt = ".court",
            Filter = "Court files|*.court"
        };

        bool? result = dialog.ShowDialog();
        if (result == true)
        {
            var courtListModel = await _dataAccess.GetCourtList(courtList.CourtCode, courtList.CourtDate, courtList.CourtRoom);
            File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), dialog.FileName), courtListModel!.SerialiseToJson());
        }
    }

    public async Task<(CourtListEntry?, string?)> LoadCourtFile()
    {
        var dialog = new OpenFileDialog();
        var result = dialog.ShowDialog();
        if (result is null || result == false) return (null, null);

        if (!File.Exists(dialog.FileName))
        {
            throw new FileNotFoundException("File does not exist");
        }

        try
        {
            string json = File.ReadAllText(dialog.FileName);
            var courtList = JsonSerializer.Deserialize<CourtListModel>(json);

            if (courtList is null)
            {
                return (null, "Expected .court file. Loading court list failed");
            }

            var courtListEntry = new CourtListEntry(courtList.CourtCode, courtList.CourtDate, courtList.CourtRoom);
            if (await _dataAccess.CheckCourtListExists(courtListEntry))
            {
                var button = MessageBoxButton.YesNo;
                var proceed = MessageBox.Show("Court list already exists. Do you want to overwrite?", "Confirmation", button, MessageBoxImage.Question);
                if (proceed == MessageBoxResult.No)
                {
                    var entries = await _dataAccess.GetSavedCourtLists();
                    var entry = entries.Where(
                        cl => cl.CourtDate == courtListEntry.CourtDate
                        && cl.CourtCode == courtListEntry.CourtCode
                        && cl.CourtRoom == courtListEntry.CourtRoom)
                    .First();

                    return (entry, null);
                }
            }

            await _dataAccess.CreateCourtList(courtList);
            return (courtListEntry, null);
        }
        catch (Exception e)
        {
            return (null, e.Message);
        }
    }

    public void CreateDocumentDirectory()
    {
        if (!Directory.Exists(FileHelpers.DocumentPath))
        {
            Directory.CreateDirectory(FileHelpers.DocumentPath);
        }
    }

    public async Task SaveDocument(Stream stream, string fileName)
    {
        var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);

        using var fileStream = new FileStream($"{FileHelpers.AppDataPath}/documents/{fileName}", FileMode.Create, FileAccess.Write);
        fileStream.Write(memoryStream.ToArray());
    }
}
