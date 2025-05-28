using Godot;
using System;

//public partial class EnemyStats : Node
//{
//	// Called when the node enters the scene tree for the first time.
//	public override void _Ready()
//	{
//	}

//	// Called every frame. 'delta' is the elapsed time since the previous frame.
//	public override void _Process(double delta)
//	{
//	}
//}
public struct EnemyStats
{
    public double Speed;
    public int Health;
    public int Damage;
    public double AttackCooldown;
    public double ProjectileSpeed;
    public float TileSize; // Añadido para el movimiento por celdas
}