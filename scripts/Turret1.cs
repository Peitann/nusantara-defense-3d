using Godot;
using System.Collections.Generic;

public partial class Turret1 : Node3D
{
	private List<Node3D> enemiesInRange = new List<Node3D>();
	private Node3D currentEnemy = null;
	private BasicEnemy currentEnemyClass = null;
	private bool currentEnemyTargetted = false;
	private float acquireSlerpProgress = 0;

	private long lastFireTime;
	[Export] public int FireRateMs { get; set; } = 1000;
	[Export] public PackedScene ProjectileType { get; set; }

	public void _on_patrol_zone_area_entered(Area3D area)
	{
		GD.Print($"Enemy entered patrol zone: {area.Name}");
		if (currentEnemy == null)
		{
			currentEnemy = area;
		}
		enemiesInRange.Add(area);
	}

	public void _on_patrol_zone_area_exited(Area3D area)
	{
		GD.Print($"Enemy exited patrol zone: {area.Name}");
		enemiesInRange.Remove(area);
		
		// Jika current enemy keluar, cari yang baru
		if (currentEnemy == area)
		{
			currentEnemy = enemiesInRange.Count > 0 ? enemiesInRange[0] : null;
		}
	}

	public void SetPatrolling(bool patrolling)
	{
		GetNode<Area3D>("PatrolZone").Monitoring = patrolling;
		GD.Print($"Turret patrolling set to: {patrolling}");
	}

	private void RotateTowardsTarget(Node3D rtarget, double delta)
	{
		var cannon = GetNode<Node3D>("Cannon");
		var targetVector = cannon.GlobalPosition.DirectionTo(
			new Vector3(rtarget.GlobalPosition.X, GlobalPosition.Y, rtarget.GlobalPosition.Z));
		var targetBasis = Basis.LookingAt(targetVector);
		cannon.Basis = cannon.Basis.Slerp(targetBasis, acquireSlerpProgress);
		
		acquireSlerpProgress += (float)delta;
		
		if (acquireSlerpProgress > 1)
		{
			GetNode("StateChart").Call("send_event", "to_attacking_state");
		}
	}

	private BasicEnemy FindEnemyParent(Node n)
	{
		if (n is BasicEnemy enemy)
		{
			return enemy;
		}
		else if (n.GetParent() != null)
		{
			return FindEnemyParent(n.GetParent());
		}
		else
		{
			return null;
		}
	}

	private void _on_patrolling_state_state_processing(double delta)
	{
		if (enemiesInRange.Count > 0)
		{
			currentEnemy = enemiesInRange[0];
			currentEnemyClass = FindEnemyParent(currentEnemy);
			GD.Print($"Found enemy: {currentEnemy?.Name}, switching to acquiring");
			GetNode("StateChart").Call("send_event", "to_acquiring_state");
		}
	}

	private void _remove_current_enemy()
	{
		GD.Print("Enemy finished");
		enemiesInRange.Remove(currentEnemy);
	}

	private void _on_acquiring_state_state_entered()
	{
		currentEnemyTargetted = false;
		acquireSlerpProgress = 0;
		GD.Print("Entering acquiring state");
	}

	private void _on_acquiring_state_state_physics_processing(double delta)
	{
		if (currentEnemy != null && enemiesInRange.Contains(currentEnemy))
		{
			if (currentEnemyClass != null && currentEnemyClass.attackable)
			{
				RotateTowardsTarget(currentEnemy, delta);
			}
			else
			{
				enemiesInRange.Remove(currentEnemy);
				currentEnemy = null;
				GetNode("StateChart").Call("send_event", "to_patrolling_state");
			}
		}
		else
		{
			currentEnemy = null;
			GetNode("StateChart").Call("send_event", "to_patrolling_state");
		}
	}

	private void _on_attacking_state_state_entered()
	{
		lastFireTime = 0;
		GD.Print("Entering attacking state");
	}

	private void _on_attacking_state_state_physics_processing(double delta)
	{
		if (currentEnemy != null && currentEnemyClass != null && 
			currentEnemyClass.attackable && enemiesInRange.Contains(currentEnemy))
		{
			GetNode<Node3D>("Cannon").LookAt(currentEnemy.GlobalPosition);
			MaybeFire();
		}
		else
		{
			currentEnemy = null;
			GetNode("StateChart").Call("send_event", "to_patrolling_state");
		}
	}

	private void MaybeFire()
	{
		if ((long)Time.GetTicksMsec() > (lastFireTime + FireRateMs))
		{
			GD.Print("Firing projectile!");
			
			var projectile = ProjectileType.Instantiate() as Projectile;
			if (projectile != null)
			{
				var spawnPoint = GetNode<Node3D>("Cannon/projectile_spawn");
				
				// Set projectile properties seperti di GDScript
				projectile.StartingPosition = spawnPoint.GlobalPosition;
				projectile.Target = currentEnemy;
				
				// Add projectile sebagai child dari turret (seperti di GDScript)
				AddChild(projectile);
				
				lastFireTime = (long)Time.GetTicksMsec();
				GD.Print($"Projectile fired at {currentEnemy.Name}");
			}
			else
			{
				GD.Print("Failed to instantiate projectile");
			}
		}
	}
}
