using UnityEngine;

public class GravitySystemData
{
    private const float MIN_START_POINT_SPEED = 1f;
    private const float MAX_START_POINT_SPEED = 2f;
    private const float SUPER_MASS_SHARE = 0.1f;
    private const float SYSTEM_RADIUS_CONSTANT = 1.3f;
    private const float MIN_CORE_RADIUS = 1;
    private const float MAX_CORE_RADIUS = 50;
    private const float CORE_CHANGING_RATE = 1;
    private const int OPTIMAL_CORE_SIZE = 200;

    public readonly ParticleSystem.Particle[] Particles;
    public readonly float[] MassData;
    public readonly bool[] CoreData;
    public readonly float TotalMass;
    public readonly int[] InCore;
    public readonly int[] OutCore;

    private readonly ParticleSystem _system;
    private readonly float _superMass;

    private float _deltaTime;
    private float _sqrCoreRadius;
    
    public float CoreRadius { get; private set; }

    public int CoreSize { get; private set; }
    
    public int OutCoreSize { get; private set; }
    
    public Vector3 MassCenter { get; private set; }

    public int Size { get; private set; }
    
    public GravitySystemData(ParticleSystem system, int systemSize)
    {
        var systemMain = system.main;
        var systemShape = system.shape;

        systemMain.maxParticles = systemSize;
        systemShape.radius = GetSystemRadius();

        _system = system;
        _system.Emit(systemSize);

        Particles = new ParticleSystem.Particle[systemSize];
        MassData = new float[systemSize];
        CoreData = new bool[systemSize];
        InCore = new int[systemSize];
        OutCore = new int[systemSize];

        Size = _system.GetParticles(Particles);

        for (var i = 0; i < Size; i++)
        {
            var vector = Particles[i].position;
            var boost = Mathf.Sqrt(vector.magnitude) / 2;
            var rawVelocity = vector.normalized;
            
            rawVelocity = new Vector3(rawVelocity.z, rawVelocity.y, -rawVelocity.x);
            Particles[i].velocity = Random.Range(MIN_START_POINT_SPEED, MAX_START_POINT_SPEED) * boost * rawVelocity;

            TotalMass += GetPointMass(Particles[i].startSize);
        }
        
        _system.SetParticles(Particles, Size);
        _superMass = TotalMass * SUPER_MASS_SHARE;
        CoreRadius = MIN_CORE_RADIUS;
        _sqrCoreRadius = CoreRadius * CoreRadius;

        float GetSystemRadius() => SYSTEM_RADIUS_CONSTANT * Mathf.Pow(systemSize * 3f / (4f * Mathf.PI), 1f / 3f); 
    }

    public void Read(float deltaTime)
    {
        _deltaTime = deltaTime;
        Size = _system.GetParticles(Particles);

        var massCenter = Vector3.zero;
        CoreSize = 0;
        OutCoreSize = 0;

        for (var i = 0; i < Size; i++)
        {
            MassData[i] = GetPointMass(Particles[i].startSize);
            CoreData[i] = (MassCenter - Particles[i].position).sqrMagnitude < _sqrCoreRadius;
            Particles[i].remainingLifetime = Particles[i].startLifetime;
            massCenter += MassData[i] * Particles[i].position;

            if (CoreData[i])
            {
                InCore[CoreSize] = i;
                CoreSize++;
            }
            else
            {
                OutCore[OutCoreSize] = i;
                OutCoreSize++;
            }
        }

        MassCenter = massCenter / TotalMass;
        
        if (CoreSize > OPTIMAL_CORE_SIZE)
        {
            if (CoreRadius > MIN_CORE_RADIUS)
            {
                CoreRadius -= _deltaTime * CORE_CHANGING_RATE;
            }
        }
        else if (CoreRadius < MAX_CORE_RADIUS)
        {
            CoreRadius += _deltaTime * CORE_CHANGING_RATE;
        }
        
        _sqrCoreRadius = CoreRadius * CoreRadius;
    }

    public void Write()
    {
        _system.SetParticles(Particles, Size);
    }

    public Point GetPoint(int index)
    {
        return new Point(this, index);
    }

    public static Burst JoinPoints(in Point pi, in Point pj, in Vector3 difference)
    {
        var minSize = pi.Size < pj.Size ? pi.Size : pj.Size;
        var mass = pi.Mass + pj.Mass;

        pi.Velocity = (pi.Velocity * pi.Mass + pj.Velocity * pj.Mass) / mass;
        pi.Size = GetPointSize(mass);
        pi.Position += pj.Mass / mass * difference;
        pi.Mass = mass;

        pj.ResetToZero();

        return new Burst(pi.Position, minSize * 3, pi.Velocity);
    }

    private static float GetPointMass(float size) => 4f / 3f * Mathf.PI * Mathf.Pow(size, 3);

    private static float GetPointSize(float mass) => Mathf.Pow(3f * mass / (Mathf.PI * 4f), 1f / 3f);
}