namespace BugHunter.Models.Invaders;

public class InvadersRenderState
{
    public bool running { get; set; }
    public int score { get; set; }
    public int lives { get; set; }
    public List<Entity> invaders { get; set; } = new();
    public List<Entity> shots { get; set; } = new();
    public Entity player { get; set; } = new();

    public class Entity
    {
        public float x { get; set; }
        public float y { get; set; }
        public float w { get; set; }
        public float h { get; set; }
        public bool alive { get; set; }
    }
}