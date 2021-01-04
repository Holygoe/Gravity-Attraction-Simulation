using UnityEngine;

public readonly struct Point
{
    private readonly GravitySystemData _gravitySystemData;
    private readonly int _index;

    public bool IsInCore
    {
        get => _gravitySystemData.CoreData [_index];
        set => _gravitySystemData.CoreData [_index] = value;
    }

    public float Mass
    {
        get => _gravitySystemData.MassData [_index];
        set => _gravitySystemData.MassData [_index] = value;
    }

    public Vector3 Position
    {
        get => _gravitySystemData.Particles[_index].position;
        set => _gravitySystemData.Particles[_index].position = value;
    }

    public float Size
    {
        get => _gravitySystemData.Particles[_index].startSize;
        set => _gravitySystemData.Particles[_index].startSize = value;
    }

    public Vector3 Velocity
    {
        get => _gravitySystemData.Particles[_index].velocity;
        set => _gravitySystemData.Particles[_index].velocity = value;
    }

    public Point(GravitySystemData gravitySystemData, int index)
    {
        _index = index;
        _gravitySystemData = gravitySystemData;
    }

    public void ResetToZero()
    {
        _gravitySystemData.Particles[_index].remainingLifetime = 0;
        Mass = 0;
        IsInCore = false;
    }
}