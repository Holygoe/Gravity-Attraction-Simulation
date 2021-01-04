using UnityEngine;

public readonly struct Burst
{
    public readonly Vector3 Position;
    public readonly float Power;
    public readonly Vector3 Velocity;

    public Burst(Vector3 position, float power, Vector3 velocity)
    {
        Position = position;
        Power = power;
        Velocity = velocity;
    }
}