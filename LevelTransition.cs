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
    private float _duration = 3f;


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




    //public void ShowTransition(int currentLevel, int nextLevel, float levelTime,
    //                         string difficulty, List<EnemyData> enemies)
    //{
    public void ShowTransition(int currentLevel, int nextLevel, float levelTime,
                        string difficulty, List<EnemyData> enemies, Action onComplete)
    {
        // Configurar textos
        _titleLabel.Text = $"Nivel {currentLevel} Completado!";
        _infoLabel.Text = $"Tiempo: {levelTime:F2}s | Dificultad: {difficulty}";
        _nextLevelLabel.Text = $"Siguiente nivel: {nextLevel}";

        // Formatear información de enemigos con BBCode
        string enemiesText = "[center]Próximos enemigos:";
        foreach (var enemy in enemies)
        {
            enemiesText += $"\n• {enemy.Type} (x{enemy.Count}): {enemy.Health}HP, {enemy.Damage} daño";
        }
        enemiesText += "[/center]";
        _enemiesLabel.Text = enemiesText;

        // Iniciar animación
        Visible = true;
        //_countdownBar.Value = 100;
        //_startTime = Time.GetTicksMsec();
        //_timer.Start(_duration);
        // Usar solo un temporizador
        GetTree().CreateTween()
            .TweenProperty(_countdownBar, "value", 0, 3.0f)
            .From(100.0f)
            .Finished += () => {
                onComplete?.Invoke();
                QueueFree();
            };
    }

    public override void _Process(double delta)
    {
        if (!_timer.IsStopped())
        {
            double elapsed = (Time.GetTicksMsec() - _startTime) / 1000.0;
            float progress = (float)(elapsed / _duration);
            _countdownBar.Value = 100 - (progress * 100);
        }
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