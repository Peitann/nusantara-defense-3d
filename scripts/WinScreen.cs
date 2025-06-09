using Godot;

public partial class WinScreen : Control
{
    [Signal] public delegate void ContinueButtonPressedEventHandler();
    [Signal] public delegate void MenuButtonPressedEventHandler();

    private Label scoreLabel;
    private int finalScore = 0;

    public override void _Ready()
    {
        scoreLabel = GetNode<Label>("MainPanel/VBoxContainer/ScoreContainer/ScoreValue");
        
        // Ambil score dari game manager atau main scene
        var main = GetTree().CurrentScene as Main;
        if (main != null)
        {
            finalScore = main.Cash; // Atau sistem scoring lain yang Anda gunakan
            UpdateScoreDisplay();
        }
    }

    public void SetScore(int score)
    {
        finalScore = score;
        UpdateScoreDisplay();
    }

    private void UpdateScoreDisplay()
    {
        if (scoreLabel != null)
        {
            scoreLabel.Text = finalScore.ToString("N0");
        }
    }

    private void _on_continue_button_pressed()
    {
        EmitSignal(SignalName.ContinueButtonPressed);
        // Atau langsung load scene berikutnya
        GetTree().ChangeSceneToFile("res://scenes/main.tscn");
    }

    private void _on_menu_button_pressed()
    {
        EmitSignal(SignalName.MenuButtonPressed);
        // Kembali ke main menu
        GetTree().ChangeSceneToFile("res://scenes/main_menu.tscn");
    }

    public override void _Input(InputEvent @event)
    {
        // Optional: ESC key untuk kembali ke menu
        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            if (keyEvent.Keycode == Key.Escape)
            {
                _on_menu_button_pressed();
            }
        }
    }
}