using Godot;
using System.Collections.Generic;

public partial class WeaponSystem : Node2D
{
    [Export] private PackedScene BasicProjectileScene;
    [Export] private PackedScene ExplosiveProjectileScene;
    [Export] private PackedScene NuclearProjectileScene;
    [Export] private float BasicFireRate = 0.5f;
    [Export] private float ExplosiveFireRate = 1.0f;
    [Export] private float NuclearFireRate = 3.0f; // Cooldown más largo
    
    [Export] private float BasicDamage = 1f;
    [Export] private float ExplosiveDamage = 2f;
    [Export] private float NuclearDamage = 1f; // Daño base mayor

    [Export] private float BasicProjectileSpeed = 300f;
    [Export] private float ExplosiveProjectileSpeed = 200f;
    [Export] private float NuclearProjectileSpeed = 150f; // Más lento

    [Export] private float ProjectileSpawnOffset = 34f; // Nueva propiedad para controlar el offset
    private Timer _cooldownTimer;
    private bool _canShoot = true;
    public string _currentWeaponType = "BasicWeapon";
    // Referencia al TileMapLayer actual (no exportada, se asignará dinámicamente)
    private TileMapLayer _currentTileMap;

    // Método para actualizar el TileMap cuando cambia
    public void SetTileMap(TileMapLayer tileMap)
    {
        _currentTileMap = tileMap;
        GD.Print("TileMap actualizado en WeaponSystem");
    }
    public override void _Ready()
    {
        _cooldownTimer = new Timer();
        AddChild(_cooldownTimer);
        _cooldownTimer.Timeout += OnCooldownEnd;
    }

    public void Shoot(Vector2 playerPosition, Vector2 targetPosition)
    {
        if (!_canShoot) return;

        Vector2 direction = (targetPosition - playerPosition ).Normalized();

        switch (_currentWeaponType)
        {
            case "BasicWeapon":
                SpawnBasicProjectile(playerPosition, direction);
                _cooldownTimer.Start(BasicFireRate);
                break;

            case "ExplosiveWeapon":
                SpawnExplosiveProjectile(playerPosition, direction);
                _cooldownTimer.Start(ExplosiveFireRate);
                break;
            case "NuclearWeapon":
                SpawnNuclearProjectile(playerPosition, direction);
                _cooldownTimer.Start(NuclearFireRate);
                break;
        }

        _canShoot = false;
    }

    private void SpawnBasicProjectile(Vector2 position, Vector2 direction)
    {
        var projectile = BasicProjectileScene.Instantiate<BasicProjectile>();
        GetTree().Root.AddChild(projectile);
        // Aplicamos el offset a la posición de spawn
        Vector2 spawnPosition = position + (direction * ProjectileSpawnOffset);
        // Corregido: Usando el nombre correcto del parámetro (baseDamage)
        projectile.Initialize(
            position: spawnPosition,
            direction: direction,
            baseDamage: BasicDamage,
            speed: BasicProjectileSpeed,
            tileMap: _currentTileMap

        );
        //esto falla EffectManager.Instance?.RegisterProjectile(projectile, "player");
    }

    private void SpawnExplosiveProjectile(Vector2 position, Vector2 direction)
    {
        if (_currentTileMap == null || !IsInstanceValid(_currentTileMap))
        {
            GD.PrintErr("Error: TileMap no asignado o inválido para proyectil explosivo");
            return;
        }

        var projectile = ExplosiveProjectileScene.Instantiate<ExplosiveProjectile>();
        GetTree().Root.AddChild(projectile);

        // Usar la posición del mouse como objetivo
        Vector2 targetPos = GetGlobalMousePosition();
        Vector2I targetTile = _currentTileMap.LocalToMap(targetPos);
        Vector2 centeredTargetPos = _currentTileMap.MapToLocal(targetTile);
        projectile.Initialize(
            position: position + (direction * ProjectileSpawnOffset),
            direction: direction,
            baseDamage: ExplosiveDamage,
            speed: ExplosiveProjectileSpeed,
            tileMap: _currentTileMap, // Pasar el TileMapLayer
            centeredTargetPos
           
        );
        //esto falla//projectile.OnExplode += (explosion) => {
        //    EffectManager.Instance?.RegisterExplosion(explosion, "player");
        //};
        //EffectManager.Instance?.RegisterProjectile(projectile, "player");
    }
    private void SpawnNuclearProjectile(Vector2 position, Vector2 direction)
    {
        if (_currentTileMap == null || !IsInstanceValid(_currentTileMap))
        {
            GD.PrintErr("Error: TileMap no asignado o inválido para proyectil nuclear");
            return;
        }

        var projectile = NuclearProjectileScene.Instantiate<NuclearProjectile>();
        GetTree().Root.AddChild(projectile);

        Vector2 targetPos = GetGlobalMousePosition();
        Vector2I targetTile = _currentTileMap.LocalToMap(targetPos);
        Vector2 centeredTargetPos = _currentTileMap.MapToLocal(targetTile);

        projectile.Initialize(
            position: position + (direction * ProjectileSpawnOffset),
            direction: direction,
            baseDamage: NuclearDamage,
            speed: NuclearProjectileSpeed,
            tileMap: _currentTileMap,
            centeredTargetPos
        );
    }
    public void SwitchWeapon(string weaponType)
    {
        _currentWeaponType = weaponType;
    }

    private void OnCooldownEnd()
    {
        _canShoot = true;
    }
}