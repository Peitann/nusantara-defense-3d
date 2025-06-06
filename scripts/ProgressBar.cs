using Godot;

public partial class ProgressBar : Godot.ProgressBar
{
	private StyleBoxFlat healthStylebox;
	private Gradient healthBarGradient;

	public override void _Ready()
	{
		healthStylebox = GetThemeStylebox("fill") as StyleBoxFlat;
		healthBarGradient = GD.Load<Gradient>("res://resources/health_bar_colours.res");
		
		// Connect value changed signal
		ValueChanged += OnValueChanged;
	}

	private void _on_value_changed(double newValue)
	{
		OnValueChanged(newValue);
	}

	private void OnValueChanged(double newValue)
	{
		if (healthBarGradient != null && healthStylebox != null)
		{
			healthStylebox.BgColor = healthBarGradient.Sample((float)(newValue / MaxValue));
		}
	}
}
