using BugHunter.Models.Invaders;
using Microsoft.JSInterop;

namespace BugHunter.Services;


public class InvadersGameService
{
    private readonly DotNetObjectReference<InvadersGameService> _self;
    private readonly List<Invader> _invaders = new();
    private readonly List<Shot> _shots = new();
    private Player _player = new(380, 560, 40, 16, 3);

    private float _dir = 1f;         // direction invaders (1 -> droite, -1 -> gauche)
    private float _stepTimer;        // descente périodique
    private float _cooldown;         // tir joueur
    private bool _running;
    private int _score;

    private float _enemyTimer;           // cadence de tir ennemie
    private readonly Random _rng = new();
    private bool _invincible;            // petite invincibilité après coup
    private float _invincibleTime;

    private float _axis;         // -1 gauche, 0 neutre, +1 droite
    private bool _fireHeld;      // espace maintenu




    public Action<int>? OnGameOver { get; set; }

    public InvadersGameService()
    {
        _self = DotNetObjectReference.Create(this);
    }

    public DotNetObjectReference<InvadersGameService> GetJsRef() => _self;

    [JSInvokable]
    public void Start()
    {
        _running = true; _score = 0;
        _player = _player with { X = 380, Lives = 3 };
        _shots.Clear(); _invaders.Clear();
        _dir = 1f; _stepTimer = 0.6f; _cooldown = 0f;

        _enemyTimer = 1.2f;
        _invincible = false;
        _invincibleTime = 0f;

        _axis = 0f;
        _fireHeld = false;

        // grille 8 x 4
        for (int r = 0; r < 4; r++)
            for (int c = 0; c < 8; c++)
                _invaders.Add(new Invader(100 + c * 70, 80 + r * 50, 40, 24, true));
    }

    [JSInvokable]
    public InvadersRenderState Tick(float dt)
    {
        if (_running)
        {
            // gestion invincibilité joueur
            if (_invincible)
            {
                _invincibleTime -= dt;
                if (_invincibleTime <= 0)
                    _invincible = false;
            }

            // cooldown tir joueur
            if (_cooldown > 0) _cooldown -= dt;

            // ⇨ Mouvement joueur par axe * dt
            const float PlayerSpeed = 320f; // ajuste ici (300–380)
            float nx = _player.X + _axis * PlayerSpeed * dt;
            _player = _player with { X = Math.Clamp(nx, 10, 800 - _player.Width - 10) };

            // déplacements horizontaux des invaders
            const float InvaderSpeed = 36f; // avant 40f
            bool hitSide = false;
            for (int i = 0; i < _invaders.Count; i++)
            {
                var inv = _invaders[i];
                if (!inv.Alive) continue;
                var ix = inv.X + _dir * InvaderSpeed * dt;
                if (ix < 20 || ix + inv.W > 780) hitSide = true;
                _invaders[i] = inv with { X = ix };
            }

            // descente si touche un bord
            if (hitSide)
            {
                _dir *= -1f;
                for (int i = 0; i < _invaders.Count; i++)
                {
                    var inv = _invaders[i];
                    if (!inv.Alive) continue;

                    var ny = inv.Y + 16;
                    _invaders[i] = inv with { Y = ny };

                    if (ny + inv.H >= _player.Y)
                    {
                        GameOver();
                    }
                }
            }

            // tirs ennemis (toutes les ~1s)
            _enemyTimer -= dt;
            if (_enemyTimer <= 0f)
            {
                var alive = _invaders.Where(v => v.Alive).ToList();
                if (alive.Count > 0)
                {
                    var byCol = alive.GroupBy(v => (int)((v.X - 100) / 70))
                                     .OrderBy(_ => _rng.Next()).First();
                    var shooter = byCol.OrderByDescending(v => v.Y).First();
                    _shots.Add(new Shot(shooter.X + shooter.W / 2,
                                        shooter.Y + shooter.H,
                                        0, +160f, false));
                }
                _enemyTimer = MathF.Max(0.65f, 1.2f - 0.05f * (_score / 240f));
            }

            // déplacements des tirs
            for (int i = _shots.Count - 1; i >= 0; i--)
            {
                var s = _shots[i];
                nx = s.X + s.VX * dt;
                var ny = s.Y + s.VY * dt;
                if (ny < -10 || ny > 620)
                {
                    _shots.RemoveAt(i);
                    continue;
                }
                _shots[i] = s with { X = nx, Y = ny };
            }

            // collisions tir joueur → invaders
            for (int i = _shots.Count - 1; i >= 0; i--)
            {
                var s = _shots[i];
                if (!s.FromPlayer) continue;

                for (int j = 0; j < _invaders.Count; j++)
                {
                    var inv = _invaders[j];
                    if (!inv.Alive) continue;

                    if (Aabb(s.X - 2, s.Y - 8, 4, 8, inv.X, inv.Y, inv.W, inv.H))
                    {
                        _shots.RemoveAt(i);
                        _invaders[j] = inv with { Alive = false };
                        _score += 50;
                        break;
                    }
                }
            }

            // collisions tir ennemi → joueur
            for (int i = _shots.Count - 1; i >= 0; i--)
            {
                var s = _shots[i];
                if (s.FromPlayer) continue;

                if (Aabb(s.X - 2, s.Y - 8, 4, 8, _player.X, _player.Y, _player.Width, _player.Height))
                {
                    _shots.RemoveAt(i);
                    HitPlayer();
                }
            }

            // victoire si plus aucun invader vivant
            if (_invaders.TrueForAll(v => !v.Alive))
            {
                GameOver();
            }
        }

        return Snapshot();
    }

