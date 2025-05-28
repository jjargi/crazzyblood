using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class ExplosiveProjectile : Area2D
{
    [Export] private float Speed = 200f;
    [Export] private PackedScene ExplosionEffect;
    [Export] private TileMapLayer _TileMapLayer;
    [Export] private float DamageMultiplier = 1f;
    [Export] private float WaveDelay = 0.2f; // Tiempo entre cada anillo de explosión

    private Vector2 _targetPosition;
    private float _damage;
    private Vector2 _direction;
    private bool _hasExploded = false;
    private float _minDistanceToTarget = 32f;
    public event Action<Node2D> OnExplode;
    // Mapa de daño por zonas, organizado por distancia desde el centro
    private Dictionary<int, List<Vector2I>> _damageZonesByRadius = new()
    {
        {0, new List<Vector2I> {Vector2I.Zero}}, // Centro
        {1, new List<Vector2I> {new(-1, -1), new(1, 1), new(-1, 1), new(1, -1)}}, // Primer anillo
        {2, new List<Vector2I> {new(-2, -2), new(2, 2), new(-2, 2), new(2, -2)}}  // Segundo anillo
    };

    // Multiplicadores de daño por distancia
    private Dictionary<int, float> _damageMultipliers = new()
    {
        {0, 1.5f}, // Centro (150%)
        {1, 1.0f},  // Primer anillo (100%)
        {2, 0.5f}   // Segundo anillo (50%)
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
        StartExplosionWaveSequence();
        ApplyDamageByZone();
        // No llamamos QueueFree() aquí porque queremos que el nodo persista hasta que terminen todas las explosiones
    }

    private async void StartExplosionWaveSequence()
    {
        Vector2I centerTile = _TileMapLayer.LocalToMap(GlobalPosition);

        // Procesamos cada anillo en orden
        foreach (var radiusGroup in _damageZonesByRadius.OrderBy(k => k.Key))
        {
            int radius = radiusGroup.Key;
            float damageMultiplier = _damageMultipliers[radius];

            foreach (var offset in radiusGroup.Value)
            {
                Vector2I tilePos = centerTile + offset;
                if (IsTileValid(tilePos))
                {
                    SpawnExplosionAt(tilePos, damageMultiplier);
                }
            }

            // Esperar antes del siguiente anillo
            await ToSignal(GetTree().CreateTimer(WaveDelay), "timeout");
        }

        // Cuando todas las explosiones han terminado, liberar el nodo
        QueueFree();
    }

    private void SpawnExplosionAt(Vector2I tilePos, float damagePercent)
    {
        if (ExplosionEffect != null)
        {
            var explosion = ExplosionEffect.Instantiate<Node2D>();
            GetParent().AddChild(explosion);
            explosion.GlobalPosition = _TileMapLayer.MapToLocal(tilePos);

            // Disparar el evento cuando se crea una explosión
            OnExplode?.Invoke(explosion);

            if (explosion is ExplosionEffect effect)
            {
                float finalDamage = _damage * DamageMultiplier * damagePercent;
                effect.Initialize(finalDamage);
            }
        }
    }

    private void ApplyDamageByZone()
    {
        Vector2I explosionCenter = _TileMapLayer.LocalToMap(GlobalPosition);
        var enemies = GetTree().GetNodesInGroup("enemies");

        foreach (Node2D enemy in enemies)
        {
            Vector2I enemyTilePos = _TileMapLayer.LocalToMap(enemy.GlobalPosition);
            Vector2I relativePos = enemyTilePos - explosionCenter;

            // Encontrar a qué radio pertenece este enemigo
            foreach (var radiusGroup in _damageZonesByRadius)
            {
                if (radiusGroup.Value.Contains(relativePos))
                {
                    float damageMultiplier = _damageMultipliers[radiusGroup.Key];
                    float finalDamage = _damage * DamageMultiplier * damageMultiplier;
                    enemy.Call("TakeDamage", finalDamage);
                    break;
                }
            }
        }
    }

    private bool IsTileValid(Vector2I tilePos)
    {
        return _TileMapLayer.GetCellSourceId(tilePos) != -1;
    }
}