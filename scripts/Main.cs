using Godot;
using Godot.Collections;

public partial class Main : Node3D
{
	[Export] public PackedScene TileStart { get; set; }
	[Export] public PackedScene TileEnd { get; set; }
	[Export] public PackedScene TileStraight { get; set; }
	[Export] public PackedScene TileCorner { get; set; }
	[Export] public PackedScene TileCrossroads { get; set; }
	[Export] public PackedScene TileEnemy { get; set; }
	[Export] public Array<PackedScene> TileEmpty { get; set; } = new Array<PackedScene>();

	private Resource BASIC_ENEMY_SETTINGS;
	private Resource POWER_ENEMY_SETTINGS;

	[Export] public PackedScene Enemy { get; set; }
	[Export] public Array<Resource> EnemyWaves { get; set; } = new Array<Resource>();

	private bool waveSpawned = false;
	private int enemiesRemaining = 0;

	private Camera3D cam;
	private const float RAYCAST_LENGTH = 100.0f;

	private int currentWaveIndex = 0;

	[Export] public int Cash { get; set; } = 100;

	// Property untuk player health dengan real-time UI update
	private int _playerHealth = 20;
	[Export] 
	public int PlayerHealth 
	{ 
		get => _playerHealth;
		set
		{
			_playerHealth = value;
			UpdateHealthUI(); // Otomatis update UI setiap kali health berubah
		}
	}
	[Export] public int MaxHealth { get; set; } = 20;
	
	// UI elements untuk health
	private Label healthLabel;
	private ProgressBar healthBar;
	
	// TAMBAHAN: Pause system
	private PauseMenu pauseMenu;

	// Method cash management
	public void SetCash(int newCash)
	{
		Cash = newCash;
	}

	public void AddCash(int amount)
	{
		Cash += amount;
		GD.Print($"Cash added: {amount}. New total: {Cash}");
	}

	public void SubtractCash(int amount)
	{
		Cash -= amount;
		GD.Print($"Cash subtracted: {amount}. New total: {Cash}");
	}

	private PathGenerator PathGenInstance;

	public override void _Ready()
	{
		BASIC_ENEMY_SETTINGS = GD.Load<Resource>("res://resources/basic_enemy_settings.res");
		POWER_ENEMY_SETTINGS = GD.Load<Resource>("res://resources/power_enemy_settings.res");
		
		cam = GetNode<Camera3D>("Camera3D");
		
		// Akses PathGenInstance dengan error checking
		PathGenInstance = GetNode("/root/PathGenInstance") as PathGenerator;
		if (PathGenInstance == null)
		{
			GD.PrintErr("PathGenInstance not found! Check AutoLoad settings.");
			return;
		}
		
		// Debug PathGen info
		GD.Print($"PathGenInstance found: {PathGenInstance}");
		GD.Print($"Map config: {PathGenInstance.path_config}");
		
		// Inisialisasi UI health dengan initial value assignment
		try
		{
			healthLabel = GetNode<Label>("Control/HealthLabel");
			healthBar = GetNode<ProgressBar>("Control/HealthBar");
			
			// Set initial values
			if (healthBar != null)
			{
				healthBar.MaxValue = MaxHealth;
				healthBar.Value = PlayerHealth;
			}
			UpdateHealthUI();
		}
		catch (System.Exception e)
		{
			GD.PrintErr($"Failed to initialize health UI: {e.Message}");
			healthLabel = null;
			healthBar = null;
		}
		
		// TAMBAHAN: Setup pause menu
		try
		{
			pauseMenu = GetNode<PauseMenu>("Control/PauseMenuUI");
			if (pauseMenu != null)
			{
				GD.Print("Pause menu found and ready");
			}
			else
			{
				GD.PrintErr("PauseMenuUI not found in scene tree");
			}
		}
		catch (System.Exception e)
		{
			GD.PrintErr($"Failed to setup pause menu: {e.Message}");
		}
		
		CompleteGrid();
	}

	// TAMBAHAN: Input handling untuk pause
	public override void _Input(InputEvent @event)
	{
		// Handle pause input - hanya jika game tidak sedang paused
		if (@event.IsActionPressed("ui_cancel") || (@event is InputEventKey keyEvent && keyEvent.Keycode == Key.Escape && keyEvent.Pressed))
		{
			if (pauseMenu != null && !pauseMenu.IsPaused())
			{
				pauseMenu.PauseGame();
				GetViewport().SetInputAsHandled();
			}
		}
	}

	public override void _Process(double delta)
	{
		GetNode<Label>("Control/CashLabel").Text = $"Cash ${Cash}";
	}

