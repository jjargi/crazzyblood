
using Godot;

public partial class EnemyProjectile : Area2D
{
    private Vector2 _direction;
    private double _speed;
    private double _damage;

    protected TileMapLayer _tileMap;

    public void Initialize(Vector2 position, Vector2 direction, float speed, int damage, TileMapLayer tileMap)
    {
        Position = position; // <-- Esta línea es clave
        _direction = direction.Normalized();
        _speed = speed;
        _damage = damage;
        _tileMap = tileMap;
        Rotation = _direction.Angle();
    }
    public override void _Ready()
    {
        // Solo detectará objetos en la capa "Players" (Layer 1)
        CollisionLayer = 0; // No necesita estar en ninguna capa
        SetCollisionMaskValue(1, true);  // Solo detectará capa 1 (Players)
        // Conectar señal manualmente si no está conectada en el editor
        BodyEntered += OnBodyEntered;
    }
    public override void _PhysicsProcess(double delta)
    {

        Position += _direction * ((float)_speed) * (float)delta;
        if (_tileMap != null)
        {
            Vector2I tileCoords = _tileMap.LocalToMap(GlobalPosition);
            int sourceId = _tileMap.GetCellSourceId(tileCoords);

            if (sourceId == -1)
            {
                GD.Print($"Misil colisiona con tile en {tileCoords}. Destruyendo.");
                QueueFree();
            }
        }


    }

    private void OnBodyEntered(Node2D body)
    {
        GD.Print("Colisión detectada con: ", body.Name);
        if (body is Player player)
        {
            player.TakeDamage((int)_damage);
            QueueFree();
        }
    }
}