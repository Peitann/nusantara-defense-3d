using Godot;

public partial class BasicEnemy : Node3D
{
	[Signal] public delegate void EnemyFinishedEventHandler();
	
	[Export] public EnemySettings EnemySettings { get; set; }
	
	private ProgressBar progressBar;
	private float enemyHealth;
	public bool attackable = true;
	private float distanceTravelled = 0;
	private Path3D path3D;
	private PathFollow3D pathFollow3D;

	public override void _Ready()
	{
		progressBar = GetNode<ProgressBar>("HealthBar/ProgressBar");
		enemyHealth = EnemySettings.Health;
		progressBar.MaxValue = enemyHealth;
		progressBar.Value = enemyHealth;
		
		var enemyMesh = EnemySettings.EnemyScene.Instantiate();
		GetNode("Path3D/PathFollow3D/Enemy").AddChild(enemyMesh);
		
		var path3DNode = GetNode<Path3D>("Path3D");
		path3DNode.Curve = PathRouteToCurve3D();
		GetNode<PathFollow3D>("Path3D/PathFollow3D").Progress = 0;
	}

	private async void _on_spawning_state_entered()
	{
		attackable = true;
		progressBar.Visible = false;
		var animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		animationPlayer.Play("spawn");
		await ToSignal(animationPlayer, AnimationPlayer.SignalName.AnimationFinished);
		GetNode("EnemyStateChart").Call("send_event", "to_travelling_state");
	}

	private void _on_travelling_state_entered()
	{
		attackable = true;
		progressBar.Visible = true;
	}

	private void _on_travelling_state_processing(double delta)
	{
		distanceTravelled += (float)(delta * EnemySettings.Speed);
		var pathGenInstance = GetNode("/root/PathGenInstance");
		var pathRouteSize = pathGenInstance.Call("get_path_route").AsGodotArray().Count;
		
		var distanceTravelledOnScreen = Mathf.Clamp(distanceTravelled, 0, pathRouteSize - 1);
		GetNode<PathFollow3D>("Path3D/PathFollow3D").Progress = distanceTravelledOnScreen;
		
		if (distanceTravelled > pathRouteSize - 1)
		{
			GetNode("EnemyStateChart").Call("send_event", "to_damaging_state");
		}
	}

	private async void _on_despawning_state_entered()
	{
		EmitSignal(SignalName.EnemyFinished);
		progressBar.Visible = false;
		var animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		animationPlayer.Play("despawn");
		await ToSignal(animationPlayer, AnimationPlayer.SignalName.AnimationFinished);
		GetNode("EnemyStateChart").Call("send_event", "to_remove_enemy_state");
	}

	private void _on_remove_enemy_state_entered()
	{
		QueueFree();
	}

	// MODIFIKASI: Ubah damage menjadi 2 per enemy
	private void _on_damaging_state_entered()
	{
		attackable = false;
		
		// Damage player ketika enemy mencapai akhir path dengan 2 damage
		var main = GetTree().CurrentScene as Main;
		if (main != null)
		{
			// Hard-code damage 2 per enemy (tidak tergantung EnemySettings)
			int damage = 2;
			main.TakeDamage(damage);
			GD.Print($"Enemy reached end! Player took {damage} damage");
		}
		
		GetNode("EnemyStateChart").Call("send_event", "to_despawning_state");
	}

	private async void _on_dying_state_entered()
	{
		attackable = false;
		
		// Award cash to player when enemy dies
		var main = GetTree().CurrentScene as Main;
		if (main != null)
		{
			int cashReward = 10; // Base cash reward per enemy
			main.AddCash(cashReward);
			GD.Print($"Enemy died - awarded ${cashReward} cash");
		}
		
		// Hide enemy visuals
		progressBar.Visible = false;
		GetNode<Node3D>("Path3D/PathFollow3D/Enemy").Visible = false;
		
		// Emit signal bahwa enemy selesai
		EmitSignal(SignalName.EnemyFinished);
		
		// Remove enemy setelah delay pendek
		GetTree().CreateTimer(0.5f).Timeout += () => QueueFree();
	}

	private Curve3D PathRouteToCurve3D()
	{
		var c3d = new Curve3D();
		var pathGenInstance = GetNode("/root/PathGenInstance");
		var pathRoute = pathGenInstance.Call("get_path_route").AsGodotArray();
		
		foreach (var element in pathRoute)
		{
			var point = element.AsVector2I();
			c3d.AddPoint(new Vector3(point.X, 0.25f, point.Y));
		}
		
		return c3d;
	}

	private void _on_area_3d_area_entered(Area3D area)
	{
		// Check if area is Projectile
		if (area is Projectile projectile)
		{
			enemyHealth -= projectile.Damage;
			progressBar.Value = enemyHealth;
			
			// Destroy projectile setelah hit
			projectile.QueueFree();
		}

		if (enemyHealth <= 0)
		{
			GetNode("EnemyStateChart").Call("send_event", "to_dying_state");
		}
	}

	// Method untuk damage dari external (jika diperlukan)
	public void TakeDamage(int damage)
	{
		enemyHealth -= damage;
		progressBar.Value = enemyHealth;
		
		if (enemyHealth <= 0)
		{
			GetNode("EnemyStateChart").Call("send_event", "to_dying_state");
		}
	}
	
	// MODIFIKASI: Update method ini juga jika masih digunakan
	private async void _on_reaching_end_state_entered()
	{
		// Damage player ketika enemy mencapai Tile Start dengan 2 damage
		var main = GetTree().CurrentScene as Main;
		if (main != null)
		{
			// Hard-code damage 2 per enemy
			int damage = 2;
			main.TakeDamage(damage);
			GD.Print($"Enemy reached end! Player took {damage} damage");
		}
		
		// Hide enemy
		attackable = false;
		progressBar.Visible = false;
		GetNode<Node3D>("Path3D/PathFollow3D/Enemy").Visible = false;
		
		// Emit signal bahwa enemy selesai
		EmitSignal(SignalName.EnemyFinished);
		
		// Remove enemy after short delay
		await ToSignal(GetTree().CreateTimer(0.5f), SceneTreeTimer.SignalName.Timeout);
		QueueFree();
	}
}
