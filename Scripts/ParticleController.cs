using Godot;

namespace Assignment5;

public partial class ParticleController : GpuParticles2D
{
	private const string ShaderPath = "res://Shaders/custom_particle.gdshader";

	private ShaderMaterial? _shaderMaterial;
	private float _timeAccumulator;

	public override void _Ready()
	{
		base._Ready();

		SetProcess(true);
		SetupParticles();
		SetupShader();
		Emitting = true;
	}

	private void SetupParticles()
	{
		Amount = 220;
		Lifetime = 2.6f;
		OneShot = false;
		Preprocess = 0.8f;
		SpeedScale = 1.0f;

		var processMaterial = ProcessMaterial as ParticleProcessMaterial ?? new ParticleProcessMaterial();
		processMaterial.Direction = new Vector3(0, -1, 0);
		processMaterial.Spread = 45.0f;
		processMaterial.InitialVelocityMin = 30.0f;
		processMaterial.InitialVelocityMax = 60.0f;
		processMaterial.Gravity = new Vector3(0, 120, 0);
		processMaterial.ScaleMin = 0.8f;
		processMaterial.ScaleMax = 1.2f;
		ProcessMaterial = processMaterial;

		if (Texture == null)
		{
			var image = Image.Create(8, 8, false, Image.Format.Rgba8);
			image.Fill(new Color(1, 1, 1, 1));
			var texture = ImageTexture.CreateFromImage(image);
			Texture = texture;
		}
	}

	private void SetupShader()
	{
		if (ResourceLoader.Exists(ShaderPath))
		{
			var shader = ResourceLoader.Load<Shader>(ShaderPath);
			if (shader != null)
			{
				_shaderMaterial = new ShaderMaterial
				{
					Shader = shader
				};
				Material = _shaderMaterial;
			}
			else
			{
				GD.PushWarning($"Shader at '{ShaderPath}' could not be loaded.");
			}
		}
		else
		{
			GD.PushWarning($"Shader path '{ShaderPath}' does not exist.");
		}
	}

	public override void _Process(double delta)
	{
		base._Process(delta);

		if (_shaderMaterial == null)
		{
			return;
		}

		_timeAccumulator += (float)delta;
		_shaderMaterial.SetShaderParameter("custom_time", _timeAccumulator);

		var wave = 0.08f + 0.05f * Mathf.Sin(_timeAccumulator * 1.5f);
		_shaderMaterial.SetShaderParameter("wave_intensity", wave);

		var gradientOffset = 0.2f + 0.2f * Mathf.Sin(_timeAccumulator);
		_shaderMaterial.SetShaderParameter("gradient_offset", gradientOffset);
	}
}
