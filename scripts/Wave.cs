using Godot;
using Godot.Collections;

[System.Serializable]
public partial class Wave : Resource
{
	[Export] public Array<EnemySettings> Enemies { get; set; } = new Array<EnemySettings>();
}
