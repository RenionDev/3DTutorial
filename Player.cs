using Godot;
using System;

public partial class Player : CharacterBody3D
{
    // Don't forget to rebuild the project so the editor knows about the new export variable

    [Export]
    // How fast the player moves in meters per second.
    public int Speed { get; set; } = 14;

    // The downward acceleration when in the air, in meters per second squared.
    [Export] 
    public int FallAcceleration { get; set; } = 75; 
    
    // Vertical impulse applied to the character upon jumping in meters per second.
    [Export] 
    public int JumpImpulse { get; set; } = 20;
    
    // Vertical impulse applied to the character upon bouncing over a mob in meters per second.
    [Export] 
    public int BounceImpulse { get; set; } = 16;
    
    [Signal]
    // Emitted when the player was hit by a mob
    public delegate void HitEventHandler();

    private Vector3 _targetVelocity = Vector3.Zero;

    public override void _PhysicsProcess(double delta)
    {
        // We create a local variable to store the input direction.
        var direction = Vector3.Zero;
        
        // We check for each move input and update the direction accordingly.
        if (Input.IsActionPressed("move_right"))
        {
            direction.X += 1.0f;
        }
        if (Input.IsActionPressed("move_left"))
        {
            direction.X -= 1.0f;
        }
        if (Input.IsActionPressed("move_back"))
        {
            // Since we are working in 3D, the XZ plane is the ground plane. 
            // That's why we are using Z axis when moving back and forward.
            direction.Z += 1.0f;
        }
        if (Input.IsActionPressed("move_forward"))
        {
            direction.Z -= 1.0f;
        }

        if (direction != Vector3.Zero)
        {
                        // Speed up animation speed when moving
            GetNode<AnimationPlayer>("AnimationPlayer").SpeedScale = 4;
            direction = direction.Normalized();
            // Setting the basis property will affect the rotation of the node.
            GetNode<Node3D>("Pivot").Basis = Basis.LookingAt(direction);
            

        }
        else
        {
            // Reset animation speed when not moving
            GetNode<AnimationPlayer>("AnimationPlayer").SpeedScale = 1;
        }
        
        // Ground velocity
        _targetVelocity.X = direction.X * Speed;
        _targetVelocity.Z = direction.Z * Speed;
        
        // Vertical velocity
        if (!IsOnFloor()) // If in the air, fall towards the floor. Literally Gravity
        {
            _targetVelocity.Y -= FallAcceleration * (float)delta;
        }
        
        // Moving the character
        Velocity = _targetVelocity;
        
        // Jumping.
        if (IsOnFloor() && Input.IsActionJustPressed("jump"))
        {
            _targetVelocity.Y = JumpImpulse;
        }
        
        // Iterate through all collisions that occurred this frame.
        for (int index = 0; index < GetSlideCollisionCount(); index++)
        {
            // We get one of the collisions with the player.
            KinematicCollision3D collision = GetSlideCollision(index);
            
            // If the collision is with a mob.
            // With C# we leverage typing and pattern-matching
            // instead of checking for the group we created.
            if (collision.GetCollider() is Mob mob)
            {
                // We check that we are hitting it from above. We count it's from above
                // if the angle of the hit is more than cos(0.2) (36degrees)
                if (Vector3.Up.Dot(collision.GetNormal()) > 0.2f)
                {
                    // If so, we squash it and bounce.
                    mob.Squash();
                    _targetVelocity.Y = BounceImpulse;
                    // Prevent further duplicate calls.
                    break;
                }
            }
        }
        
        MoveAndSlide();
        var pivot = GetNode<Node3D>("Pivot");
        pivot.Rotation = new Vector3(Mathf.Pi / 6.0f * Velocity.Y / JumpImpulse, pivot.Rotation.Y, pivot.Rotation.Z);

    }
    
    private void Die()
    {
        EmitSignal(SignalName.Hit);
        QueueFree();
    }
    
    // We also specified this function name in PascalCase in the editor's connection window
    private void OnMobDetectorBodyEntered(Node3D body)
    {
        Die();
    }
}
