using UnityEngine;

public readonly struct Burst
{
    public readonly bool IsEmpty;
    public readonly Vector3 Position;
    public readonly float Power;
    public readonly Vector3 Velocity;
    
    public static readonly Burst Empty = new Burst(true);

    public Burst(Vector3 position, float power, Vector3 velocity)
    {
        IsEmpty = false;
        Position = position;
        Power = power;
        Velocity = velocity;
    }

    private Burst(bool isEmpty)
    {
        IsEmpty = isEmpty;
        Position = default;
        Power = default;
        Velocity = default;
    }
}