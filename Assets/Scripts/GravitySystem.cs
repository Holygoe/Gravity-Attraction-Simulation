using System.Collections.Generic;
using UnityEngine;

public class GravitySystem
{
    private const float MIN_START_POINT_SPEED = 1f;
    private const float MAX_START_POINT_SPEED = 2f;
    private const float SYSTEM_RADIUS_CONSTANT = 0.5f;
    private const float MIN_CORE_RADIUS = 1;
    private const float MAX_CORE_RADIUS = 20;
    private const float CORE_CHANGING_RATE = 1;
    private const float CORE_SIZE_SHARE = 0.5f;
    private const float GRAVITATIONAL_CONSTANT = 4f;
    private const float MAX_SPEED = 3.5f;
    private const float SQR_MAX_SPEED = MAX_SPEED * MAX_SPEED;

    public readonly float TotalMass;
    private readonly ParticleSystem.Particle[] _points;
    private readonly float[] _mass;
    private readonly int[] _inCore;
    private readonly int[] _outCore;

    private readonly ParticleSystem _system;
    private readonly int _targetCoreSize;

    private float _deltaTime;
    private float _sqrCoreRadius;
    private int _outCoreSize;

    public float CoreMass { get; private set; }

    public float CoreRadius { get; private set; }

    public int CoreSize { get; private set; }

    public Vector3 MassCenter { get; private set; }
    
    public Vector3 HeaviestPoint { get; private set; }

    public int Size { get; private set; }
    
    public GravitySystem(ParticleSystem system, int systemSize)
    {
        var systemMain = system.main;
        var systemShape = system.shape;

        systemMain.maxParticles = systemSize;
        systemShape.radius = GetSystemRadius();

        _system = system;
        _system.Emit(systemSize);

        _points = new ParticleSystem.Particle[systemSize];
        _mass = new float[systemSize];
        _inCore = new int[systemSize];
        _outCore = new int[systemSize];

        Size = _system.GetParticles(_points);

        for (var i = 0; i < Size; i++)
        {
            var vector = _points[i].position;
            vector.y = 0;
            var boost = Mathf.Sqrt(vector.magnitude * 0.1f);
            var rawVelocity = vector.normalized;
            
            rawVelocity = new Vector3(rawVelocity.z, rawVelocity.y, -rawVelocity.x);
            _points[i].velocity = Random.Range(MIN_START_POINT_SPEED, MAX_START_POINT_SPEED) * boost * rawVelocity;

            TotalMass += GetPointMass(_points[i].startSize);
        }

        CoreMass = TotalMass;
        _system.SetParticles(_points, Size);
        _targetCoreSize = (int)(systemSize * CORE_SIZE_SHARE);
        CoreRadius = MIN_CORE_RADIUS;
        _sqrCoreRadius = CoreRadius * CoreRadius;

        float GetSystemRadius() => SYSTEM_RADIUS_CONSTANT * Mathf.Pow(systemSize * 3f / (4f * Mathf.PI), 1f / 2f);
    }

    public void Read(float deltaTime)
    {
        _deltaTime = deltaTime;
        Size = _system.GetParticles(_points);

        var massCenter = Vector3.zero;

        CoreSize = 0;
        _outCoreSize = 0;
        CoreMass = 0;

        for (var i = 0; i < Size; i++)
        {
            _mass[i] = GetPointMass(_points[i].startSize);

            var isPointInCore = (MassCenter - _points[i].position).sqrMagnitude < _sqrCoreRadius;
            _points[i].remainingLifetime = _points[i].startLifetime;
            massCenter += _mass[i] * _points[i].position;

            if (isPointInCore)
            {
                _inCore[CoreSize] = i;
                CoreSize++;
                CoreMass += _mass[i];
            }
            else
            {
                _outCore[_outCoreSize] = i;
                _outCoreSize++;
            }
        }
        
        MassCenter = massCenter / TotalMass;

        UpdateCoreRadius();
    }

