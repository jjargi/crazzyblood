
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

    public override void _Ready()
    {
        _player = GetNode<Player>("Player");
        _enemyManager = GetNode<EnemyManager>("EnemyManager");

        // Crear e inicializar Timer
        _levelDelayTimer = new Timer
        {
            OneShot = true,
            WaitTime = 3.0f
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
            //EnemyHealthMultiplier = 1 + (_currentLevel * 0.2f),
            EnemyHealthMultiplier = 1 + (_currentLevel * 1f),
            EnemySpeedMultiplier = 1 + (_currentLevel * 0.1f),
            SpawnRateMultiplier = Mathf.Max(0.5f, 1 - (_currentLevel * 0.05f)),
            EnemyDamageMultiplier = 1 + (_currentLevel * 0.15f)
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
        _currentLevel++;
        GD.Print($"¡Nivel {_currentLevel - 1} completado! Avanzando al nivel {_currentLevel} en 5 segundos...");

        // Mostrar mensaje de transición
        _levelTransitionLabel.Text = $"Nivel {_currentLevel - 1} completado.\nAvanzando al nivel {_currentLevel}...";
        _levelTransitionLabel.Visible = true;

        // Limpiar antes del retardo
        //EffectManager.Instance?.ClearAllEffects(); // Asegúrate de que EffectManager esté correctamente inicializado
        _enemyManager.ClearEnemies();

        // Iniciar cuenta regresiva
        _levelDelayTimer.Start();
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
}

public struct DifficultySettings
{
    public float EnemyHealthMultiplier;
    public float EnemySpeedMultiplier;
    public float SpawnRateMultiplier;
    public float EnemyDamageMultiplier;
}
//using Godot;
//using System.Collections.Generic;

//public partial class GameBoard : Node2D
//{
//    [Export] private PackedScene[] _mapScenes;
//    [Export] private int _enemiesPerWave = 5;
//    [Export] private int _wavesPerLevel = 2;

//    private TileMapLayer _currentMap;
//    private Player _player;
//    private EnemyManager _enemyManager;
//    private int _currentWave = 0;
//    private int _currentLevel = 1;
//    private int _enemiesToDefeatInWave;
//    private int _totalEnemiesDefeatedInWave = 0;

//    private Timer _levelDelayTimer;
//    private Panel _transitionPanel;
//    private RichTextLabel _levelTransitionLabel;

//    public override void _Ready()
//    {
//        _player = GetNode<Player>("Player");
//        _enemyManager = GetNode<EnemyManager>("EnemyManager");

//        // Crear e inicializar Timer
//        _levelDelayTimer = new Timer
//        {
//            OneShot = true,
//            WaitTime = 3.0f
//        };
//        AddChild(_levelDelayTimer);
//        _levelDelayTimer.Timeout += OnLevelDelayFinished;

//        // Crear Panel de transición
//        _transitionPanel = new Panel
//        {
//            Name = "TransitionPanel",
//            Visible = false,
//            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
//            SizeFlagsVertical = Control.SizeFlags.ExpandFill
//        };

//        _transitionPanel.AddThemeStyleboxOverride("panel", new StyleBoxFlat
//        {
//            BgColor = new Color(0, 0, 0, 0.75f), // fondo negro semitransparente
//            BorderColor = new Color(1, 1, 1), // borde blanco
//            BorderWidthLeft = 2,
//            BorderWidthTop = 2,
//            BorderWidthRight = 2,
//            BorderWidthBottom = 2,
//            CornerRadiusTopLeft = 8,
//            CornerRadiusTopRight = 8,
//            CornerRadiusBottomLeft = 8,
//            CornerRadiusBottomRight = 8
//        });

//        _levelTransitionLabel = new RichTextLabel
//        {
//            Text = "",
//            HorizontalAlignment = HorizontalAlignment.Center,
//            VerticalAlignment = VerticalAlignment.Center,
//            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
//            SizeFlagsVertical = Control.SizeFlags.ExpandFill,
//            CustomMinimumSize = new Vector2(400, 200),
//            Visible = false
//        };
//        _levelTransitionLabel.Set("autowrap_mode", 1); // Word wrap (equivale a TextServer.AutowrapMode.Word)

//        _levelTransitionLabel.AddThemeFontSizeOverride("font_size", 24);
//        _transitionPanel.AddChild(_levelTransitionLabel);

//        var canvas = new CanvasLayer { Layer = 10 };
//        canvas.AddChild(_transitionPanel);
//        AddChild(canvas);

//        InitializeLevel();
//    }

//    private void InitializeLevel()
//    {
//        _transitionPanel.Visible = false;

//        LoadMapForLevel();

//        var difficultySettings = GetDifficultySettings();

//        _currentWave = 0;
//        _totalEnemiesDefeatedInWave = 0;

//        StartNextWave();
//    }

//    private void LoadMapForLevel()
//    {
//        if (_currentMap != null)
//        {
//            _currentMap.QueueFree();
//        }

//        int mapIndex = (_currentLevel - 1) % _mapScenes.Length;
//        _currentMap = _mapScenes[mapIndex].Instantiate<TileMapLayer>();
//        AddChild(_currentMap);
//        MoveChild(_currentMap, 0);

//        _player.SetTileMap(_currentMap);
//        _enemyManager.Reinitialize(_player, _currentMap);
//        _player._weaponSystem?.SetTileMap(_currentMap);

//        PositionPlayerOnMap();
//    }

//    private void PositionPlayerOnMap()
//    {
//        var usedCells = _currentMap.GetUsedCells();
//        if (usedCells.Count > 0)
//        {
//            var startPos = _currentMap.MapToLocal(usedCells[0]) + _currentMap.TileSet.TileSize / 2;
//            _player.GlobalPosition = startPos;
//        }
//    }

//    private DifficultySettings GetDifficultySettings()
//    {
//        return new DifficultySettings
//        {
//            EnemyHealthMultiplier = 1 + (_currentLevel * 1f),
//            EnemySpeedMultiplier = 1 + (_currentLevel * 0.1f),
//            SpawnRateMultiplier = Mathf.Max(0.5f, 1 - (_currentLevel * 0.05f)),
//            EnemyDamageMultiplier = 1 + (_currentLevel * 0.15f)
//        };
//    }

//    private void StartNextWave()
//    {
//        _currentWave++;
//        _enemiesToDefeatInWave = _enemiesPerWave;
//        _totalEnemiesDefeatedInWave = 0;
//        bool isLastWave = _currentWave >= _wavesPerLevel;

//        GD.Print($"Iniciando oleada {_currentWave} ({(isLastWave ? "ÚLTIMA" : "Normal")})");

//        var difficulty = GetDifficultySettings();
//        _enemyManager.StartWave(_enemiesPerWave, difficulty, isLastWave);

//        _enemyManager.EnemyDefeated -= OnEnemyDefeatedInWave;
//        _enemyManager.EnemyDefeated += OnEnemyDefeatedInWave;
//    }

//    private void OnEnemyDefeatedInWave()
//    {
//        _totalEnemiesDefeatedInWave++;
//        int remainingEnemies = _enemyManager.GetEnemyCount();

//        GD.Print($"Enemigo derrotado. Total derrotados: {_totalEnemiesDefeatedInWave}/{_enemiesPerWave}. Restantes: {remainingEnemies}");

//        if (_totalEnemiesDefeatedInWave >= _enemiesPerWave && remainingEnemies == 0)
//        {
//            GD.Print($"¡Oleada {_currentWave} completada!");

//            if (_currentWave >= _wavesPerLevel)
//            {
//                AdvanceToNextLevel();
//            }
//            else
//            {
//                StartNextWave();
//            }
//        }
//    }

//    private void AdvanceToNextLevel()
//    {
//        _currentLevel++;
//        GD.Print($"¡Nivel {_currentLevel - 1} completado! Avanzando al nivel {_currentLevel} en 3 segundos...");

//        // Mostrar mensaje
//        _levelTransitionLabel.Text = $"Nivel {_currentLevel - 1} completado.\nAvanzando al nivel {_currentLevel}...";
//        _transitionPanel.Visible = true;

//        _enemyManager.ClearEnemies();

//        _levelDelayTimer.Start();
//    }

//    private void OnLevelDelayFinished()
//    {
//        InitializeLevel();
//    }

//    public void OnPlayerDefeated()
//    {
//        GD.Print("¡Jugador derrotado!");

//        _currentLevel = 1;
//        _enemyManager.ClearEnemies();
//        InitializeLevel();
//        _player.ResetPlayer();
//    }
//}

//public struct DifficultySettings
//{
//    public float EnemyHealthMultiplier;
//    public float EnemySpeedMultiplier;
//    public float SpawnRateMultiplier;
//    public float EnemyDamageMultiplier;
//}
