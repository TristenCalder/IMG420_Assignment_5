using Godot;

namespace Assignment5;

public partial class LaserDetector : Node2D
{
    [Export] public float LaserLength { get; set; } = 500f;
    [Export] public Color LaserColorNormal { get; set; } = Colors.Green;
    [Export] public Color LaserColorAlert { get; set; } = Colors.Red;
    [Export] public NodePath PlayerPath { get; set; }

    private const float BaseLaserWidth = 3.0f;

    private RayCast2D? _rayCast;
    private Line2D? _laserBeam;
    private Node2D? _player;
    private bool _isAlarmActive;
    private Timer? _alarmTimer;
    private float _alarmPulseTime;
    private Node2D? _impactMarker;

    public override void _Ready()
    {
        base._Ready();
        SetupRaycast();
        SetupVisuals();
        AcquirePlayer();
        SetupAlarmTimer();
    }

    private void AcquirePlayer()
    {
        if (!string.IsNullOrEmpty(PlayerPath))
        {
            _player = GetNodeOrNull<Node2D>(PlayerPath);
        }

        if (_player == null)
        {
            _player = GetTree().GetFirstNodeInGroup("player") as Node2D;
        }
    }

    private void SetupAlarmTimer()
    {
        _alarmTimer = new Timer
        {
            Name = "AlarmTimer",
            WaitTime = 1.5,
            OneShot = true,
            Autostart = false
        };
        _alarmTimer.Timeout += ResetAlarm;
        AddChild(_alarmTimer);
    }

    private void SetupRaycast()
    {
        _rayCast = new RayCast2D
        {
            Name = "LaserRayCast",
            TargetPosition = new Vector2(LaserLength, 0),
            CollisionMask = 1, // Detect player layer by default
            CollideWithAreas = false,
            Enabled = true
        };
        AddChild(_rayCast);
    }

    private void SetupVisuals()
    {
        _laserBeam = new Line2D
        {
            Name = "LaserBeam",
            Width = BaseLaserWidth,
            DefaultColor = LaserColorNormal,
            TextureMode = Line2D.LineTextureMode.Stretch
        };
        _laserBeam.Points = new Vector2[] { Vector2.Zero, new(LaserLength, 0) };
        AddChild(_laserBeam);

        _impactMarker = new Node2D
        {
            Name = "ImpactMarker",
            Visible = false
        };
        var markerVisual = new Polygon2D
        {
            Color = new Color(1, 1, 1, 0.9f),
            Polygon = new Vector2[]
            {
                new(-4, 0),
                new(0, 4),
                new(4, 0),
                new(0, -4)
            }
        };
        _impactMarker.AddChild(markerVisual);
        AddChild(_impactMarker);
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        if (_player == null || !_player.IsInsideTree())
        {
            AcquirePlayer();
        }

        if (_rayCast == null || _laserBeam == null)
        {
            return;
        }

        _rayCast.TargetPosition = new Vector2(LaserLength, 0);
        _rayCast.ForceRaycastUpdate();

        UpdateLaserBeam();

        if (_isAlarmActive && _laserBeam != null)
        {
            _alarmPulseTime += (float)delta;
            float pulse = 0.75f + 0.25f * Mathf.Sin(_alarmPulseTime * 10.0f);
            _laserBeam.Width = BaseLaserWidth * pulse;
        }
        else if (_laserBeam != null)
        {
            _laserBeam.Width = BaseLaserWidth;
            _alarmPulseTime = 0f;
        }

        if (_rayCast.IsColliding())
        {
            GodotObject? collider = _rayCast.GetCollider();
            if (collider != null && _player != null && collider == _player)
            {
                TriggerAlarm();
            }
        }
    }

    private void UpdateLaserBeam()
    {
        if (_rayCast == null || _laserBeam == null)
        {
            return;
        }

        Vector2 endPoint = new(LaserLength, 0);

        if (_rayCast.IsColliding())
        {
            var collisionPoint = ToLocal(_rayCast.GetCollisionPoint());
            endPoint = collisionPoint;
            if (_impactMarker != null)
            {
                _impactMarker.Visible = true;
                _impactMarker.Position = collisionPoint;
            }
        }
        else if (_impactMarker != null)
        {
            _impactMarker.Visible = false;
        }

        _laserBeam.Points = new[] { Vector2.Zero, endPoint };
    }

    private void TriggerAlarm()
    {
        if (_laserBeam == null || _isAlarmActive)
        {
            return;
        }

        _isAlarmActive = true;
        _laserBeam.DefaultColor = LaserColorAlert;
        Modulate = Colors.White;
        GD.Print("ALARM! Player detected!");
        _alarmTimer?.Start();
    }

    private void ResetAlarm()
    {
        if (_laserBeam == null)
        {
            return;
        }

        _isAlarmActive = false;
        _laserBeam.DefaultColor = LaserColorNormal;
        Modulate = Colors.White;
    }
}
