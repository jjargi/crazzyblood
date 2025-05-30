
using Godot;
using System.Collections.Generic;

public partial class GameBoard : Node2D
{
    [Export] private PackedScene[] _mapScenes;
    [Export] private int _enemiesPerWave = 5;
    [Export] private int _wavesPerLevel = 2;

    private TileMapLayer _currentMap;
    private Player _player;
    private EnemyManager _enemyManager;
    private int _currentWave = 0;
    private int _currentLevel = 1;
    private int _enemiesToDefeatInWave;
    private int _totalEnemiesDefeatedInWave = 0;

    private Timer _levelDelayTimer;
    private Label _levelTransitionLabel;
    private double _levelStartTime; // Añade esta variable al inicio con las otras variables de clase

    public override void _Ready()
    {
        _player = GetNode<Player>("Player");
        _enemyManager = GetNode<EnemyManager>("EnemyManager");

        // Crear e inicializar Timer
        _levelDelayTimer = new Timer
        {
            OneShot = true,
            WaitTime = 4.0f
        };
        AddChild(_levelDelayTimer);
        _levelDelayTimer.Timeout += OnLevelDelayFinished;

        // Crear e inicializar Label para el mensaje de transición
        _levelTransitionLabel = new Label
        {
            Text = "",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill,
            Visible = false
        };
        var canvas = new CanvasLayer();
        canvas.AddChild(_levelTransitionLabel);
        AddChild(canvas);

        InitializeLevel();
    }

    private void InitializeLevel()
    {
        // Ocultar mensaje de transición
        _levelTransitionLabel.Visible = false;

        // Cargar mapa correspondiente al nivel
        LoadMapForLevel();

        // Configurar dificultad
        var difficultySettings = GetDifficultySettings();

        // Reiniciar contadores de oleada
        _currentWave = 0;
        _totalEnemiesDefeatedInWave = 0;

        // Iniciar primera oleada
        StartNextWave();
    }

    private void LoadMapForLevel()
    {
        if (_currentMap != null)
        {
            _currentMap.QueueFree();
        }

        int mapIndex = (_currentLevel - 1) % _mapScenes.Length;
        _currentMap = _mapScenes[mapIndex].Instantiate<TileMapLayer>();
        AddChild(_currentMap);
        MoveChild(_currentMap, 0);

        _player.SetTileMap(_currentMap);
        _enemyManager.Reinitialize(_player, _currentMap);
        _player._weaponSystem?.SetTileMap(_currentMap);

        PositionPlayerOnMap();
    }

    private void PositionPlayerOnMap()
    {
        var usedCells = _currentMap.GetUsedCells();
        if (usedCells.Count > 0)
        {
            var startPos = _currentMap.MapToLocal(usedCells[0]) + _currentMap.TileSet.TileSize / 2;
            _player.GlobalPosition = startPos;
        }
    }

    private DifficultySettings GetDifficultySettings()
    {
        return new DifficultySettings
        {
            EnemyHealthMultiplier = _currentLevel * 1f,
            EnemySpeedMultiplier = Mathf.Max(0.1f, 4 - (_currentLevel * 0.5f)),
            SpawnRateMultiplier = Mathf.Max(0.5f, 2 - (_currentLevel * 0.5f)),
            EnemyDamageMultiplier = _currentLevel,
            EnemyAtackCooldownMultiplier = Mathf.Max(0.5f, 4 - (_currentLevel * 0.5f)),
        };
    }

    private void StartNextWave()
    {
        _currentWave++;
        _enemiesToDefeatInWave = _enemiesPerWave;
        _totalEnemiesDefeatedInWave = 0;
        bool isLastWave = _currentWave >= _wavesPerLevel;

        GD.Print($"Iniciando oleada {_currentWave} ({(isLastWave ? "ÚLTIMA" : "Normal")})");

        var difficulty = GetDifficultySettings();
        _enemyManager.StartWave(_enemiesPerWave, difficulty, isLastWave);

        _enemyManager.EnemyDefeated -= OnEnemyDefeatedInWave;
        _enemyManager.EnemyDefeated += OnEnemyDefeatedInWave;
    }

    private void OnEnemyDefeatedInWave()
    {
        _totalEnemiesDefeatedInWave++;
        int remainingEnemies = _enemyManager.GetEnemyCount();

        GD.Print($"Enemigo derrotado. Total derrotados: {_totalEnemiesDefeatedInWave}/{_enemiesPerWave}. Restantes: {remainingEnemies}");

        if (_totalEnemiesDefeatedInWave >= _enemiesPerWave && remainingEnemies == 0)
        {
            GD.Print($"¡Oleada {_currentWave} completada!");

            if (_currentWave >= _wavesPerLevel)
            {
                AdvanceToNextLevel();
            }
            else
            {
                StartNextWave();
            }
        }
    }


    private void AdvanceToNextLevel()
    {
        // Calcular valores
        double levelTime = (Time.GetTicksMsec() - _levelStartTime) / 1000.0;
        string difficulty = GetDifficultyName(_currentLevel + 1);

        // Crear lista de enemigos
        var nextEnemies = new List<EnemyData>
    {
        new EnemyData {
            Type = "Zombie",
            Count = 10 + _currentLevel,
            Health = 100 * (1 + _currentLevel * 0.2f),
            Damage = 20 * (1 + _currentLevel * 0.15f)
        },
        new EnemyData {
            Type = "Araña",
            Count = 5 + _currentLevel/2,
            Health = 60 * (1 + _currentLevel * 0.15f),
            Damage = 30 * (1 + _currentLevel * 0.1f)
        }
    };

        // Mostrar transición (delegamos el timing a LevelTransition)
        var transition = GD.Load<PackedScene>("res://LevelTransition.tscn").Instantiate<LevelTransition>();
        GetTree().Root.AddChild(transition);
        transition.ShowTransition(
            currentLevel: _currentLevel,
            nextLevel: _currentLevel + 1,
            levelTime: (float)levelTime,
            difficulty: difficulty,
            enemies: nextEnemies,
            onComplete: () => {
                _currentLevel++;
                _enemyManager.ClearEnemies();
                InitializeLevel();
            }
        );
    }
    private void OnLevelDelayFinished()
    {
        InitializeLevel();
    }

    public void OnPlayerDefeated()
    {
        GD.Print("¡Jugador derrotado!");

        _currentLevel = 1;

        //EffectManager.Instance?.ClearByType("enemy");
        _enemyManager.ClearEnemies();
        InitializeLevel();
        _player.ResetPlayer();
    }
    // Añade este método a tu clase GameBoard
    private string GetDifficultyName(int level)
    {
        if (level < 3) return "Fácil";
        if (level < 6) return "Media";
        if (level < 9) return "Difícil";
        return "Extrema";
    }
}

public struct DifficultySettings
{
    public float EnemyHealthMultiplier;
    public float EnemySpeedMultiplier;
    public float SpawnRateMultiplier;
    public float EnemyDamageMultiplier;
    public float EnemyAtackCooldownMultiplier;
}