	private async void SpawnWave()
	{
		if (currentWaveIndex >= EnemyWaves.Count) 
		{
			GD.Print("No more waves available");
			return;
		}
		
		GD.Print($"Starting wave {currentWaveIndex}");
		
		// Akses Wave resource dengan benar
		var waveResource = EnemyWaves[currentWaveIndex] as Wave;
		if (waveResource == null)
		{
			GD.Print("Wave resource is null");
			return;
		}
		
		var enemyWave = waveResource.Enemies;
		waveSpawned = false;
		enemiesRemaining = 0;

		GD.Print($"Wave has {enemyWave.Count} enemies");

		for (int i = 0; i < enemyWave.Count; i++)
		{
			await ToSignal(GetTree().CreateTimer(enemyWave[i].NextEnemyDelay), SceneTreeTimer.SignalName.Timeout);
			GD.Print($"Instantiating enemy {i + 1}");
			
			var enemy2 = Enemy.Instantiate() as BasicEnemy;
			if (enemy2 == null)
			{
				GD.Print("Failed to instantiate enemy");
				continue;
			}
			
			enemy2.EnemySettings = enemyWave[i];
			
			AddChild(enemy2);
			enemy2.AddToGroup("enemies");
			enemiesRemaining += 1;
			enemy2.EnemyFinished += CheckWave;
		}

		waveSpawned = true;
		GD.Print($"Wave spawned with {enemiesRemaining} enemies");
	}

	private void CheckWave()
	{
		GD.Print("Got here!");
		enemiesRemaining -= 1;
		if (enemiesRemaining <= 0 && waveSpawned)
		{
			GetNode("StateChart").Call("send_event", "to_complete");
		}
	}

	private void CompleteGrid()
	{
		// Akses langsung seperti di GDScript
		var mapLength = PathGenInstance.path_config.MapLength;
		var mapHeight = PathGenInstance.path_config.MapHeight;
		var pathRoute = PathGenInstance.get_path_route();

		GD.Print($"Map size: {mapLength}x{mapHeight}, Path length: {pathRoute.Count}");
		
		// Debug: Print path route content
		GD.Print($"PathRoute type: {pathRoute.GetType()}");
		for (int i = 0; i < pathRoute.Count; i++)
		{
			GD.Print($"PathRoute[{i}]: {pathRoute[i]} (Type: {pathRoute[i].GetType()})");
		}

		// Convert pathRoute to List<Vector2I> dengan cara yang lebih sederhana
		var pathRouteList = new System.Collections.Generic.List<Vector2I>();
		
		for (int i = 0; i < pathRoute.Count; i++)
		{
			var point = pathRoute[i];
			Vector2I vector2I;
			
			try
			{
				// Coba konversi langsung dari Variant ke Vector2
				var vec2 = point.AsVector2();
				vector2I = new Vector2I((int)vec2.X, (int)vec2.Y);
			}
			catch
			{
				try
				{
					// Alternatif: coba konversi ke Vector2I langsung
					vector2I = point.AsVector2I();
				}
				catch
				{
					GD.PrintErr($"Failed to convert point {i}: {point}");
					continue;
				}
			}
			
			pathRouteList.Add(vector2I);
			GD.Print($"Added to pathRouteList: {vector2I}");
		}
		
		GD.Print($"Final pathRouteList count: {pathRouteList.Count}");

		// Debug: Pastikan semua tile resources ada
		GD.Print($"TileEmpty count: {TileEmpty?.Count ?? 0}");
		GD.Print($"TileStart: {TileStart}");
		GD.Print($"TileEnd: {TileEnd}");
		GD.Print($"TileStraight: {TileStraight}");
		GD.Print($"TileCorner: {TileCorner}");
		GD.Print($"TileCrossroads: {TileCrossroads}");
		
		if (TileEmpty == null || TileEmpty.Count == 0)
		{
			GD.PrintErr("TileEmpty is null or empty!");
			return;
		}

		// Generate empty tiles - sama persis dengan GDScript
		for (int x = 0; x < mapLength; x++)
		{
			for (int y = 0; y < mapHeight; y++)
			{
				var currentPos = new Vector2I(x, y);
				
				if (!pathRouteList.Contains(currentPos))
				{
					// pick_random equivalent
					var randomIndex = GD.RandRange(0, TileEmpty.Count - 1);
					var tile = TileEmpty[randomIndex].Instantiate() as Node3D;
					AddChild(tile);
					tile.GlobalPosition = new Vector3(x, 0, y);
					tile.GlobalRotationDegrees = new Vector3(0, GD.RandRange(0, 3) * 90, 0);
					GD.Print($"Created empty tile at ({x}, {y})");
				}
			}
		}

		// Generate path tiles - sama persis dengan GDScript
		for (int i = 0; i < pathRoute.Count; i++)
		{
			int tileScore = PathGenInstance.get_tile_score(i);
			var pathTile = PathGenInstance.get_path_tile(i);
			GD.Print($"Tile {i}: score {tileScore}, position {pathTile}");
			
			var tile = TileEmpty[0].Instantiate() as Node3D;
			var tileRotation = Vector3.Zero;

			// Logic yang sama persis dengan GDScript
			if (tileScore == 2)
			{
				tile = TileEnd.Instantiate() as Node3D;
				tileRotation = new Vector3(0, -90, 0);
			}
			else if (tileScore == 8)
			{
				tile = TileStart.Instantiate() as Node3D;
				tileRotation = new Vector3(0, 90, 0);
			}
			else if (tileScore == 10)
			{
				tile = TileStraight.Instantiate() as Node3D;
				tileRotation = new Vector3(0, 90, 0);
			}
			else if (tileScore == 1 || tileScore == 4 || tileScore == 5)
			{
				tile = TileStraight.Instantiate() as Node3D;
				tileRotation = new Vector3(0, 0, 0);
			}
			else if (tileScore == 6)
			{
				tile = TileCorner.Instantiate() as Node3D;
				tileRotation = new Vector3(0, 180, 0);
			}
			else if (tileScore == 12)
			{
				tile = TileCorner.Instantiate() as Node3D;
				tileRotation = new Vector3(0, 90, 0);
			}
			else if (tileScore == 9)
			{
				tile = TileCorner.Instantiate() as Node3D;
				tileRotation = new Vector3(0, 0, 0);
			}
			else if (tileScore == 3)
			{
				tile = TileCorner.Instantiate() as Node3D;
				tileRotation = new Vector3(0, 270, 0);
			}
			else if (tileScore == 15)
			{
				tile = TileCrossroads.Instantiate() as Node3D;
				tileRotation = new Vector3(0, 0, 0);
			}

			AddChild(tile);
			tile.GlobalPosition = new Vector3(pathTile.X, 0, pathTile.Y);
			tile.GlobalRotationDegrees = tileRotation;
			GD.Print($"Created path tile at ({pathTile.X}, {pathTile.Y}) with score {tileScore}");
		}
	}

