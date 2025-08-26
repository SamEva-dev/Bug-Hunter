
using BugHunter.Models;

namespace BugHunter.Services;

public class BugSpawner
{
    private readonly Random _rng = new();
    private float _timer;


    public void Reset() => _timer = 0f;


    public void Update(GameState s, float dt)
    {
        _timer -= dt;
        var interval = MathF.Max(0.35f, 1.2f - 0.2f * s.Difficulty);
        if (_timer <= 0)
        {
            Spawn(s);
            _timer += interval;
        }
    }


    private void Spawn(GameState s)
    {
        bool rare = _rng.NextDouble() < 0.2;
        string type = rare ? PickRare() : PickCommon();
        float x = (float)_rng.Next(40, 760);
        float y = -20f;
        float vy = 60f + 40f * s.Difficulty + (float)_rng.NextDouble() * 40f;
        int points = rare ? 250 : 100;
        float r = rare ? 22f : 18f;


        s.Bugs.Add(new Bug(Guid.NewGuid(), type, x, y, 0f, vy, points, r, rare));
    }


    private string PickCommon() => _rng.Next(0, 3) switch { 0 => "NullRef", 1 => "SQLi", _ => "N+1" };
    private string PickRare() => _rng.Next(0, 3) switch { 0 => "Race", 1 => "Leak", _ => "Deadlock" };
}