using Godot;
using System;

public partial class Boss1 : Enemy
{
    // Configuración especial del boss
    [Export] private float _waveAmplitude = 128f;
    [Export] private float _waveFrequency = 8f;
    // Usamos la propiedad pública heredada, reutilizable para cualquier proyectil
    [Export] public override PackedScene ProjectileScene { get; set; }
    [Export] public PackedScene SecondaryProjectileScene { get; set; } // Nuevo ataque secundario

    private float _timeSinceLastAttack = 0f;
    private Vector2 _initialPosition;
    private bool _isMoving = false;

    // Evento para notificar cuando el jefe es derrotado
    public event Action<Boss1> BossDefeated;

    public override void _Ready()
    {
        base._Ready();
        // Escalar el sprite para que sea más grande
        _sprite.Scale = new Vector2(1.5f, 1.5f);

        // Configurar timers con intervalos diferentes para el boss
        _jumpTimer.WaitTime = JumpInterval * 1.5f; // Más lento que enemigos normales
        _attackTimer.WaitTime = AttackCooldown * 0.7f; // Ataca más frecuentemente
    }

    //protected override void OnJumpTimerTimeout()
    //{
    //    if (_isDying || _player == null || !IsInstanceValid(_player) || _isMoving) return;

    //    Vector2 diff = _player.GlobalPosition - GlobalPosition;
    //    Vector2 move = Vector2.Zero;

    //    // Movimiento de 2 celdas en la dirección del jugador
    //    if (Mathf.Abs(diff.X) > Mathf.Abs(diff.Y))
    //        move.X = Mathf.Sign(diff.X) * 2; // 2 celdas horizontal
    //    else
    //        move.Y = Mathf.Sign(diff.Y) * 2; // 2 celdas vertical

    //    _initialPosition = GlobalPosition;
    //    _isMoving = true;

    //    // Animación de movimiento suave
    //    var tween = GetTree().CreateTween();
    //    tween.TweenProperty(this, "global_position",
    //                      GlobalPosition + move * TileSize,
    //                      JumpInterval * 0.5f)
    //         .SetEase(Tween.EaseType.Out);

    //    tween.Finished += () => _isMoving = false;

    //    // Girar sprite según dirección
    //    _sprite.FlipH = move.X < 0;
    //}
    protected override void OnJumpTimerTimeout()
    {
        if (_isDying || _player == null || !IsInstanceValid(_player)) return;

        Vector2 diff = _player.GlobalPosition - GlobalPosition;
        Vector2 move = Vector2.Zero;

        // Movimiento de 2 celdas en la dirección del jugador
        if (Mathf.Abs(diff.X) > Mathf.Abs(diff.Y))
            move.X = Mathf.Sign(diff.X) * 2; // 2 celdas horizontal
        else
            move.Y = Mathf.Sign(diff.Y) * 2; // 2 celdas vertical

        // Teletransportación instantánea (como los enemigos normales)
        GlobalPosition += move * TileSize;

        // Girar sprite según dirección
        _sprite.FlipH = move.X < 0;
    }
    protected override void OnAttackTimerTimeout()
    {
        if (_isDying || _player == null || !IsInstanceValid(_player)) return;

        //// Disparar proyectil sinusoidal
        //ShootSinusoidalProjectile();

        //// 30% de probabilidad de disparar un segundo proyectil
        if (GD.Randf() < 0.3f)
        {
            GetTree().CreateTimer(0.3f).Timeout += ShootSinusoidalProjectile;
        }
        //// 10% de probabilidad de disparar un segundo proyectil
        if  (GD.Randf() < 0.1f)
        {
            GetTree().CreateTimer(0.2f).Timeout += ShootSecondaryProjectile;
        }
    }

    private void ShootSinusoidalProjectile()
    {
        var projectile = ProjectileScene.Instantiate<SinusoidalProjectile>();
        GetParent().AddChild(projectile);

        Vector2 direction = (_player.GlobalPosition - GlobalPosition).Normalized();

        projectile.Initialize(
            startPosition: GlobalPosition,
            direction: direction,
            speed: ProjectileSpeed * 0.8f, // Más lento pero con patrón
            damage: Damage * 2, // Doble daño
            tileMap: _tileMap,
            amplitude: _waveAmplitude,
            frequency: _waveFrequency
        );
    }
    private void ShootSecondaryProjectile()
    {
        if (SecondaryProjectileScene is not PackedScene scene) return;

        var projectile = scene.Instantiate<EnemyProjectile>();
        GetParent().AddChild(projectile);

        Vector2 direction = (_player.GlobalPosition - GlobalPosition).Normalized();

        projectile.Initialize(
            position: GlobalPosition,
            direction: direction,
            speed: ProjectileSpeed * 0.5f, // Más lento que el principal
            damage: Damage,
            tileMap: _tileMap
        );
    }
    public override void TakeDamage(int damage)
    {
        // El boss recibe menos daño (opcional)
        base.TakeDamage((int)(damage));

        // Efecto visual adicional
        if (_currentHealth > 0)
        {
            var tween = GetTree().CreateTween();
            tween.TweenProperty(_sprite, "scale", new Vector2(1.7f, 1.7f), 0.1f);
            tween.TweenProperty(_sprite, "scale", new Vector2(1.5f, 1.5f), 0.1f);
        }
        else
        {
            // Notificar que el jefe fue derrotado
            BossDefeated?.Invoke(this);
        }
    }
}