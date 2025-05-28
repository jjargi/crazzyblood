
using Godot;
using System;
using System.Collections.Generic;

public partial class EnemyManager : Node2D
{
    [Export] public PackedScene EnemyScene { get; set; }
    [Export] public int MaxEnemies = 10;
    [Export] public float SpawnInterval = 2.0f;
    [Export] public float MinDistanceToPlayer = 5f; // Distancia mínima en celdas
                                                    // Variables de estado
    private TileMapLayer _currentMap;
    private Player _player;
    private EnemyManager _enemyManager;
    private int _currentWave = 0;
    private int _currentLevel = 1;
    private int _enemiesDefeatedInCurrentWave = 0;
    private TileMapLayer _tileMap;
    private List<Enemy> _enemies = new();
    private Timer _spawnTimer;
    private DifficultySettings _currentDifficulty;
    public event Action EnemyDefeated;
    private int _enemiesSpawnedThisWave = 0;
    private int _totalEnemiesForWave = 0;
    private bool _isLastWave = false;
    private int _enemiesSpawned = 0;
    private bool _waveActive = false;

    private void OnEnemyDeath(Enemy enemy)
    {
        _enemies.Remove(enemy);
        _enemiesDefeatedThisWave++;
        EnemyDefeated?.Invoke();
        GD.Print($"Enemigo eliminado. Derrotados: {_enemiesDefeatedThisWave}/{_totalEnemiesForWave}");
    }
    public void Reinitialize(Player player, TileMapLayer tileMap)
    {
        _player = player;
        _tileMap = tileMap;
    }
    private int _enemiesDefeatedThisWave = 0;
    public void StartWave(int enemyCount, DifficultySettings difficulty, bool isLastWave = false)
    {
        ClearEnemies(); // Limpiar enemigos previos

        _totalEnemiesForWave = enemyCount;
        _enemiesSpawnedThisWave = 0;
        _enemiesDefeatedThisWave = 0;
        _waveActive = true;
        _currentDifficulty = difficulty;
        _isLastWave = isLastWave;

        GD.Print($"Preparando oleada. Enemigos a generar: {_totalEnemiesForWave}");

        // Configurar timer
        _spawnTimer.WaitTime = SpawnInterval * difficulty.SpawnRateMultiplier;
        _spawnTimer.Start();
    }
    public void Initialize(Player player, TileMapLayer tileMap)
    {
        _player = player;
        _tileMap = tileMap;

        _spawnTimer = new Timer();
        AddChild(_spawnTimer);
        _spawnTimer.Timeout += SpawnEnemy;
        _spawnTimer.Start(SpawnInterval);
    }
    private void SpawnEnemy()
    {
        // Si ya hemos spawnado todos los enemigos de la oleada
        if (_enemiesSpawned >= _totalEnemiesForWave)
        {
            _spawnTimer.Stop();
            _waveActive = false;
            return;
        }

        // Si hay muchos enemigos activos, esperar antes de spawnear más
        if (_enemies.Count >= MaxEnemies)
        {
            return;
        }

        // Spawnear nuevo enemigo
        var enemy = EnemyScene.Instantiate<Enemy>();
        //enemy.Initialize(_player, new EnemyStats, _tileMap
        //{
        //    Speed = GD.RandRange(80f, 120f) * _currentDifficulty.EnemySpeedMultiplier,
        //    Health = 1,
        //    Damage = (int)(1 * _currentDifficulty.EnemyDamageMultiplier),
        //    AttackCooldown = GD.RandRange(1.5f, 3f),
        //    ProjectileSpeed = GD.RandRange(150f, 250f),
        //    TileSize = _tileMap.TileSet.TileSize.X,
        //});
        var stats = new EnemyStats
        {
            Speed = GD.RandRange(80f, 120f) * _currentDifficulty.EnemySpeedMultiplier,
            Health = (int)_currentDifficulty.EnemyHealthMultiplier,
            Damage = (int)(1 * _currentDifficulty.EnemyDamageMultiplier),
            AttackCooldown = GD.RandRange(1.5f, 3f),
            ProjectileSpeed = GD.RandRange(150f, 250f),
            TileSize = _tileMap.TileSet.TileSize.X,
        };

        enemy.Initialize(_player, stats, _tileMap);

        enemy.Death += OnEnemyDeath;
        enemy.GlobalPosition = GetCenteredPositionInRandomValidTile();
        AddChild(enemy);
        _enemies.Add(enemy);
        _enemiesSpawned++;

        GD.Print($"Enemigos spawnados: {_enemiesSpawned}/{_totalEnemiesForWave} | Activos: {_enemies.Count}");
    }