    private void GameOver()
    {
        _running = false;
        OnGameOver?.Invoke(_score);
    }

    [JSInvokable]
    public Task Move(int axis)
    {
        if (!_running) return Task.CompletedTask;
        float nx = _player.X + axis * 220f * (1 / 60f);
        _player = _player with { X = Math.Clamp(nx, 10, 800 - _player.Width - 10) };
        return Task.CompletedTask;
    }

    [JSInvokable]
    public Task Shoot()
    {
        if (!_running) return Task.CompletedTask;
        if (_cooldown > 0) return Task.CompletedTask;
        _cooldown = 0.25f;
        _shots.Add(new Shot(_player.X + _player.Width / 2, _player.Y - 8, 0, -400f, true));
        return Task.CompletedTask;
    }

    [JSInvokable] public void SetAxis(float axis) => _axis = Math.Clamp(axis, -1f, 1f);
    [JSInvokable] public void SetFire(bool down) => _fireHeld = down;


    private void HitPlayer()
    {
        if (_invincible || !_running) return;
        var lives = _player.Lives - 1;
        _player = _player with { Lives = lives };
        _invincible = true;
        _invincibleTime = 1.2f; // 1.2s d’invincibilité visuelle

        if (lives <= 0) GameOver();
    }

    [JSInvokable] public void TogglePause() => _running = !_running;

    private static bool Aabb(float x1, float y1, float w1, float h1, float x2, float y2, float w2, float h2)
      => x1 < x2 + w2 && x1 + w1 > x2 && y1 < y2 + h2 && y1 + h1 > y2;

    private InvadersRenderState Snapshot() => new()
    {
        running = _running,
        score = _score,
        lives = _player.Lives,
        player = new InvadersRenderState.Entity { x = _player.X, y = _player.Y, w = _player.Width, h = _player.Height, alive = true },
        invaders = _invaders.Select(v => new InvadersRenderState.Entity { x = v.X, y = v.Y, w = v.W, h = v.H, alive = v.Alive }).ToList(),
        shots = _shots.Select(s => new InvadersRenderState.Entity { x = s.X - 2, y = s.Y - 8, w = 4, h = 8, alive = true }).ToList()
    };
}
