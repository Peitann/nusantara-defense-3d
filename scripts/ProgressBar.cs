using Godot;

public partial class ProgressBar : Godot.ProgressBar
{
	private StyleBoxFlat healthStylebox;
	private Gradient healthBarGradient;

	public override void _Ready()
	{
		try
		{
			healthStylebox = GetThemeStylebox("fill") as StyleBoxFlat;
			healthBarGradient = GD.Load<Gradient>("res://resources/health_bar_colours.res");
			
			// Connect value changed signal
			ValueChanged += OnValueChanged;
			
			// Set initial color
			UpdateHealthColor();
			
			GD.Print("ProgressBar initialized successfully");
		}
		catch (System.Exception e)
		{
			GD.PrintErr($"Error initializing ProgressBar: {e.Message}");
		}
	}

	private void _on_value_changed(double newValue)
	{
		OnValueChanged(newValue);
	}

	private void OnValueChanged(double newValue)
	{
		UpdateHealthColor();
	}
	
	private void UpdateHealthColor()
	{
		if (healthBarGradient != null && healthStylebox != null && MaxValue > 0)
		{
			float healthPercentage = (float)(Value / MaxValue);
			Color newColor = healthBarGradient.Sample(healthPercentage);
			healthStylebox.BgColor = newColor;
			
			GD.Print($"Health bar color updated: {healthPercentage:P0} - {newColor}");
		}
	}
	
	// Method untuk force update dari luar
	public void ForceUpdateColor()
	{
		UpdateHealthColor();
	}
}
