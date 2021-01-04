using UnityEngine;

public readonly struct Point
{
    private readonly GravitySystem _gravitySystem;
    private readonly int _index;

    public bool IsInCore
    {
        get => _gravitySystem.CoreData [_index];
        set => _gravitySystem.CoreData [_index] = value;
    }

    public float Mass
    {
        get => _gravitySystem.MassData [_index];
        set => _gravitySystem.MassData [_index] = value;
    }

    public Vector3 Position
    {
        get => _gravitySystem.Particles[_index].position;
        set => _gravitySystem.Particles[_index].position = value;
    }

    public float Size
    {
        get => _gravitySystem.Particles[_index].startSize;
        set => _gravitySystem.Particles[_index].startSize = value;
    }

    public Vector3 Velocity
    {
        get => _gravitySystem.Particles[_index].velocity;
        set => _gravitySystem.Particles[_index].velocity = value;
    }

    public Point(GravitySystem gravitySystem, int index)
    {
        _index = index;
        _gravitySystem = gravitySystem;
    }

    public void ResetToZero()
    {
        _gravitySystem.Particles[_index].remainingLifetime = 0;
        Mass = 0;
        IsInCore = false;
    }
}