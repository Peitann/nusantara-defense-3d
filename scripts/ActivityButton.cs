using Godot;

public partial class ActivityButton : Button
{
	[Export] public Texture2D ActivityButtonIcon { get; set; }
	[Export] public PackedScene ActivityDraggable { get; set; }
	[Export] public int Cost { get; set; } = 100;

	private Main main; // Ubah tipe ke Main
	private Camera3D _cam;
	private Node3D _draggable;
	private BaseMaterial3D _errorMat;

	private const int RAYCAST_LENGTH = 100;

	private bool _isDragging = false;
	private bool _isValidLocation = false;
	private Vector3 _lastValidLocation;

	public override void _Ready()
	{
		// Akses Main script dengan benar
		main = GetNode("../..") as Main;
		if (main == null)
		{
			main = GetTree().CurrentScene as Main;
		}
		
		Icon = ActivityButtonIcon;
		_draggable = ActivityDraggable.Instantiate() as Node3D;
		
		// Set patrolling jika ada method tersebut
		if (_draggable.HasMethod("SetPatrolling"))
		{
			_draggable.Call("SetPatrolling", false);
		}
		
		AddChild(_draggable);
		_draggable.Visible = false;
		_cam = GetViewport().GetCamera3D();
		
		_errorMat = GD.Load<BaseMaterial3D>("res://materials/red_transparent.material");
	}

	public override void _Process(double delta)
	{
		// Check cash dengan akses langsung ke Main
		if (main != null)
		{
			Disabled = main.Cash < Cost;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_isDragging)
		{
			var spaceState = _draggable.GetWorld3D().DirectSpaceState;
			var mousePos = GetViewport().GetMousePosition();
			var origin = _cam.ProjectRayOrigin(mousePos);
			var end = origin + _cam.ProjectRayNormal(mousePos) * RAYCAST_LENGTH;
			
			// Convert binary string to collision mask
			var collisionMask = System.Convert.ToUInt32("100111", 2);
			var query = PhysicsRayQueryParameters3D.Create(origin, end, collisionMask);
			query.CollideWithAreas = true;
			var rayResult = spaceState.IntersectRay(query);
			
			if (rayResult.Count > 0)
			{
				var collisionObject = rayResult["collider"].AsGodotObject() as CollisionObject3D;
				if (collisionObject.GetGroups().Count > 0 && 
					collisionObject.GetGroups()[0].ToString() == "grid_empty")
				{
					_draggable.Visible = true;
					_isValidLocation = true;
					_lastValidLocation = new Vector3(collisionObject.GlobalPosition.X, 0.2f, collisionObject.GlobalPosition.Z);
					_draggable.GlobalPosition = _lastValidLocation;
					ClearChildMeshError(_draggable);
				}
				else
				{
					_draggable.Visible = true;
					_draggable.GlobalPosition = new Vector3(collisionObject.GlobalPosition.X, 0.2f, collisionObject.GlobalPosition.Z);
					_isValidLocation = false;
					SetChildMeshError(_draggable);
				}
			}
			else
			{
				_draggable.Visible = false;
			}
		}
	}

	private void SetChildMeshError(Node node)
	{
		foreach (Node child in node.GetChildren())
		{
			if (child is MeshInstance3D meshInstance)
			{
				SetMeshError(meshInstance);
			}
			if (child is Node && child.GetChildCount() > 0)
			{
				SetChildMeshError(child);
			}
		}
	}

	private void SetMeshError(MeshInstance3D meshInstance)
	{
		for (int surfaceIndex = 0; surfaceIndex < meshInstance.Mesh.GetSurfaceCount(); surfaceIndex++)
		{
			meshInstance.SetSurfaceOverrideMaterial(surfaceIndex, _errorMat);
		}
	}

	private void ClearChildMeshError(Node node)
	{
		foreach (Node child in node.GetChildren())
		{
			if (child is MeshInstance3D meshInstance)
			{
				ClearMeshError(meshInstance);
			}
			if (child is Node && child.GetChildCount() > 0)
			{
				ClearChildMeshError(child);
			}
		}
	}

	private void ClearMeshError(MeshInstance3D meshInstance)
	{
		for (int surfaceIndex = 0; surfaceIndex < meshInstance.Mesh.GetSurfaceCount(); surfaceIndex++)
		{
			meshInstance.SetSurfaceOverrideMaterial(surfaceIndex, null);
		}
	}

	// Signal methods
	private void _on_button_down()
	{
		_isDragging = true;
	}

	private void _on_button_up()
	{
		_isDragging = false;
		_draggable.Visible = false;

		if (_isValidLocation && main != null)
		{
			// Check jika cash cukup
			if (main.Cash >= Cost)
			{
				var activity = ActivityDraggable.Instantiate() as Node3D;
				GetTree().CurrentScene.AddChild(activity);
				activity.GlobalPosition = _lastValidLocation;
				
				// Set patrolling jika ada method tersebut
				if (activity.HasMethod("SetPatrolling"))
				{
					activity.Call("SetPatrolling", true);
				}
				
				// Kurangi cash dengan akses langsung
				main.Cash -= Cost;
				GD.Print($"Turret placed! Cash reduced by {Cost}. New cash: {main.Cash}");
			}
			else
			{
				GD.Print($"Not enough cash! Need {Cost}, have {main.Cash}");
			}
		}
	}
}
