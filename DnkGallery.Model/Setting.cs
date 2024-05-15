using System.Text.Json;
namespace DnkGallery.Model;

public enum Source {
    Local,
    Git
}

public sealed record Setting {
    public Source Source { get; set; } = Source.Local;
    public string SourcePath { get; set; } = ".";
    
    private string GetSettingPath() {
        var folderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var combine = Path.Combine(folderPath,"dnkgallery_setting.json");
        return combine;
    }
    
    public void Load() {
        var settingPath = GetSettingPath();
        if (!File.Exists(settingPath)) {
            Save(this).Wait();
        }
        var text = File.ReadAllText(settingPath);
        var deserialize = JsonSerializer.Deserialize<Setting>(text);
        Source = deserialize.Source;
        SourcePath = deserialize.SourcePath;
        
        SettingChanged?.Invoke(this, this);
    }
    
    public async Task Save(Setting? setting) {
        Source = setting.Source;
        SourcePath = setting.SourcePath;
        

        var serialize = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(GetSettingPath(),serialize);
        
        SettingChanged?.Invoke(this, this);
    }
    
    public event EventHandler<Setting> SettingChanged;
}
