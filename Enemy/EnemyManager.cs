using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class EnemyManager : Node2D
{
    // Campos exportados
    [Export] public PackedScene EnemyScene { get; set; }
    [Export] public int MaxEnemies = 10;
    [Export] public float SpawnInterval = 2.0f;
    [Export] public float MinDistanceToPlayer = 5f;
    [Export] private PackedScene[] _bossScenes;

    // Propiedades públicas
    public PackedScene[] BossScenes => _bossScenes;

    // Campos privados
    private Player _player;
    private TileMapLayer _tileMap;
    private Timer _spawnTimer;
    private DifficultySettings _currentDifficulty;
    private List<Enemy> _enemies = new List<Enemy>();
    private bool _bossSpawned = false;
    private int _currentBossIndex = 0;

    // Variables de control de oleadas
    private int _enemiesSpawnedThisWave = 0;
    private int _totalEnemiesForWave = 0;
    private int _enemiesDefeatedThisWave = 0;
    private bool _isLastWave = false;
    private bool _waveActive = false;

    // Eventos
    public event Action EnemyDefeated;

    public void Initialize(Player player, TileMapLayer tileMap)
    {
        _player = player;
        _tileMap = tileMap;

        _spawnTimer = new Timer();
        AddChild(_spawnTimer);
        _spawnTimer.Timeout += OnSpawnTimerTimeout;
    }

    public void Reinitialize(Player player, TileMapLayer tileMap)
    {
        ClearEnemies();
        Initialize(player, tileMap);
    }

    public void StartWave(int enemyCount, DifficultySettings difficulty, bool isLastWave = false)
    {
        ClearEnemies();
        _bossSpawned = false;

        _totalEnemiesForWave = enemyCount;
        _enemiesSpawnedThisWave = 0;
        _enemiesDefeatedThisWave = 0;
        _waveActive = true;
        _currentDifficulty = difficulty;
        _isLastWave = isLastWave;

        GD.Print($"Iniciando oleada {(isLastWave ? "FINAL" : "")}. Enemigos: {enemyCount}");

        _spawnTimer.WaitTime = SpawnInterval * difficulty.SpawnRateMultiplier;
        _spawnTimer.Start();
    }

    private void OnSpawnTimerTimeout()
    {
        if (_enemiesSpawnedThisWave >= _totalEnemiesForWave)
        {
            _spawnTimer.Stop();
            _waveActive = false;

            if (_isLastWave && _enemies.Count == 0 && !_bossSpawned)
            {
                SpawnBoss(_currentBossIndex);
            }
            return;
        }

        if (_enemies.Count >= MaxEnemies)
            return;

        SpawnEnemy();
    }

    private void SpawnEnemy()
    {
        var enemy = EnemyScene.Instantiate<Enemy>();
        var stats = new EnemyStats
        {
            Speed = (int)_currentDifficulty.EnemySpeedMultiplier,
            Health = (int)_currentDifficulty.EnemyHealthMultiplier,
            Damage = (int)_currentDifficulty.EnemyDamageMultiplier,
            AttackCooldown = _currentDifficulty.EnemyAtackCooldownMultiplier,
            ProjectileSpeed = GD.RandRange(150f, 250f),
            TileSize = (int)_tileMap.TileSet.TileSize.X,
        };

        enemy.Initialize(_player, stats, _tileMap);
        enemy.Death += OnEnemyDeath;
        enemy.GlobalPosition = GetCenteredPositionInRandomValidTile();

        AddChild(enemy);
        _enemies.Add(enemy);
        _enemiesSpawnedThisWave++;
    }

    private void OnEnemyDeath(Enemy enemy)
    {
        if (_enemies.Contains(enemy))
        {
            _enemies.Remove(enemy);
            _enemiesDefeatedThisWave++;
            EnemyDefeated?.Invoke();

            GD.Print($"Enemigo derrotado: {_enemiesDefeatedThisWave}/{_totalEnemiesForWave}");

            if (_enemiesDefeatedThisWave >= _totalEnemiesForWave && _enemies.Count == 0)
            {
                if (_isLastWave && !_bossSpawned)
                {
                    SpawnBoss(_currentBossIndex);
                }
            }
        }
    }

    private void SpawnBoss(int bossIndex)
    {
        if (_bossScenes == null || _bossScenes.Length == 0)
        {
            GD.PrintErr("No hay escenas de jefe configuradas!");
            return;
        }

        _currentBossIndex = bossIndex % _bossScenes.Length;
        var boss = _bossScenes[_currentBossIndex].Instantiate<Enemy>();

        var stats = new EnemyStats
        {
            Speed = (int)(_currentDifficulty.EnemySpeedMultiplier * 0.7f),
            Health = (int)(_currentDifficulty.EnemyHealthMultiplier * 5f),
            Damage = (int)(_currentDifficulty.EnemyDamageMultiplier * 2f),
            AttackCooldown = _currentDifficulty.EnemyAtackCooldownMultiplier * 0.8f,
            ProjectileSpeed = GD.RandRange(200f, 300f),
            TileSize = (int)_tileMap.TileSet.TileSize.X,
        };

        boss.Initialize(_player, stats, _tileMap);
        boss.Death += OnEnemyDeath;
        boss.GlobalPosition = GetCenteredPositionInRandomValidTile();
        boss.Scale = new Vector2(1.5f, 1.5f);

        AddChild(boss);
        _enemies.Add(boss);
        _bossSpawned = true;

        GD.Print($"¡JEFE {_currentBossIndex} HA APARECIDO!");
    }

    public bool HasActiveBoss()
    {
        return _bossSpawned && _enemies.Any(e => IsInstanceValid(e));
    }

    public bool IsBossDefeated()
    {
        return _bossSpawned && !_enemies.Any(e => IsInstanceValid(e));
    }

    public void ClearEnemies()
    {
        _spawnTimer.Stop();
        _waveActive = false;

        foreach (var enemy in _enemies.ToList())
        {
            if (IsInstanceValid(enemy))
            {
                enemy.Death -= OnEnemyDeath;
                enemy.QueueFree();
            }
        }
        _enemies.Clear();
        _enemiesSpawnedThisWave = 0;
        _totalEnemiesForWave = 0;
        _enemiesDefeatedThisWave = 0;
    }

    public int GetEnemyCount()
    {
        _enemies.RemoveAll(enemy => !IsInstanceValid(enemy));
        return _enemies.Count;
    }

    public bool HasWaveCompleted()
    {
        return _enemiesSpawnedThisWave >= _totalEnemiesForWave &&
               _enemiesDefeatedThisWave >= _totalEnemiesForWave &&
               _enemies.Count == 0;
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

        // Conversión explícita para evitar error de multiplicación
        Vector2 tileSize = _tileMap.TileSet.TileSize;
        return _tileMap.MapToLocal(tilePosition) + (tileSize * 0.5f);
    }

    private bool IsTileValid(Vector2I tilePos) =>
        _tileMap.GetCellSourceId(tilePos) != -1;

    private bool IsTooCloseToPlayer(Vector2I tilePos, Vector2I playerTile) =>
        playerTile.DistanceTo(tilePos) < MinDistanceToPlayer;

    public void SetCurrentMapIndex(int mapIndex)
    {
        _currentBossIndex = mapIndex;
    }
}