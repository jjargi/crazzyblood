using Godot;
using System;
using static Godot.TextServer;

public partial class Player : CharacterBody2D
{
    private GameBoard _gameBoard;
    private PlayerMovement _movement;
    public WeaponSystem _weaponSystem;
    private HBoxContainer _healthContainer;
    private AnimatedSprite2D _sprite;
    private ColorRect[] _lifeRects;
    private int _currentHealth;
    private bool _isTakingDamage = false;
    public bool _isDying = false;
    public TileMapLayer _TileMapLayer;

    [Export] public int MaxHealth = 5;
    //public WeaponSystem WeaponSystem { get; private set; }
    public void SetTileMap(TileMapLayer tileMap)
    {
        _TileMapLayer = tileMap;
        if (_movement != null)
        {
            _movement.SetTileMap(tileMap);
        }
    }

    public override void _Ready()
    {
        _gameBoard = GetParent<GameBoard>();
        _movement = GetNode<PlayerMovement>("MovementSystem");
        _weaponSystem = GetNode<WeaponSystem>("WeaponSystem");

        // Inicializar movimiento primero
        _movement.Initialize(this);

        // El TileMap se asignará después desde GameBoard
        var enemyManager = GetNode<EnemyManager>("../EnemyManager");
        enemyManager.Initialize(this, _TileMapLayer);


        _gameBoard = GetParent<GameBoard>();
        //_TileMapLayer = GetNode<TileMapLayer>("../TileMapLayer");
        _movement = GetNode<PlayerMovement>("MovementSystem");
        _movement.Initialize(this);
        _weaponSystem = GetNode<WeaponSystem>("WeaponSystem");


        _healthContainer = GetNode<HBoxContainer>("HealthContainer");
        _sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        // Configurar barra de vida
        _currentHealth = MaxHealth;
        SetupHealthBar();
        // Configurar en la CharacterBody2D (no en el CollisionShape)
        SetCollisionLayerValue(1, true); // Asignar a capa 1 (jugador)
        // No necesita máscara a menos que deba detectar algo
    }

    public override void _PhysicsProcess(double delta)
    {
        _movement.HandleMovement(delta);
        if (Input.IsActionJustPressed("fire_primary"))
        {
            // Ahora pasamos dos parámetros: posición del jugador y posición del objetivo
            _weaponSystem.Shoot(GlobalPosition, GetGlobalMousePosition());
        }

        if (Input.IsActionJustPressed("weapon_1"))
        {
            _weaponSystem.SwitchWeapon("BasicWeapon");
        }
        else if (Input.IsActionJustPressed("weapon_2"))
        {
            _weaponSystem.SwitchWeapon("ExplosiveWeapon");
        }
        else if (Input.IsActionJustPressed("weapon_3"))
        {
            _weaponSystem.SwitchWeapon("NuclearWeapon");

        }
    }

    public void TakeDamage(int damage)
    {
        if (_isDying || _isTakingDamage) return;

        _isTakingDamage = true;
        _currentHealth = Math.Max(_currentHealth - damage, 0);
        UpdateHealthBar();

        // Efecto visual de daño
        _sprite.Modulate = Colors.Red;
        GetTree().CreateTimer(0.1).Timeout += () => _sprite.Modulate = Colors.White;

        // Cooldown de daño
        GetTree().CreateTimer(0.5).Timeout += () => _isTakingDamage = false;

        GD.Print($"Player health: {_currentHealth}/{MaxHealth}");

        if (_currentHealth <= 0)
        {
            Die();
        }
    }
    private void SetupHealthBar()
    {
        // Limpiar hijos existentes
        foreach (Node child in _healthContainer.GetChildren())
        {
            child.QueueFree();
        }
        // Configurar el contenedor para centrado automático
        //_healthContainer.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
        //_healthContainer.Alignment = BoxContainer.AlignmentMode.Center;
        // Crear nuevos indicadores de vida
        _lifeRects = new ColorRect[MaxHealth];
        for (int i = 0; i < MaxHealth; i++)
        {
            var lifeRect = new ColorRect
            {
                Color = Colors.Green,
                CustomMinimumSize = new Vector2(10, 10),
                SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter
            };
            _healthContainer.AddChild(lifeRect);
            _lifeRects[i] = lifeRect;
        }
        UpdateHealthBar();
    }
    private void UpdateHealthBar()
    {
        for (int i = 0; i < MaxHealth; i++)
        {
            _lifeRects[i].Color = i < _currentHealth ? Colors.Green : Colors.Red;
        }
    }
    private async void Die()
    {
        _isDying = true;
        _sprite.Play("death_animation");
        // Opcional: Deshabilitar colisiones
        GetNode<CollisionShape2D>("CollisionShape2D").SetDeferred("disabled", true);
        // Esperar a que termine la animación antes de reiniciar
        await ToSignal(_sprite, "animation_finished");

        // Reiniciar la escena principal
        //GetTree().ReloadCurrentScene();
        _gameBoard.OnPlayerDefeated();

    }
    public void ResetPlayer()
    {
        _currentHealth = MaxHealth;
        _isDying = false;
        _isTakingDamage = false;
        UpdateHealthBar();
        GetNode<CollisionShape2D>("CollisionShape2D").Disabled = false;
        _sprite.Play("default");
        _sprite.Modulate = Colors.White;

        // Reposicionar al jugador
        //PositionPlayerOnMap();
    }
}