using Godot;
using System.Collections.Generic;

namespace Assignment5;

public partial class PhysicsChain : Node2D
{
	[Export] public int ChainSegments { get; set; } = 5;
	[Export] public float SegmentDistance { get; set; } = 30f;
	[Export] public PackedScene? SegmentScene { get; set; }

	private const string DefaultSegmentScenePath = "res://Scenes/ChainSegment.tscn";

	private readonly List<RigidBody2D> _segments = new();
	private readonly List<Joint2D> _joints = new();
	private StaticBody2D? _anchor;

	public override void _Ready()
	{
		base._Ready();

		if (SegmentScene == null && ResourceLoader.Exists(DefaultSegmentScenePath))
		{
			SegmentScene = ResourceLoader.Load<PackedScene>(DefaultSegmentScenePath);
		}

		CreateChain();
	}

	private void ClearChain()
	{
		foreach (var joint in _joints)
		{
			joint.QueueFree();
		}
		_joints.Clear();

		foreach (var segment in _segments)
		{
			segment.QueueFree();
		}
		_segments.Clear();

		_anchor?.QueueFree();
		_anchor = null;
	}

	private void CreateChain()
	{
		ClearChain();

		if (SegmentScene == null)
		{
			GD.PushWarning("PhysicsChain: SegmentScene is not assigned.");
			return;
		}

		_anchor = new StaticBody2D
		{
			Name = "Anchor",
			Position = Vector2.Zero
		};
		AddChild(_anchor);

		var anchorShape = new CollisionShape2D
		{
			Shape = new CircleShape2D
			{
				Radius = 6f
			}
		};
		_anchor.AddChild(anchorShape);

		var anchorVisual = new Polygon2D
		{
			Color = new Color(0.2f, 0.6f, 0.9f),
			Polygon = new Vector2[]
			{
				new(-6, -6),
				new(6, -6),
				new(6, 6),
				new(-6, 6)
			}
		};
		_anchor.AddChild(anchorVisual);

		RigidBody2D? previousSegment = null;

		for (int i = 0; i < Mathf.Max(ChainSegments, 1); i++)
		{
			var segment = SegmentScene.Instantiate<RigidBody2D>();
			if (segment == null)
			{
				GD.PushWarning("PhysicsChain: Segment scene must inherit from RigidBody2D.");
				continue;
			}

			segment.Name = $"Segment_{i}";
			segment.Position = new Vector2(0, (i + 1) * SegmentDistance);
			AddChild(segment);
			_segments.Add(segment);

			PinJoint2D joint = new()
			{
				Name = $"Joint_{i}",
				Bias = 0.3f,
				Softness = 0.05f
			};

			Vector2 jointPosition;
			if (i == 0)
			{
				joint.NodeA = _anchor.GetPath();
				joint.NodeB = segment.GetPath();
				jointPosition = new Vector2(0, SegmentDistance * 0.5f);
			}
			else if (previousSegment != null)
			{
				joint.NodeA = previousSegment.GetPath();
				joint.NodeB = segment.GetPath();
				jointPosition = (previousSegment.Position + segment.Position) * 0.5f;
			}
			else
			{
				continue;
			}

			joint.Position = jointPosition;

			AddChild(joint);
			_joints.Add(joint);
			segment.Sleeping = false;
			previousSegment = segment;
		}

		if (_segments.Count > 0)
		{
			_segments[^1].ApplyCentralImpulse(new Vector2(80, 0));
		}
	}

	public void ApplyForceToSegment(int segmentIndex, Vector2 force)
	{
		if (segmentIndex < 0 || segmentIndex >= _segments.Count)
		{
			GD.PushWarning("PhysicsChain: Segment index out of range.");
			return;
		}

		_segments[segmentIndex].ApplyCentralImpulse(force);
	}
}
