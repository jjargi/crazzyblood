using Godot;

public partial class ExplosionEffect : Area2D
{
    [Export] private float _duration = 0.5f;
    private AnimatedSprite2D _sprite;
    private float _damage;
    [Export] private float Lifetime = 0.5f;
    public override void _Ready()
    {
        _sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        if (_sprite == null)
        {
            GD.PrintErr("Error: No se encontró AnimatedSprite2D");
            return;
        }

        _sprite.Play("default");
        _sprite.AnimationFinished += () => QueueFree();

        var timer = new Timer();
        AddChild(timer);
        timer.Start(_duration);
    }

    public void Initialize(float damage)
    {
        //_damage = damage;
        _damage = damage;
        //GetNode<AnimationPlayer>("AnimationPlayer").Play("explode");esto falla!!
        GetTree().CreateTimer(Lifetime).Timeout += () => QueueFree();
    }

    private void _on_body_entered(Node body)
    {
        if (body is Node2D body2D && body.HasMethod("TakeDamage"))
        {
            body.Call("TakeDamage", _damage);
            GD.Print($"Daño por explosión: {_damage}");
        }
    }
}