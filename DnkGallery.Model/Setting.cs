using System.Text.Json;
namespace DnkGallery.Model;

public enum Source {
    Local,
    Git
}

public sealed record Setting {
    public Source Source { get; set; } = Source.Local;
    public string LocalPath { get; set; } = ".";
    public string GitRepos { get; set; } = "Ishning/dnkFuns";
    
    public string GitAccessToken { get; set; }

    public string SourcePath => Source switch {
        Source.Local => LocalPath,
        Source.Git => Path.AltDirectorySeparatorChar.ToString(),
        _ => throw new ArgumentOutOfRangeException()
    };
    
    
    private JsonSerializerOptions jsonSerializerOptions = new() { WriteIndented = true };
    
    private string GetSettingPath() {
        var folderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var combine = Path.Combine(folderPath, "dnkgallery_setting.json");
        return combine;
    }
    
    public void Load() {
        var settingPath = GetSettingPath();
        if (!File.Exists(settingPath)) {
            var serialize = JsonSerializer.Serialize(this, jsonSerializerOptions);
            File.WriteAllText(settingPath, serialize);
        }
        var text = File.ReadAllText(settingPath);
        var deserialize = JsonSerializer.Deserialize<Setting>(text);
        
        Update(deserialize);
        SettingChanged?.Invoke(this, this);
    }
    
    public async Task SaveAsync(Setting? setting) {
        Update(setting);
        var serialize = JsonSerializer.Serialize(this, jsonSerializerOptions);
        await File.WriteAllTextAsync(GetSettingPath(), serialize);
        
        SettingChanged?.Invoke(this, this);
    }
    
    
    private void Update(Setting setting) {
        Source = setting.Source;
        LocalPath = setting.LocalPath;
        GitRepos = setting.GitRepos;
        GitAccessToken = setting.GitAccessToken;
    }
    
    public event EventHandler<Setting> SettingChanged;
}
