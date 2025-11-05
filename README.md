# Assignment 5 – Rendering and Physics (Godot 4.5)

This Godot 4.5 C# project implements the three required systems from the assignment brief:

- **Custom particle shader** with animated color gradient and wave distortion.
- **Rigid-body chain** connected via pin joints that reacts to forces and the player.
- **Laser detector** that raycasts, visualises the beam, and raises an alarm when the player crosses it.

## Project Structure

- `project.godot` – Godot project configuration (main scene set to `Scenes/Main.tscn`).
- `Assignment5.csproj` / `Assignment5.sln` – .NET project used by the C# scripts.
- `Scenes/` – Godot scenes:
  - `Main.tscn` – Root scene instancing the particle system, physics demo, laser system, and controllable player.
  - `ParticleSystem.tscn` – Hosts the GPU particle emitter that uses the custom shader.
  - `PhysicsDemo.tscn` – Spawns the rope/chain segments and connects them via pin joints.
  - `ChainSegment.tscn` – Individual chain link geometry and collision data.
  - `LaserSystem.tscn` – Raycasting laser guard node.
  - `Player.tscn` – CharacterBody2D with collision and simple movement controls.
- `Scripts/` – C# scripts implementing the systems.
- `Shaders/custom_particle.gdshader` – Shader used by the particle system.

## Feature Notes

### Shader & Particles
- `Scripts/ParticleController.cs` configures a `GpuParticles2D` emitter and assigns the shader.
- `Shaders/custom_particle.gdshader` performs:
  - Time-driven horizontal wave distortion (`sin` on UV).
  - UV-based color gradient between configurable start/end colours.
  - Animated gradient offset to keep the particles in motion.
- Shader parameters are updated every frame to animate the gradient and wave strength.

### Physics Chain
- `Scripts/PhysicsChain.cs` spawns the requested number of chain segments (default 6) and connects each neighbour with a `PinJoint2D`.
- The first joint pins the chain to a static anchor body to keep it suspended.
- Force can be injected with `ApplyForceToSegment`, which uses impulses on individual `RigidBody2D` links.

### Raycasting Laser
- `Scripts/LaserDetector.cs` creates a `RayCast2D` and `Line2D` at runtime.
- The beam length, colours, and detection target can be configured in the inspector.
- When the ray collides with the player (looked up via export path or `"player"` group) the beam flashes red, pulses its width, and prints an alarm message.

### Player
- `Scripts/Player.cs` reads the default Godot input actions (`ui_left`, `ui_right`, `ui_up`, `ui_down`) and uses `MoveAndSlide` for smooth 2D movement.

## Usage

1. Open the folder in Godot 4.5 with C# support enabled.
2. Let Godot restore/build the `Assignment5.csproj`.
3. Run the main scene (`Scenes/Main.tscn`).
4. Use arrow keys or WASD (mapped to the same actions) to move the player into the chain and laser to test the interactions.

## Shader Explanation

The shader mixes two colours based on the particle UV Y coordinate, adds an adjustable offset, and multiplies the result by the particle texture. A sinusoidal offset driven by a custom time uniform shifts the UV X coordinate, creating the wave effect. The controller script updates `custom_time`, `wave_intensity`, and `gradient_offset` each frame to animate the effect.

## Physics Parameters

- Link spacing (`SegmentDistance`) defaults to `32`, giving a fairly loose rope.
- Pin joints use mild softness, damping, and bias to stabilise the chain without making it rigid.
- Chain segments run on collision layer 2, while the player remains on layer 1, allowing interactions while keeping the laser focused on the player layer.

## Raycast Detection

- The laser casts forward every physics frame, updating the `Line2D` end point with either the full length or the detected collision position.
- The collision mask is set to player layer (1) by default so only the player triggers the alarm.
- The alarm state auto-resets after 1.5 seconds via a `Timer`, returning the beam to its idle colour and width.

## Next Steps

- Replace placeholder visuals with bespoke sprites and effects.
- Add UI/audio feedback for the alarm using Godot `AudioStreamPlayer2D` and `Label`.
- Extend the player controller with jump/dash or integrate it into a larger gameplay scene.
