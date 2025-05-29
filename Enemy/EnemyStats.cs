using Godot;
using System;

public struct EnemyStats
{
    public double Speed;
    public int Health;
    public int Damage;
    public double AttackCooldown;
    public double ProjectileSpeed;
    public float TileSize; // Añadido para el movimiento por celdas
}