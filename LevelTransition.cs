using Godot;
using System;
using System.Collections.Generic;

public partial class LevelTransition : CanvasLayer
{
    private Label _titleLabel;
    private Label _infoLabel;
    private RichTextLabel _enemiesLabel;
    private Label _nextLevelLabel;
    private TextureProgressBar _countdownBar;

    private Timer _timer;
    private double _startTime;
    private float _duration = 4f;


    public override void _Ready()
    {
        // Obtener referencias a los nodos usando GetNode
        var panel = GetNode<Panel>("Panel");
        var vbox = panel.GetNode<VBoxContainer>("VBoxContainer");

        _titleLabel = vbox.GetNode<Label>("TitleLabel");
        _infoLabel = vbox.GetNode<Label>("InfoLabel");
        _enemiesLabel = vbox.GetNode<RichTextLabel>("EnemiesLabel");
        _nextLevelLabel = vbox.GetNode<Label>("NextLevelLabel");
        _countdownBar = vbox.GetNode<TextureProgressBar>("CountdownBar");

        Visible = false;
        _timer = new Timer();
        AddChild(_timer);
        _timer.Timeout += OnTimeout;
    }

    public void ShowTransition(int currentLevel, int nextLevel, float levelTime,
                        string difficulty, List<EnemyData> enemies, Action onComplete,
                        int bossIndex = -1) // Parámetro opcional
    {
        // Configurar textos
        _titleLabel.Text = $"Nivel {currentLevel} Completado!";
        _infoLabel.Text = $"Tiempo: {levelTime:F2}s | Dificultad: {difficulty}";
        _nextLevelLabel.Text = $"Siguiente nivel: {nextLevel}";

        // Formatear información de enemigos con BBCode
        string enemiesText = "[center]Próximos enemigos:";

        // Añadir información especial si hay jefe
        if (bossIndex >= 0)
        {
            enemiesText += $"\n\n[color=yellow]¡Jefe especial en el nivel {nextLevel}![/color]";
        }

        foreach (var enemy in enemies)
        {
            enemiesText += $"\n• {enemy.Type} (x{enemy.Count}): {enemy.Health:F0}HP, {enemy.Damage:F0} daño";
        }
        enemiesText += "[/center]";
        _enemiesLabel.Text = enemiesText;

        // Resto del método permanece igual...
        Visible = true;
        _countdownBar.MaxValue = 100;
        _countdownBar.Value = 100;

        var tween = GetTree().CreateTween();
        tween.TweenProperty(_countdownBar, "value", 0, _duration)
             .SetTrans(Tween.TransitionType.Linear)
             .SetEase(Tween.EaseType.InOut);

        tween.Finished += () => {
            onComplete?.Invoke();
            QueueFree();
        };
    }

    public override void _Process(double delta)
    {
        if (_countdownBar != null)
            GD.Print("CountdownBar Value: ", _countdownBar.Value);
        //if (!_timer.IsStopped())
        //{
        //    double elapsed = (Time.GetTicksMsec() - _startTime) / 1000.0;
        //    float progress = (float)(elapsed / _duration);
        //    _countdownBar.Value = 100 - (progress * 100);
        //}
    }

    private void OnTimeout()
    {
        QueueFree();
    }
}

public class EnemyData
{
    public string Type { get; set; }
    public int Count { get; set; }
    public float Health { get; set; }
    public float Damage { get; set; }
}