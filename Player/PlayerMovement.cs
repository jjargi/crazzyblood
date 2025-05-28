
using System;
using Godot;
using static Godot.TextServer;

public partial class PlayerMovement : Node2D
{
    private Player _player;
    private TileMapLayer _tileMap;
    private AnimatedSprite2D _sprite;
    private WeaponSystem _weaponSystem;
    public override void _Ready()
    {
        _weaponSystem = GetNode<WeaponSystem>("../WeaponSystem");

    }
    public void Initialize(Player player)
    {
        _player = player;
        _tileMap = player.GetParent().GetNode<TileMapLayer>("TileMapLayer");
        _sprite = player.GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        _sprite.Play("default");
    }

    public void HandleMovement(double delta)
    {
        Vector2I currentTile = _tileMap.LocalToMap(_player.GlobalPosition);
        Vector2I targetTile = currentTile;
        // Inicializa la dirección
        Vector2I direction = Vector2I.Right;
        // Detecta input y establece dirección
        if (Input.IsActionJustPressed("ui_right")) { targetTile.X += 1; direction = Vector2I.Right; }
        if (Input.IsActionJustPressed("ui_left")) { targetTile.X -= 1; direction = Vector2I.Left; }
        if (Input.IsActionJustPressed("ui_down")) { targetTile.Y += 1; direction = Vector2I.Down; }
        if (Input.IsActionJustPressed("ui_up")) { targetTile.Y -= 1; direction = Vector2I.Up; }

        if (_tileMap.GetUsedCells().Contains(targetTile))
        {
            _player.GlobalPosition = _tileMap.MapToLocal(targetTile);
            UpdateAnimation(_weaponSystem._currentWeaponType);
        }
        
    }
    private void UpdateAnimation(string weapon)
    {
        if (_player._isDying) return;
        if (weapon == "BasicWeapon") _player.GetNode<AnimatedSprite2D>("AnimatedSprite2D").Play("default");//RIGHT
        else if (weapon == "ExplosiveWeapon") _player.GetNode<AnimatedSprite2D>("AnimatedSprite2D").Play("explosive");//LEFT
        else if (weapon == "NuclearWeapon") _player.GetNode<AnimatedSprite2D>("AnimatedSprite2D").Play("nuclear");//LEFT
    }
    public void SetTileMap(TileMapLayer tileMap)
    {
        _tileMap = tileMap;
    }

}