using Godot;

[System.Serializable]
public partial class PathGeneratorConfig : Resource
{
	[Export] public int MapLength { get; set; } = 20;
	[Export] public int MapHeight { get; set; } = 11;
	[Export] public int MinPathSize { get; set; } = 20;
	[Export] public int MaxPathSize { get; set; } = 50;
	[Export] public bool AddLoops { get; set; } = true;
	[Export] public int MinLoops { get; set; } = 0;
	[Export] public int MaxLoops { get; set; } = 3;
}
