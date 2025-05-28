////using Godot;
////using System;

////public partial class EffectManager : Node
////{
////	// Called when the node enters the scene tree for the first time.
////	public override void _Ready()
////	{
////	}

////	// Called every frame. 'delta' is the elapsed time since the previous frame.
////	public override void _Process(double delta)
////	{
////	}
////}
//// EffectManager.cs
using Godot;
using System.Collections.Generic;

public partial class EffectManager : Node2D
{
    //    private static EffectManager _instance;

    //    // Diccionarios para categorizar los efectos
    //    private Dictionary<string, List<Node>> _projectiles = new();
    //    private Dictionary<string, List<Node>> _explosions = new();

    //    public static EffectManager Instance => _instance;

    //    public override void _Ready()
    //    {
    //        if (_instance != null && _instance != this) QueueFree();
    //        else _instance = this;

    //        // Inicializar categorías
    //        _projectiles.Add("player", new List<Node>());
    //        _projectiles.Add("enemy", new List<Node>());
    //        _explosions.Add("player", new List<Node>());
    //        _explosions.Add("enemy", new List<Node>());
    //    }

    //    public void RegisterProjectile(Node projectile, string sourceType)
    //    {
    //        if (!_projectiles.ContainsKey(sourceType)) return;

    //        _projectiles[sourceType].Add(projectile);
    //        projectile.TreeExiting += () => _projectiles[sourceType].Remove(projectile);
    //    }

    //    public void RegisterExplosion(Node explosion, string sourceType)
    //    {
    //        if (!_explosions.ContainsKey(sourceType)) return;

    //        _explosions[sourceType].Add(explosion);
    //        explosion.TreeExiting += () => _explosions[sourceType].Remove(explosion);
    //    }

    //    public void ClearAllEffects()
    //    {
    //        ClearByType("player");
    //        ClearByType("enemy");
    //    }

    //    public void ClearByType(string type)
    //    {
    //        GD.Print($"Limpiando efectos de tipo: {type}");

    //        // Limpiar proyectiles
    //        foreach (var projectile in _projectiles[type].ToArray())
    //        {
    //            if (IsInstanceValid(projectile)) projectile.QueueFree();
    //        }
    //        _projectiles[type].Clear();

    //        // Limpiar explosiones
    //        foreach (var explosion in _explosions[type].ToArray())
    //        {
    //            if (IsInstanceValid(explosion)) explosion.QueueFree();
    //        }
    //        _explosions[type].Clear();
    //    }
}