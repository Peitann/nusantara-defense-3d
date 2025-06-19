using Godot;
using System;

[System.Serializable]
public partial class EnemySettings : Resource
{
	[Export] public int MaxHealth { get; set; } = 100;
	[Export] public float Speed { get; set; } = 1.0f;
	[Export] public int Cash { get; set; } = 25;
	[Export] public float NextEnemyDelay { get; set; } = 1.0f;
	[Export] public PackedScene EnemyScene { get; set; }

	// Ubah default damage menjadi 2
	[Export] public int Damage { get; set; } = 1; // Damage ke player

	// Untuk backward compatibility, tambahkan property Health yang redirect ke MaxHealth
	public int Health
	{
		get => MaxHealth;
		set => MaxHealth = value;
	}

	// Untuk backward compatibility, tambahkan property Value yang redirect ke Cash
	public int Value
	{
		get => Cash;
		set => Cash = value;
	}
}
