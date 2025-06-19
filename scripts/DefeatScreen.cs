using Godot;

public partial class DefeatScreen : Control
{
    [Signal] public delegate void RetryButtonPressedEventHandler();
    [Signal] public delegate void MenuButtonPressedEventHandler();

    public override void _Ready()
    {
        // Update defeat message untuk menampilkan "Rempah Telah Dicuri"
        var messageLabel = GetNode<Label>("MainPanel/VBoxContainer/MessageLabel");
        if (messageLabel != null)
        {
            messageLabel.Text = "REMPAH TELAH DICURI!\nPerdagangan Nusantara Telah Runtuh";
        }

        // Optional: Play defeat sound effect
        // AudioManager.PlayDefeatSound();
    }

    private void _on_retry_button_pressed()
    {
        EmitSignal(SignalName.RetryButtonPressed);
        // Restart current level
        GetTree().ReloadCurrentScene();
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