using Godot;

public partial class SinusoidalProjectile : EnemyProjectile
{
    private float _amplitude;
    private float _frequency;
    private float _time = 0f;
    private Vector2 _originalDirection;
    private float _projectileSpeed; // Almacenar la velocidad localmente

    public void Initialize(Vector2 startPosition, Vector2 direction, float speed,
                         int damage, TileMapLayer tileMap, float amplitude, float frequency)
    {
        base.Initialize(startPosition, direction, speed, damage, tileMap);
        _amplitude = amplitude;
        _frequency = frequency;
        _originalDirection = direction.Normalized();
        _projectileSpeed = speed;
    }

    public override void _PhysicsProcess(double delta)
    {
        _time += (float)delta;

        // Calcular desplazamiento sinusoidal perpendicular a la dirección
        Vector2 perpendicular = new Vector2(-_originalDirection.Y, _originalDirection.X);
        Vector2 offset = perpendicular * Mathf.Sin(_time * _frequency) * _amplitude;

        // Calcular nueva posición (usando Position en lugar de Velocity)
        Vector2 movement = _originalDirection * _projectileSpeed * (float)delta;
        Position += movement + offset * (float)delta;

        // Rotar el proyectil para que siga la dirección del movimiento
        Rotation = (_originalDirection + offset * 0.1f).Angle();

        // Verificar colisión con tiles (como en la clase base)
        if (_tileMap != null)
        {
            Vector2I tileCoords = _tileMap.LocalToMap(GlobalPosition);
            int sourceId = _tileMap.GetCellSourceId(tileCoords);

            if (sourceId == -1)
            {
                QueueFree();
            }
        }
    }
}