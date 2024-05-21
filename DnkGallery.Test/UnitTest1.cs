using LibGit2Sharp;

namespace DnkGallery.Test;

public class Tests() {
    
    [SetUp]
    public void Setup() {
    }
    
    // [Test]
    public void Fetch() {
        var logMessage = "";
        using var repo = new Repository("C:\\Users\\NianChen\\Pictures\\dnk\\temp");
        var remote = repo.Network.Remotes["origin"];
        var refSpecs = remote.FetchRefSpecs.Select(x => x.Specification);
        Commands.Fetch(repo, remote.Name, refSpecs, null, logMessage);
    }
    [Test]
    public void Pull() {
        var logMessage = "";
        using var repo = new Repository("C:\\Users\\NianChen\\Pictures\\dnk\\temp");
        var remote = repo.Network.Remotes["origin"];
        var refSpecs = remote.FetchRefSpecs.Select(x => x.Specification);
        var mergeResult = Commands.Pull(repo,new Signature("xueque","maqueyuxue@outlook.com",DateTimeOffset.Now),
            new PullOptions(){MergeOptions = new MergeOptions(){FastForwardStrategy = FastForwardStrategy.Default}});
    }
    // [Test]
    public void Clone() {
        var clone = Repository.Clone("https://github.com/Ishning/dnkFuns", "C:\\Users\\NianChen\\Pictures\\dnk\\temp");
        Console.WriteLine(clone);
    }
}
