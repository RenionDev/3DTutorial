using Godot;
using System;

public partial class Mob : CharacterBody3D
{
    // Don't forget to rebuild the project so the editor knows about the new export variable.

    [Export]
    // Minimum speed of the mob in meters per second
    public int MinSpeed { get; set; } = 10;

    [Export]
    // Maximum speed of the mob in meters per second
    public int MaxSpeed { get; set; } = 18;

    [Signal]
    // Emitted when the player jumped on the mob.
    public delegate void SquashedEventHandler();
    
    public override void _PhysicsProcess(double delta)
    {
        MoveAndSlide();
    }

    // This function will be called from the Main scene.
    public void Initialize(Vector3 startPosition, Vector3 playerPosition)
    {
        // We position the mob by placing it at startPosition
        // and rotate it towards playerPosition, so it looks at the player.
        LookAtFromPosition(startPosition, playerPosition);
        // Rotate this mob randomly within range of -45 and +45 degrees,
        // so that it doesn't move directly towards the player.
        RotateY((float)GD.RandRange(-Mathf.Pi / 4, Mathf.Pi / 4));
        
        // We calculate a random speed (integer).
        int randomSpeed = GD.RandRange(MinSpeed, MaxSpeed);
        // We calculate a forward velocity that represents the speed.
        Velocity = Vector3.Forward * randomSpeed;
        // We then rotate the velocity vector based on the mob's Y rotation
        // in order to move in the direction the mob is looking.
        Velocity = Velocity.Rotated(Vector3.Up, Rotation.Y);
    }

    // We also specified this function name in PascalCase in the editor's connection window.
    private void OnVisibilityNotifierScreenExited()
    {
        QueueFree();
    }

    public void Squash()
    {
        EmitSignal(SignalName.Squashed);
        QueueFree();
    }
}
