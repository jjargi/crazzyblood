using Godot;
using System;

//public partial class EnemyConfig : Node
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
public class EnemyConfig
{
    public float MovementSpeed { get; set; } = 100f;
    public int Health { get; set; } = 3;
    public int Damage { get; set; } = 1;
    public float DetectionRadius { get; set; } = 200f;
    public float AttackCooldown { get; set; } = 2f;
    public float ProjectileSpeed { get; set; } = 200f;
    public PackedScene ProjectileScene { get; set; }
}