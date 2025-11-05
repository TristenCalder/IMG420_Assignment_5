using Godot;

namespace Assignment5;

public partial class Player : CharacterBody2D
{
    [Export] public float Speed { get; set; } = 200f;

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        Vector2 input = Vector2.Zero;
        if (Input.IsActionPressed("ui_right"))
        {
            input.X += 1f;
        }
        if (Input.IsActionPressed("ui_left"))
        {
            input.X -= 1f;
        }
        if (Input.IsActionPressed("ui_down"))
        {
            input.Y += 1f;
        }
        if (Input.IsActionPressed("ui_up"))
        {
            input.Y -= 1f;
        }

        input = input.Normalized();
        Velocity = input * Speed;
        MoveAndSlide();

        for (int i = 0; i < GetSlideCollisionCount(); i++)
        {
            var collision = GetSlideCollision(i);
            if (collision.GetCollider() is RigidBody2D rigidBody)
            {
                Vector2 pushDirection = -collision.GetNormal();
                float impulseStrength = Speed * (float)delta * 6.0f;
                Vector2 impulse = pushDirection * impulseStrength;
                rigidBody.ApplyCentralImpulse(impulse);
                rigidBody.Sleeping = false;
            }
        }
    }
}
