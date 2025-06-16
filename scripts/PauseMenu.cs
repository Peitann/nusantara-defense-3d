using Godot;

public partial class PauseMenu : Control
{
	// Define signals properly
	[Signal]
	public delegate void OnResumeRequestedEventHandler();
	
	[Signal]
	public delegate void OnRestartRequestedEventHandler();
	
	[Signal]
	public delegate void OnMainMenuRequestedEventHandler();

	private bool isPaused = false;
	private Button resumeButton;
	private Button restartButton;
	private Button mainMenuButton;

	public override void _Ready()
	{
		// Start hidden
		Visible = false;
		ProcessMode = ProcessModeEnum.WhenPaused;
		
		// Get button references
		try
		{
			resumeButton = GetNode<Button>("CenterContainer/PanelContainer/VBoxContainer/ButtonContainer/ResumeButton");
			restartButton = GetNode<Button>("CenterContainer/PanelContainer/VBoxContainer/ButtonContainer/RestartButton");
			mainMenuButton = GetNode<Button>("CenterContainer/PanelContainer/VBoxContainer/ButtonContainer/MainMenuButton");
			
			GD.Print("PauseMenu buttons initialized successfully");
		}
		catch (System.Exception e)
		{
			GD.PrintErr($"Error initializing PauseMenu buttons: {e.Message}");
		}
	}

	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("ui_cancel") || (@event is InputEventKey keyEvent && keyEvent.Keycode == Key.Escape && keyEvent.Pressed))
		{
			if (isPaused)
			{
				ResumeGame();
			}
			else
			{
				PauseGame();
			}
			GetViewport().SetInputAsHandled();
		}
	}

	public void PauseGame()
	{
		isPaused = true;
		GetTree().Paused = true;
		Visible = true;
		
		// Focus on resume button for keyboard navigation
		if (resumeButton != null)
		{
			resumeButton.GrabFocus();
		}
		
		GD.Print("Game Paused");
	}

	public void ResumeGame()
	{
		isPaused = false;
		GetTree().Paused = false;
		Visible = false;
		GD.Print("Game Resumed");
	}

	public bool IsPaused()
	{
		return isPaused;
	}

	// Button signal handlers
	private void _on_resume_button_pressed()
	{
		EmitSignal(SignalName.OnResumeRequested);
		ResumeGame();
	}

	private void _on_restart_button_pressed()
	{
		EmitSignal(SignalName.OnRestartRequested);
		ResumeGame(); // Unpause before restarting
		GetTree().ReloadCurrentScene();
	}

	private void _on_main_menu_button_pressed()
	{
		EmitSignal(SignalName.OnMainMenuRequested);
		ResumeGame(); // Unpause before changing scene
		GetTree().ChangeSceneToFile("res://scenes/main_menu.tscn");
	}
}
