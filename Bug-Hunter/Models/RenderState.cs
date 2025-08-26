
namespace BugHunter.Models;

public class RenderState
{
    public bool running { get; set; }
    public int score { get; set; }
    public int combo { get; set; }
    public float time { get; set; }
    public float difficulty { get; set; }
    public List<BugView> bugs { get; set; } = new();
    public List<ParticleView> particles { get; set; } = new();


    public class BugView
    {
        public float x { get; set; }
        public float y { get; set; }
        public float r { get; set; }
        public string kind { get; set; } = string.Empty;
        public bool rare { get; set; }
    }

    public class ParticleView
    {
        public float x { get; set; }
        public float y { get; set; }
        public float life { get; set; }
    }
}