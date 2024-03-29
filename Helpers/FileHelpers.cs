﻿namespace CourtSystem.Helpers;
public static class FileHelpers {
    public static string GetMimeType(string fileExtension) {
        return fileExtension switch {
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".txt" => "text/plain",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".pdf" => "application/pdf",
            _ => "application/octet-stream", // Default MIME type for unknown file types
        };
    }
}
