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

	// Tambahkan method untuk mengubah cash jika diperlukan
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
		
		// Akses PathGenInstance sebagai PathGenerator langsung
		PathGenInstance = GetNode("/root/PathGenInstance") as PathGenerator;
		
		CompleteGrid();
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

		// Convert pathRoute to List<Vector2I>
		var pathRouteList = new System.Collections.Generic.List<Vector2I>();
		foreach (var point in pathRoute)
		{
			pathRouteList.Add(point.AsVector2I());
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
				}
			}
		}

		// Generate path tiles - sama persis dengan GDScript
		for (int i = 0; i < pathRoute.Count; i++)
		{
			int tileScore = PathGenInstance.get_tile_score(i);
			GD.Print($"Tile {i}: score {tileScore}, position {PathGenInstance.get_path_tile(i)}");
			
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
			
			var pathTile = PathGenInstance.get_path_tile(i);
			tile.GlobalPosition = new Vector3(pathTile.X, 0, pathTile.Y);
			tile.GlobalRotationDegrees = tileRotation;
		}
	}

	public Godot.Collections.Array GetPathRoute()
	{
		return PathGenInstance.get_path_route();
	}

	public void TakeDamage(int damage)
	{
		GD.Print($"Player took {damage} damage!");
	}

	// Signal handlers
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
	}
}