    public void ClearEnemies()
    {
        _spawnTimer.Stop();
        _waveActive = false;

        foreach (var enemy in _enemies)
        {
            if (IsInstanceValid(enemy))
            {
                enemy.Death -= OnEnemyDeath;
                enemy.QueueFree();
            }
        }
        _enemies.Clear();
        _enemiesSpawned = 0;
        _totalEnemiesForWave = 0;
    }
    private Vector2 GetCenteredPositionInRandomValidTile()
    {
        var usedRect = _tileMap.GetUsedRect();
        Vector2I tilePosition;
        int attempts = 0;
        Vector2I playerTile = _tileMap.LocalToMap(_player.GlobalPosition);

        do
        {
            tilePosition = new Vector2I(
                GD.RandRange(usedRect.Position.X, usedRect.End.X - 1),
                GD.RandRange(usedRect.Position.Y, usedRect.End.Y - 1)
            );
            attempts++;
        } while ((!IsTileValid(tilePosition) ||
                IsTooCloseToPlayer(tilePosition, playerTile)) &&
                attempts < 100);

        return CalculateCenteredPosition(tilePosition);
    }

    private Vector2 CalculateCenteredPosition(Vector2I tilePosition)
    {
        // 1. Obtener posición base del tile
        Vector2 tileWorldPos = _tileMap.MapToLocal(tilePosition);

        // 2. Obtener tamaño del tile
        Vector2 tileSize = _tileMap.TileSet.TileSize;

        // 3. Calcular posición centrada exacta
        Vector2 centeredPosition = tileWorldPos + (tileSize * 0.5f);

        // 4. Ajuste extra si los tiles son cuadrados (opcional)
        if (_tileMap.TileSet.TileShape == TileSet.TileShapeEnum.Square)
        {
            centeredPosition -= new Vector2(tileSize.X * 0.5f, tileSize.Y * 0.5f);
        }

        GD.Print($"Tile: {tilePosition} | WorldPos: {tileWorldPos} | Centered: {centeredPosition}");
        return centeredPosition;
    }

    private bool IsTileValid(Vector2I tilePos) =>
        _tileMap.GetCellSourceId(tilePos) != -1;

    private bool IsTooCloseToPlayer(Vector2I tilePos, Vector2I playerTile) =>
        playerTile.DistanceTo(tilePos) < MinDistanceToPlayer;

    public int GetEnemyCount()
    {
        // Primero limpia la lista de enemigos de cualquier null
        _enemies.RemoveAll(enemy => !IsInstanceValid(enemy));

        GD.Print($"Enemigos reales en lista: {_enemies.Count}");
        GD.Print($"Hijos directos del EnemyManager: {GetChildCount()}");

        return _enemies.Count;
    }
    
    public bool HasWaveCompleted()
    {
        bool completed = _enemiesSpawnedThisWave >= _totalEnemiesForWave &&
                        _enemiesDefeatedThisWave >= _totalEnemiesForWave &&
                        _enemies.Count == 0;

        GD.Print($"Verificando oleada completada: Generados={_enemiesSpawnedThisWave}/{_totalEnemiesForWave}, " +
                $"Derrotados={_enemiesDefeatedThisWave}/{_totalEnemiesForWave}, " +
                $"Activos={_enemies.Count}");

        return completed;
    }
}
