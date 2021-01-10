using UnityEngine;

public readonly struct Burst
{
    public readonly Vector3 Position;
    public readonly float Energy;
    public readonly Vector3 Velocity;

    public Burst(Vector3 position, float energy, Vector3 velocity)
    {
        Position = position;
        Energy = energy;
        Velocity = velocity;
    }
}