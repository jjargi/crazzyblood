using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class NuclearProjectile : Area2D
{
    [Export] private float Speed = 100f;
    [Export] private PackedScene ExplosionEffect;
    [Export] private TileMapLayer _TileMapLayer;
    [Export] private float DamageMultiplier = 1f;
    [Export] private float WaveDelay = 0.2f; // Tiempo entre capas de explosión

    private Vector2 _targetPosition;
    private float _damage;
    private Vector2 _direction;
    private bool _hasExploded = false;
    private float _minDistanceToTarget = 32f;

    // Capas de la explosión nuclear (cuadrada)
    private Dictionary<int, List<Vector2I>> _blastLayers = new()
    {
        // Capa 0: Centro (3x3)
        {
            0, new List<Vector2I> {
                Vector2I.Zero,
                new(-1, 0), new(1, 0), new(0, -1), new(0, 1),
                new(-1, -1), new(1, 1), new(-1, 1), new(1, -1)
            }
        },
        // Capa 1: Borde interior (5x5 sin el 3x3 central)
        {
            1, new List<Vector2I> {
                new(-2, 0), new(2, 0), new(0, -2), new(0, 2),
                new(-2, -1), new(2, 1), new(-2, 1), new(2, -1),
                new(-1, -2), new(1, 2), new(-1, 2), new(1, -2)
            }
        },
        // Capa 2: Esquinas (completa el 5x5)
        {
            2, new List<Vector2I> {
                new(-2, -2), new(2, 2), new(-2, 2), new(2, -2)
            }
        }
    };

    // Multiplicadores de daño por capa
    private Dictionary<int, float> _damageMultipliers = new()
    {
        {0, 3.0f}, // Centro - Daño máximo
        {1, 2.0f}, // Borde interior - Daño medio
        {2, 1.0f}  // Esquinas - Daño mínimo
    };

    public void Initialize(Vector2 position, Vector2 direction, float baseDamage, float speed, TileMapLayer tileMap, Vector2 targetPosition)
    {
        _targetPosition = targetPosition;
        GlobalPosition = position;
        _direction = direction.Normalized();
        _damage = baseDamage;
        Speed = speed;
        _TileMapLayer = tileMap;
        Rotation = _direction.Angle();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_hasExploded) return;

        Position += _direction * Speed * (float)delta;

        if (GlobalPosition.DistanceTo(_targetPosition) <= _minDistanceToTarget)
        {
            TriggerExplosion();
        }
        if (_TileMapLayer != null)
        {
            Vector2I tileCoords = _TileMapLayer.LocalToMap(GlobalPosition);
            int sourceId = _TileMapLayer.GetCellSourceId(tileCoords);

            if (sourceId == -1)
            {
                GD.Print($"Misil colisiona con tile en {tileCoords}. Destruyendo.");
                QueueFree();
            }
        }
    }

    private void OnBodyEntered(Node2D body)
    {
        if (_hasExploded) return;
        TriggerExplosion();
    }

    private void TriggerExplosion()
    {
        _hasExploded = true;
        StartNuclearBlastSequence();
        // QueueFree se llama al final de la secuencia
    }

    private async void StartNuclearBlastSequence()
    {
        Vector2I centerTile = _TileMapLayer.LocalToMap(GlobalPosition);

        // Procesamos cada capa en orden
        foreach (var layer in _blastLayers.OrderBy(l => l.Key))
        {
            int layerNum = layer.Key;
            float damageMultiplier = _damageMultipliers[layerNum];

            foreach (var offset in layer.Value)
            {
                Vector2I tilePos = centerTile + offset;
                if (IsTileValid(tilePos))
                {
                    SpawnExplosionAt(tilePos, damageMultiplier);
                }
            }

            // Esperar antes de la siguiente capa
            await ToSignal(GetTree().CreateTimer(WaveDelay), "timeout");
        }

        // Cuando todas las explosiones han terminado
        QueueFree();
    }

    private void SpawnExplosionAt(Vector2I tilePos, float damagePercent)
    {
        if (ExplosionEffect != null)
        {
            var explosion = ExplosionEffect.Instantiate<Node2D>();
            GetParent().AddChild(explosion);
            explosion.GlobalPosition = _TileMapLayer.MapToLocal(tilePos);

            if (explosion is ExplosionEffect effect)
            {
                float finalDamage = _damage * DamageMultiplier * damagePercent;
                effect.Initialize(finalDamage);
            }
        }
    }

    private bool IsTileValid(Vector2I tilePos)
    {
        return _TileMapLayer.GetCellSourceId(tilePos) != -1;
    }
}