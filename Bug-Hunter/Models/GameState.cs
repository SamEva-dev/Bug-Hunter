
namespace BugHunter.Models;
public class GameState
{
    public bool Running { get; set; }
    public int Score { get; set; }
    public int Combo { get; set; }
    public float TimeLeft { get; set; } = 90f;
    public float Difficulty { get; set; } = 1f;
    public List<Bug> Bugs { get; } = new();
    public List<Particle> Particles { get; } = new();
}