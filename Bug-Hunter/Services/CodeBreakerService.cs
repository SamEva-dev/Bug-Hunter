namespace BugHunter.Services;

public enum CBLevel { Easy, Normal, Hard }

public record WordInfo(string Word, string Category, CBLevel Level, string? Clue = null);

public class CodeBreakerService
{
    private static readonly WordInfo[] Words = new[]
    {
        new WordInfo("CLASS","C#",     CBLevel.Easy,   "Type C# de base"),
        new WordInfo("EVENT","C#",     CBLevel.Normal, "Pattern d'abonnement"),
        new WordInfo("CACHE","Cloud",  CBLevel.Easy,   "Stockage rapide en mémoire"),
        new WordInfo("TOKEN","Web",    CBLevel.Normal, "Auth: JWT, accès…"),
        new WordInfo("INDEX","DB",     CBLevel.Easy,   "Accélère les requêtes"),
        new WordInfo("QUEUE","Cloud",  CBLevel.Normal, "FIFO, messages"),
        new WordInfo("AZURE","Cloud",  CBLevel.Normal, "Cloud Microsoft"),
        new WordInfo("DEBUG","Dev",    CBLevel.Easy,   "Breakpoints, pas à pas…"),
        new WordInfo("MERGE","Git",    CBLevel.Normal, "Fusion de branches"),
        new WordInfo("SOLID","Arch",   CBLevel.Hard,   "5 principes d’OO"),
        new WordInfo("REACT","Web",    CBLevel.Hard,   "Librairie UI JS"),
        new WordInfo("ARRAY","C#",     CBLevel.Easy,   "Structure indexée"),
        new WordInfo("STATE","Arch",   CBLevel.Normal, "Machine d'états / store"),
        new WordInfo("ERROR","Dev",    CBLevel.Easy,   "Exception, stacktrace…"),
        new WordInfo("RANGE","C#",     CBLevel.Normal, "[start..end)"),
        new WordInfo("BYTES","Sys",    CBLevel.Normal, "Unités binaires"),
        new WordInfo("MONGO","DB",     CBLevel.Normal, "Base NoSQL"),
        new WordInfo("REDIS","DB",     CBLevel.Easy,   "Cache clé/valeur"),
    }.Where(w => w.Word.Length == 5).ToArray();

    private static readonly HashSet<string> Dict = Words.Select(w => w.Word).ToHashSet();

    public WordInfo PickWord(CBLevel level)
    {
        var pool = Words.Where(w => w.Level == level).DefaultIfEmpty(Words[0]).ToArray();
        var r = Random.Shared.Next(pool.Length);
        return pool[r];
    }

    public bool IsValid(string guess)
        => guess.Length == 5 && Dict.Contains(guess.ToUpperInvariant());
}