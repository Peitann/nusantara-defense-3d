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
        try
        {
            titleImage = GetNode<TextureRect>("CenterContainer/VBoxContainer/TitleContainer/TitleImage");
            buttonContainer = GetNode<VBoxContainer>("CenterContainer/VBoxContainer/ButtonContainer");
        }
        catch (System.Exception e)
        {
            GD.PrintErr($"Could not find node: {e.Message}");
            return; // Skip further processing in _Ready
        }

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
            GetNode<Button>("CenterContainer/VBoxContainer/ButtonContainer/GuideButton"),
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

    private void _on_guide_button_pressed()
    {
        PlayButtonSound();
        ShowGuideDialog();
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

    private void ShowGuideDialog()
    {
        var dialog = new AcceptDialog();
        dialog.Title = "Panduan Permainan - Pertahanan Rempah Nusantara";
        dialog.DialogText = @"🎯 TUJUAN PERMAINAN
Lindungi gudang rempah dari invasi penjajah! Jangan biarkan kondisi rempah mencapai nol.

⚔️ CARA BERMAIN
• Klik 'MULAI GELOMBANG' untuk memulai serangan musuh
• Tempatkan meriam dengan drag & drop dari panel kiri
• Meriam otomatis menembak musuh dalam jangkauan
• Setiap musuh yang dikalahkan memberikan uang

🏰 STRATEGI PERTAHANAN
• Tempatkan meriam di lokasi strategis
• Blokir jalur musuh dengan penempatan cerdas
• Upgrade meriam untuk damage lebih besar
• Kelola uang dengan bijak

💰 SISTEM EKONOMI
• Uang awal: $100
• Meriam Dasar: $100
• Meriam Kuat: $200
• Setiap musuh memberikan $25

⚡ KONTROL
• ESC: Pause/Resume game
• Mouse: Drag & drop meriam
• Klik: Interaksi dengan UI

🌶️ KONDISI REMPAH
• Kondisi Rempah: 5/5 (mulai penuh)
• Setiap musuh yang lolos mengurangi 2 kondisi
• Jika kondisi rempah habis, rempah akan dicuri!

🏆 KEMENANGAN
Bertahan hidup dan kalahkan semua gelombang musuh untuk mempertahankan kedaulatan Nusantara!";

        dialog.Size = new Vector2I(650, 750);

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
Kerajaan-kerajaan Nusantara mempertahankan kedaulatan dengan gagah berani melawan invasi bangsa asing.

'Lebih baik mati berdiri daripada hidup berlutut'
- Semangat perjuangan yang tidak pernah padam

Game ini menggunakan Template dari WideArchShark dari Youtube & Github

Dimodifikasi oleh : Ahmad Fatan Haidar (231524034) D4-2B";

        dialog.Size = new Vector2I(600, 600);

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
