using Godot;

public partial class ActionButton : Button
{
	[Export] public Texture2D ButtonIcon { get; set; }
	[Export] public PackedScene ButtonObject { get; set; }

	private Camera3D cam;
	private Node3D actionObject;

	private const int RAYCAST_LENGTH = 100;

	private bool _isDragging = false;
	private bool _isValidLocation = false;
	private Vector3 _lastValidLocation;

	private float _dragAlpha = 0.5f;

	private BaseMaterial3D errorMat;

	public override void _Ready()
	{
		Icon = ButtonIcon;
		actionObject = ButtonObject.Instantiate() as Node3D;
		AddChild(actionObject);
		actionObject.Visible = false;
		cam = GetViewport().GetCamera3D();
		
		errorMat = GD.Load<BaseMaterial3D>("res://materials/red_transparent.material");
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_isDragging)
		{
			var spaceState = actionObject.GetWorld3D().DirectSpaceState;
			var mousePos = GetViewport().GetMousePosition();
			var origin = cam.ProjectRayOrigin(mousePos);
			var end = origin + cam.ProjectRayNormal(mousePos) * RAYCAST_LENGTH;
			
			var query = PhysicsRayQueryParameters3D.Create(origin, end);
			query.CollideWithAreas = true;
			var rayResult = spaceState.IntersectRay(query);
			
			if (rayResult.Count > 0)
			{
				var collisionObject = rayResult["collider"].AsGodotObject() as CollisionObject3D;
				OnMainMouseHit(collisionObject);
			}
			else
			{
				actionObject.Visible = false;
				_isValidLocation = false;
			}
		}
	}

	private void SetChildMeshAlphas(Node node)
	{
		foreach (Node child in node.GetChildren())
		{
			if (child is MeshInstance3D meshInstance)
			{
				SetMeshAlpha(meshInstance);
			}

			if (child is Node && child.GetChildCount() > 0)
			{
				SetChildMeshAlphas(child);
			}
		}
	}

	private void SetMeshAlpha(MeshInstance3D meshInstance)
	{
		for (int surfaceIndex = 0; surfaceIndex < meshInstance.Mesh.GetSurfaceCount(); surfaceIndex++)
		{
			var material = meshInstance.Mesh.SurfaceGetMaterial(surfaceIndex).Duplicate(true) as BaseMaterial3D;
			meshInstance.SetSurfaceOverrideMaterial(surfaceIndex, material);
			var overrideMaterial = meshInstance.GetSurfaceOverrideMaterial(surfaceIndex) as BaseMaterial3D;
			overrideMaterial.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
			overrideMaterial.AlbedoColor = new Color(overrideMaterial.AlbedoColor.R,
												   overrideMaterial.AlbedoColor.G,
												   overrideMaterial.AlbedoColor.B,
												   _dragAlpha);
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
			meshInstance.SetSurfaceOverrideMaterial(surfaceIndex, errorMat);
		}
	}

	private void ClearMaterialOverrides(Node node)
	{
		foreach (Node child in node.GetChildren())
		{
			if (child is MeshInstance3D meshInstance)
			{
				ClearMaterialOverride(meshInstance);
			}

			if (child is Node && child.GetChildCount() > 0)
			{
				ClearMaterialOverrides(child);
			}
		}
	}

	private void ClearMaterialOverride(MeshInstance3D meshInstance)
	{
		for (int surfaceIndex = 0; surfaceIndex < meshInstance.Mesh.GetSurfaceCount(); surfaceIndex++)
		{
			meshInstance.SetSurfaceOverrideMaterial(surfaceIndex, null);
		}
	}

	private void OnMainMouseHit(CollisionObject3D tile)
	{
		actionObject.Visible = true;

		if (tile.GetGroups()[0].ToString().StartsWith("grid_empty"))
		{
			SetChildMeshAlphas(actionObject);
			actionObject.GlobalPosition = new Vector3(tile.GlobalPosition.X, 0.2f, tile.GlobalPosition.Z);
			_lastValidLocation = actionObject.GlobalPosition;
			_isValidLocation = true;
		}
		else
		{
			SetChildMeshError(actionObject);
			actionObject.GlobalPosition = new Vector3(tile.GlobalPosition.X, 0.2f, tile.GlobalPosition.Z);
			_isValidLocation = false;
		}
	}

	// Signal methods - sesuai dengan nama di scene
	private void _on_button_down()
	{
		_isDragging = true;
		_isValidLocation = false;
	}

	private void _on_button_up()
	{
		_isDragging = false;
		actionObject.Visible = false;

		if (_isValidLocation)
		{
			var newObject = ButtonObject.Instantiate() as Node3D;
			GetTree().CurrentScene.AddChild(newObject);
			newObject.GlobalPosition = _lastValidLocation;
		}
	}
}