    public void Write()
    {
        var mostWeight = 0f;
        
        for (var i = 0; i < Size; i++)
        {
            if (_mass[i] > mostWeight)
            {
                mostWeight = _mass[i];
                HeaviestPoint = _points[i].position;
            }
            
            if (_points[i].velocity.sqrMagnitude > SQR_MAX_SPEED)
            {
                _points[i].velocity = _points[i].velocity.normalized * MAX_SPEED;
            }
        }
        
        _system.SetParticles(_points, Size);
    }

    public IEnumerable<Burst> UpdateCore()
    {
        for (var iCoreIndex = 0; iCoreIndex < CoreSize; iCoreIndex++)
        {
            var i = _inCore[iCoreIndex];
            var iMass = _mass[i];
            
            if (iMass == 0) continue;

            var iSize = _points[i].startSize;

            for (var jCoreIndex = iCoreIndex + 1; jCoreIndex < CoreSize; jCoreIndex++)
            {
                var j = _inCore[jCoreIndex];
                var jMass = _mass[j];

                if (jMass == 0) continue;
                
                var jSize = _points[j].startSize;
                
                var difference = _points[j].position - _points[i].position;

                const float COLLISION_FACTOR = 0.3f * 0.3f;
                
                if (difference.sqrMagnitude < Square(iSize + jSize) * COLLISION_FACTOR)
                {
                    var isMainPointBigger = iSize > jSize;
                    
                    var burst = isMainPointBigger
                        ? JoinPoints(i, j, in difference)
                        : JoinPoints(j, i, in difference);

                    yield return burst;
                    
                    if (isMainPointBigger) continue;

                    break;
                }

                var deltaForce = GetForce(iMass, jMass, difference) * _deltaTime;
                
                _points[i].velocity += deltaForce / iMass;
                _points[j].velocity -= deltaForce / jMass;
            }
        }
    }

    public void UpdateOutCore()
    {
        for (var outCoreIndex = 0; outCoreIndex < _outCoreSize; outCoreIndex++)
        {
            var i = _outCore[outCoreIndex];
            
            var difference = MassCenter - _points[i].position;
            var force = GetForce(_mass[i], CoreMass, difference);
            _points[i].velocity += _deltaTime / _mass[i] * force;
        }
    }
    
    private Burst JoinPoints(int b, int s, in Vector3 difference)
    {
        var burstPower = _mass[s];
        var mass = _mass[b] + _mass[s];

        _points[b].velocity = (_points[b].velocity * _mass[b] + _points[s].velocity * _mass[s]) / mass;
        _points[b].startSize = GetPointSize(mass);
        _points[b].position += _mass[s] / mass * difference;
        _mass[b] = mass;

        _mass[s] = 0;
        _points[s].startSize = 0;
        _points[s].remainingLifetime = 0;

        return new Burst(_points[s].position, burstPower, _points[b].velocity);
    }

    private void UpdateCoreRadius()
    {
        var difference = _deltaTime * CORE_CHANGING_RATE * CoreRadius;
        
        if (CoreSize > _targetCoreSize)
        {
            if (CoreRadius < MIN_CORE_RADIUS) return;
            
            CoreRadius -= difference;
            _sqrCoreRadius = CoreRadius * CoreRadius;
        }
        else if (CoreRadius < MAX_CORE_RADIUS)
        {
            CoreRadius += difference;
            _sqrCoreRadius = CoreRadius * CoreRadius;
        }
    }

    private static float GetPointMass(float size) => 4f / 3f * Mathf.PI * Mathf.Pow(size, 3);

    private static float GetPointSize(float mass) => Mathf.Pow(3f * mass / (Mathf.PI * 4f), 1f / 3f);

    private static float Square(float value) => value * value;
    
    private static Vector3 GetForce(float massI, float massJ, Vector3 difference)
    {
        return GRAVITATIONAL_CONSTANT * massI * massJ 
               / difference.sqrMagnitude
               * difference.normalized;
    }
}