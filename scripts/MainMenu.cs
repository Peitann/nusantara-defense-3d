using Godot;

public partial class MainMenu : Control
{
	private AudioStreamPlayer menuMusic;
	private AudioStreamPlayer buttonSound;
	private TextureRect titleImage;
	private VBoxContainer buttonContainer;
	
	public override void _Ready()
	{
		// Get references to UI elements
		titleImage = GetNode<TextureRect>("CenterContainer/VBoxContainer/TitleContainer/TitleImage");
		buttonContainer = GetNode<VBoxContainer>("CenterContainer/VBoxContainer/ButtonContainer");
		
		// Setup audio
		SetupAudio();
		
		// No entrance animations needed
		
		// Setup button effects without animations
		SetupButtonEffects();
	}
	
	private void SetupAudio()
	{
		// Background music
		menuMusic = new AudioStreamPlayer();
		AddChild(menuMusic);
		
		// Button sound effects
		buttonSound = new AudioStreamPlayer();
		AddChild(buttonSound);
	}
	
	private void SetupButtonEffects()
	{
		// Add hover effects to all buttons without animations
		var buttons = new[]
		{
			GetNode<Button>("CenterContainer/VBoxContainer/ButtonContainer/StartButton"),
			GetNode<Button>("CenterContainer/VBoxContainer/ButtonContainer/BattlefieldButton"),
			GetNode<Button>("CenterContainer/VBoxContainer/ButtonContainer/SettingsButton"),
			GetNode<Button>("CenterContainer/VBoxContainer/ButtonContainer/AboutButton"),
			GetNode<Button>("CenterContainer/VBoxContainer/ButtonContainer/ExitButton")
		};
		
		foreach (var button in buttons)
		{
			if (button != null)
			{
				// Just change color on hover, no animations
				button.MouseEntered += () => button.Modulate = new Color(1.1f, 1.1f, 1.0f, 1.0f);
				button.MouseExited += () => button.Modulate = new Color(1, 1, 1, 1);
			}
		}
	}
	
	private void PlayButtonSound()
	{
		if (buttonSound != null && buttonSound.Stream != null)
		{
			buttonSound.Play();
		}
	}
	
	// Signal handlers with no animations
	private void _on_start_button_pressed()
	{
		PlayButtonSound();
		GetTree().ChangeSceneToFile("res://scenes/main.tscn");
	}
	
	private void _on_battlefield_button_pressed()
	{
		PlayButtonSound();
		GD.Print("Battlefield selection not implemented yet");
		
		// For now, go to game
		GetTree().ChangeSceneToFile("res://scenes/main.tscn");
	}
	
	private void _on_settings_button_pressed()
	{
		PlayButtonSound();
		ShowSettingsDialog();
	}
	
	private void _on_about_button_pressed()
	{
		PlayButtonSound();
		ShowAboutDialog();
	}
	
	private void _on_exit_button_pressed()
	{
		PlayButtonSound();
		GetTree().Quit();
	}
	
	private void ShowSettingsDialog()
	{
		var dialog = new AcceptDialog();
		dialog.Title = "Pengaturan";
		dialog.DialogText = @"Menu pengaturan akan segera hadir!

Fitur yang akan ditambahkan:
• Volume musik dan efek suara
• Kualitas grafis
• Kontrol input  
• Bahasa
• Resolusi layar";
		dialog.Size = new Vector2I(450, 350);
		
		AddChild(dialog);
		dialog.PopupCentered();
		
		dialog.Confirmed += () => dialog.QueueFree();
	}
	
	private void ShowAboutDialog()
	{
		var dialog = new AcceptDialog();
		dialog.Title = "Tentang Sejarah Nusantara";
		dialog.DialogText = @"PERTAHANAN REMPAH NUSANTARA

Terinspirasi dari perjuangan heroik bangsa Indonesia melawan penjajahan kolonial yang berusaha menguasai kekayaan rempah-rempah Nusantara.

🌶️ SEJARAH REMPAH NUSANTARA
Rempah-rempah seperti pala, cengkeh, lada hitam, dan kayu manis adalah komoditas berharga yang menjadikan Nusantara sebagai pusat perdagangan dunia selama berabad-abad.

⚔️ PERLAWANAN HEROIK
Kerajaan-kerajaan Nusantara seperti Majapahit, Sriwijaya, dan Aceh mempertahankan kedaulatan dengan gagah berani melawan invasi bangsa asing.

🏛️ WARISAN BUDAYA
Arsitektur tradisional, sistem pertahanan, dan semangat gotong royong menjadi kekuatan utama dalam menghadapi tantangan.

'Lebih baik mati berdiri daripada hidup berlutut'
- Semangat perjuangan yang tidak pernah padam";
		
		dialog.Size = new Vector2I(600, 500);
		
		AddChild(dialog);
		dialog.PopupCentered();
		
		dialog.Confirmed += () => dialog.QueueFree();
	}
	
	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventKey keyEvent && keyEvent.Pressed)
		{
			switch (keyEvent.Keycode)
			{
				case Key.Enter:
				case Key.Space:
					_on_start_button_pressed();
					break;
				case Key.Escape:
					_on_exit_button_pressed();
					break;
			}
		}
	}
}