	public Godot.Collections.Array GetPathRoute()
	{
		return PathGenInstance.get_path_route();
	}

	// MODIFIKASI: TakeDamage sekarang menerima damage 2 per enemy dan update UI real-time
	public void TakeDamage(int damage)
	{
		PlayerHealth -= damage; // Ini akan otomatis trigger UpdateHealthUI via property setter
		PlayerHealth = Mathf.Max(0, PlayerHealth);
		
		GD.Print($"Player took {damage} damage! Health: {PlayerHealth}/{MaxHealth}");
		
		// Check game over IMMEDIATELY setelah damage
		if (PlayerHealth <= 0)
		{
			CheckGameEnd();
		}
	}

	// Method untuk update health UI (dipanggil otomatis oleh property setter)
	private void UpdateHealthUI()
	{
		if (healthLabel != null)
			healthLabel.Text = $"Health: {PlayerHealth}/{MaxHealth}";
		if (healthBar != null)
			healthBar.Value = PlayerHealth;
	}

	// Signal handlers - tetap sama
	private void _on_start_wave_button_pressed()
	{
		GetNode("StateChart").Call("send_event", "to_active");
	}

	private void _on_active_state_entered()
	{
		GetNode<Button>("StartWaveButton").Disabled = true;
		SpawnWave();
	}

	private void _on_complete_state_entered()
	{
		currentWaveIndex += 1;
		GetNode<Button>("StartWaveButton").Disabled = false;
		
		// Check if all waves completed
		CheckGameEnd();
	}

	// TAMBAHAN: Pause menu signal handlers
	private void _on_pause_menu_ui_resume_requested()
	{
		GD.Print("Resume game requested from pause menu");
	}

	private void _on_pause_menu_ui_restart_requested()
	{
		GD.Print("Restart game requested from pause menu");
	}

	private void _on_pause_menu_ui_main_menu_requested()
	{
		GD.Print("Main menu requested from pause menu");
	}

	// Method untuk menangani win/lose conditions - tetap sama
	private void CheckGameEnd()
	{
		// Player kalah jika health habis
		if (PlayerHealth <= 0)
		{
			ShowDefeatScreen();
			return;
		}

		// Cek jika semua wave sudah diselesaikan
		if (currentWaveIndex >= EnemyWaves.Count)
		{
			ShowWinScreen();
			return;
		}
	}

	public void ShowWinScreen()
	{
		var winScreen = GD.Load<PackedScene>("res://scenes/win_screen.tscn").Instantiate() as WinScreen;
		GetTree().CurrentScene.AddChild(winScreen);
		winScreen.SetScore(Cash); // Atau sistem scoring lain
	}

	public void ShowDefeatScreen()
	{
		var defeatScreen = GD.Load<PackedScene>("res://scenes/defeat_screen.tscn").Instantiate() as DefeatScreen;
		GetTree().CurrentScene.AddChild(defeatScreen);
	}
}
