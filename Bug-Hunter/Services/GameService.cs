
using BugHunter.Models;
using Microsoft.JSInterop;

namespace BugHunter.Services;

public class GameService
{
    private readonly BugSpawner _spawner;
    private readonly DotNetObjectReference<GameService> _selfRef;
    private readonly GameState _state = new();

    public Action<int>? OnGameOver { get; set; }

    public GameService(BugSpawner spawner)
    {
        _spawner = spawner;
        _selfRef = DotNetObjectReference.Create(this);
    }

    public DotNetObjectReference<GameService> GetJsRef() => _selfRef;


    [JSInvokable]
    public RenderState Tick(float dt)
    {
        if (_state.Running)
        {
            _state.TimeLeft -= dt;
            if (_state.TimeLeft <= 0)
            {
                _state.TimeLeft = 0;
                _state.Running = false;
                OnGameOver?.Invoke(_state.Score);
            }
            else
            {
                _state.Difficulty += dt * 0.03f;
                _spawner.Update(_state, dt);
                UpdateBugs(_state, dt);
                UpdateParticles(_state, dt);
            }
        }

        return Snapshot();
    }

    [JSInvokable]
    public void OnClick(float x, float y)
    {
        if (!_state.Running) return;

        for (int i = 0; i < _state.Bugs.Count; i++)
        {
            var b = _state.Bugs[i];
            var dx = b.X - x;
            var dy = b.Y - y;

            if (dx * dx + dy * dy <= b.Radius * b.Radius)
            {
                _state.Bugs.RemoveAt(i);

                _state.Combo++;
                var mult = 1.0 + Math.Min(0.5, _state.Combo * 0.1);
                _state.Score += (int)(b.Points * mult);

                for (int k = 0; k < 10; k++)
                {
                    var angle = k * (MathF.PI * 2 / 10);
                    _state.Particles.Add(new Particle(
                        X: b.X,
                        Y: b.Y,
                        VX: MathF.Cos(angle) * 120f,
                        VY: MathF.Sin(angle) * 120f,
                        Life: 0.4f
                    ));
                }

                return;
            }
        }

        _state.Combo = 0;
    }


    [JSInvokable]
    public void Start()
    {
        _state.Running = true;
        _state.Score = 0;
        _state.Combo = 0;
        _state.TimeLeft = 90f;
        _state.Difficulty = 1f;
        _state.Bugs.Clear();
        _spawner.Reset();
    }

    [JSInvokable] public void TogglePause() => _state.Running = !_state.Running;



    private void UpdateBugs(GameState s, float dt)
    {
        for (int i = s.Bugs.Count - 1; i >= 0; i--)
        {
            var b = s.Bugs[i];
            var nx = b.X + b.VX * dt;
            var ny = b.Y + b.VY * dt;

            if (ny > 600 + 60)
                s.Bugs.RemoveAt(i);
            else
                s.Bugs[i] = b with { X = nx, Y = ny };
        }

    }

    private void UpdateParticles(GameState s, float dt)
    {
        for (int i = s.Particles.Count - 1; i >= 0; i--)
        {
            var p = s.Particles[i];
            var nx = p.X + p.VX * dt;
            var ny = p.Y + p.VY * dt;
            var life = p.Life - dt;

            if (life <= 0)
                s.Particles.RemoveAt(i);
            else
                s.Particles[i] = p with { X = nx, Y = ny, Life = life };
        }
    }


    private RenderState Snapshot() => new()
    {
        running = _state.Running,
        score = _state.Score,
        combo = _state.Combo,
        time = MathF.Round(_state.TimeLeft, 1),
        difficulty = _state.Difficulty,
        bugs = _state.Bugs.Select(b => new RenderState.BugView
        {
            x = b.X,
            y = b.Y,
            r = b.Radius,
            kind = b.Type,
            rare = b.Rare
        }).ToList(),
        particles = _state.Particles.Select(p => new RenderState.ParticleView
        {
            x = p.X,
            y = p.Y,
            life = p.Life
        }).ToList()
    };


}