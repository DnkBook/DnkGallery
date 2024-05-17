namespace DnkGallery.Model;

public class Chapter(string name, string dir,bool hasChildren, string[] anchors) {
    public string Name { get; set; } = name;
    public string Dir { get; set; } = dir;
    public bool HasChildren { get; set; } = hasChildren;
    public string[] Anchors { get; set; } = anchors;
    
    public void Deconstruct(out string Name, out string Dir, out bool HasChildren, out string[] Anchors) {
        Name = this.Name;
        Dir = this.Dir;
        HasChildren = this.HasChildren;
        Anchors = this.Anchors;
    }
}
