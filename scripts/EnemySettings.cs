using Godot;
using System;

[System.Serializable]
public partial class EnemySettings : Resource
{
	[Export] public float Speed { get; set; } = 1.0f;
	[Export] public float Health { get; set; } = 100.0f;
	[Export] public float Damage { get; set; } = 5.0f;
	[Export] public PackedScene EnemyScene { get; set; }
	[Export] public float NextEnemyDelay { get; set; } = 2.5f;
	[Export] public int Value { get; set; } = 50;
}
