
using Godot;
using System;

public partial class Enemy : CharacterBody2D
{
    // Configuración exportable
    [Export] public int MaxHealth = 4;
    [Export] public int Damage = 1;
    [Export] public float JumpInterval = 2f;
    [Export] public float TileSize = 64f;
    [Export] public float AttackCooldown = 3f;
    [Export] public float ProjectileSpeed = 200f;
    [Export] public PackedScene ProjectileScene;

    // Nodos
    private AnimatedSprite2D _sprite;
    private HBoxContainer _healthContainer;
    private Timer _jumpTimer;
    private Timer _attackTimer;

    // Estado
    private Player _player;
    private int _currentHealth;
    private bool _isDying = false;
    private bool _isTakingDamage = false;
    private ColorRect[] _lifeRects;
    private TileMapLayer _tileMap;
    public event Action<Enemy> Death;


    public void Initialize(Player player, EnemyStats stats, TileMapLayer tileMap)
    {
        _player = player;
        MaxHealth = stats.Health;
        Damage = stats.Damage;
        JumpInterval = (float)stats.Speed;
        AttackCooldown = (float)stats.AttackCooldown;
        //_stats = stats;
        _currentHealth = stats.Health;
        TileSize = stats.TileSize; // Recibimos el tamaño del tile
        _tileMap = tileMap;
    }
    public override void _Ready()
    {
        _sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        _healthContainer = GetNode<HBoxContainer>("HealthContainer");
        _sprite.AnimationFinished += OnAnimationFinished;
        // Configurar barra de vida
        SetupHealthBar();

        // Configurar timers
        SetupTimers();

        _sprite.Play("default");
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

    private void SetupTimers()
    {
        // Timer de movimiento
        _jumpTimer = new Timer
        {
            WaitTime = JumpInterval,
            OneShot = false
        };
        AddChild(_jumpTimer);
        _jumpTimer.Timeout += OnJumpTimerTimeout;
        _jumpTimer.Start();

        // Timer de ataque
        _attackTimer = new Timer
        {
            WaitTime = AttackCooldown,
            OneShot = false
        };
        AddChild(_attackTimer);
        _attackTimer.Timeout += OnAttackTimerTimeout;
        _attackTimer.Start();
    }

    private void OnJumpTimerTimeout()
    {
        if (_isDying || _player == null || !IsInstanceValid(_player)) return;

        Vector2 diff = _player.GlobalPosition - GlobalPosition;

        // Determinar dirección cardinal (priorizando el eje con mayor diferencia)
        Vector2 move = Vector2.Zero;
        if (Mathf.Abs(diff.X) > Mathf.Abs(diff.Y))
            move.X = Mathf.Sign(diff.X); // Movimiento horizontal
        else
            move.Y = Mathf.Sign(diff.Y); // Movimiento vertical

        // Mover exactamente 1 celda
        GlobalPosition += move * TileSize;

        // Actualizar dirección del sprite
        _sprite.FlipH = move.X < 0;
    }

    private void OnAttackTimerTimeout()
    {
        if (_isDying || _player == null || !IsInstanceValid(_player)) return;

        ShootAtPlayer();
    }

    private void ShootAtPlayer()
    {
        var projectile = ProjectileScene.Instantiate<EnemyProjectile>();
        GetParent().AddChild(projectile);
        projectile.GlobalPosition = GlobalPosition;//super importante para que dispare en la posicion del player
        Vector2 direction = (_player.GlobalPosition - GlobalPosition).Normalized();
        projectile.Initialize(
            GlobalPosition,
            direction,
            ProjectileSpeed,
            Damage,
            _tileMap

        );
    }

    public void TakeDamage(int damage)
    {
        if (_isDying || _isTakingDamage) return;

        _isTakingDamage = true;
        _currentHealth -= damage;
        UpdateHealthBar();

        // Efecto de daño
        _sprite.Modulate = Colors.Red;
        GetTree().CreateTimer(0.1).Timeout += () => _sprite.Modulate = Colors.White;

        // Reiniciar cooldown de daño
        GetTree().CreateTimer(0.5).Timeout += () => _isTakingDamage = false;

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    private void UpdateHealthBar()
    {
        for (int i = 0; i < MaxHealth; i++)
        {
            _lifeRects[i].Color = i < _currentHealth ? Colors.Green : Colors.Red;
        }
    }

    public async void Die()
    {
        if (_isDying) return;
        _isDying = true;

        _sprite.Play("explosion");
        GetNode<CollisionShape2D>("CollisionShape2D").SetDeferred("disabled", true);

        // Asegúrate de emitir el evento ANTES de QueueFree
        Death?.Invoke(this);

        // Opcional: Esperar a que termine la animación
        await ToSignal(_sprite, "animation_finished");
        QueueFree();
    }

    private void OnAnimationFinished()
    {
        //if (_sprite.Animation == "explosion")
        //{
        //    Death?.Invoke(this);
        //    QueueFree();
        //}
    }
}