using Godot;

public partial class Projectile : Area3D
{
	public Vector3 StartingPosition { get; set; }
	public Node3D Target { get; set; }

	[Export] public float Speed { get; set; } = 2.0f; // metres per second
	[Export] public int Damage { get; set; } = 25;
	private float lerpPos = 0;

	public override void _Ready()
	{
		GlobalPosition = StartingPosition;
	}

	public override void _Process(double delta)
	{
		if (Target != null && lerpPos < 1)
		{
			GlobalPosition = StartingPosition.Lerp(Target.GlobalPosition, lerpPos);
			lerpPos += (float)delta * Speed;
		}
		else
		{
			QueueFree();
		}
	}

	// Method untuk setting dari turret
	public void SetStartingPosition(Vector3 position)
	{
		StartingPosition = position;
	}

	public void SetTarget(Node3D target)
	{
		Target = target;
	}

	public void SetDamage(int damage)
	{
		Damage = damage;
	}
}
