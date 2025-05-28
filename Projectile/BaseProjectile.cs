using Godot;


public partial class BaseProjectile : Area2D
{
    [Export] public float Speed { get; set; } = 300f;
    [Export] public float DamageMultiplier = 1f;
    public float FinalDamage { get; protected set; }

    protected Vector2 Direction;
    protected Timer LifetimeTimer;

    // Asegurarse que los parámetros coincidan con lo que se usa en WeaponSystem
    public virtual void Initialize(Vector2 position, Vector2 direction, float baseDamage, float speed)
    {
        Position = position;
        Direction = direction.Normalized();
        FinalDamage = baseDamage * DamageMultiplier;
        Speed = speed;
        Rotation = direction.Angle();
    }


    public override void _PhysicsProcess(double delta)
    {
        Position += Direction * Speed * (float)delta;
    }

    protected virtual void OnLifetimeEnd()
    {
        QueueFree();
    }

    protected virtual void OnBodyEntered(Node2D body)
    {
        // Lógica de impacto básica
        HandleImpact(body);
        QueueFree();
    }

    // Método base que puede ser sobrescrito
    protected virtual void HandleImpact(Node body)
    {
        if (body.HasMethod("TakeDamage"))
        {
            body.Call("TakeDamage", FinalDamage); // Usa el daño calculado
        }
    }
    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
    }
}