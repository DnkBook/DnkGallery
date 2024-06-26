﻿using System.Globalization;
namespace DnkGallery.Model;

public record Ana {
    public Ana(string path, DateTime dateTime, string name) {
        Path = path;
        DateTime = dateTime;
        Name = name;
    }
    
    public string Path { get; set; }
    public string Name { get; set; }
    public DateTime DateTime { get; set; }
    
    public bool LocalExists { get; set; }
    
    public byte[]? ImageBytes { get; set; }
    public static string NewFileName => DateTime.Now.ToString("yyyy-MM-dd_HHmmss") + ".jpg";
    
    public static bool NameFilter(string name) =>
        name.ToLower().EndsWith(".jpg") || name.ToLower().EndsWith(".png") ||
        name.ToLower().EndsWith(".jpeg");
    
    public Ana(string name, string path) {
        var fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(name);
        if (DateTime.TryParseExact(fileNameWithoutExtension, "yyyy-MM-dd_HHmmss", null, DateTimeStyles.None,out var dateTime)) {
            DateTime = dateTime;
        }
        Path = path;
        Name = name;
    }
    
    public Ana(string path) {
        var fileName = System.IO.Path.GetFileName(path);
        var fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(fileName);
        if (DateTime.TryParseExact(fileNameWithoutExtension, "yyyy-MM-dd_HHmmss", null, DateTimeStyles.None,out var dateTime)) {
            DateTime = dateTime;
        }
        Path = path;
        Name = fileName;
    }
}
