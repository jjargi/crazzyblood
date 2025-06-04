
using Godot;

public partial class StraightProjectile : EnemyProjectile
{
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta); // Ya se encarga del movimiento recto y colisión
    }
}