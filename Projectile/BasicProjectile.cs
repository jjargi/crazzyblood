using Godot;
using System;

// 1. Asegúrate que la clase parcial coincide exactamente con el tipo de nodo
public partial class BasicProjectile : BaseProjectile
{

    protected TileMapLayer TileMap;
    //[Export] public float MaxLifetime { get; set; } = 5f;

    private Vector2 _direction;
    //private Timer _lifetimeTimer;

    public void Initialize(Vector2 position, Vector2 direction, float baseDamage, float speed, TileMapLayer tileMap)
    {
        base.Initialize(position, direction, baseDamage, speed); // Usa el método base
        TileMap = tileMap;
        //SetupTimer();
    }
    public override void _PhysicsProcess(double delta)
    {
        // 5. Movimiento usando Position del nodo
        Position += Direction * Speed * (float)delta;

        if (TileMap != null)
        {
            Vector2I tileCoords = TileMap.LocalToMap(GlobalPosition);
            int sourceId = TileMap.GetCellSourceId(tileCoords);

            if (sourceId == -1)
            {
                GD.Print($"Misil colisiona con tile en {tileCoords}. Destruyendo.");
                QueueFree();
            }
        }


    }

    //private void SetupTimer()
    //{
    //    _lifetimeTimer = new Timer();
    //    AddChild(_lifetimeTimer);
    //    _lifetimeTimer.Timeout += OnTimerEnd;
    //    _lifetimeTimer.Start(MaxLifetime);
    //}

    //private void OnTimerEnd()
    //{
    //    QueueFree();
    //}

    // 6. Manejo de señales
    private bool _hasHit = false; // Añade este campo
    private void OnBodyEntered(Node2D body)
    {
        if (_hasHit) return; // Evita múltiples impactos
        _hasHit = true;// Marca como que ya impactó
        if (body.HasMethod("TakeDamage"))
        {
            body.Call("TakeDamage", FinalDamage);
        }
        QueueFree();
    }

    // 7. Conexión de señales en _Ready
    public override void _Ready()
    {
        base._Ready();
        DamageMultiplier = 1f; // Multiplicador básico (daño normal)
        BodyEntered += OnBodyEntered;
    }
}