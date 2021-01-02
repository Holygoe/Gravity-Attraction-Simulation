using Cinemachine;
using UnityEngine;

public class Simulation : MonoBehaviour
{
    public const int DEFAULT_SYSTEM_SIZE = 200;
    private const float RADIUS_CONSTANT = 1.309f;

    [SerializeField] private float gravitationalConstant = 3;
    [SerializeField] private float maxSpeed = 2f;
    [SerializeField] private float minStartSpeed = 0.5f;
    [SerializeField] private float maxStartSpeed = 1f;
    [SerializeField] private ParticleSystem burstSfx;
    [SerializeField] private Transform centerMass;
    
    private ParticleSystem _system;
    private ParticleSystem.Particle[] _points;
    private float[] _masses;
    private bool[] _isInteractive;
    private float _sqrMaxSpeed;
    private float _sqrInteractionRadius;
    private float _totalMass;
    private float _largestMass;
    private CinemachineImpulseSource _impulseSource;
    private SoundMaker _soundMaker;
    
    public int CurrentSystemSize { get; private set; }

    private void Start()
    {
        var systemSize = PlayerPrefs.GetInt(Menu.SYSTEM_SIZE_PREF, DEFAULT_SYSTEM_SIZE);
        _system = GetComponent<ParticleSystem>();
        _soundMaker = GetComponent<SoundMaker>();

        var systemShape = _system.shape;
        systemShape.radius = Mathf.Pow(RADIUS_CONSTANT * systemSize * 3f / (4f * Mathf.PI), 1f / 3f);
        _sqrInteractionRadius = Mathf.Pow(systemShape.radius * 2, 2);

        _system.Emit(systemSize);
        _sqrMaxSpeed = maxSpeed * maxSpeed;
        _impulseSource = GetComponent<CinemachineImpulseSource>();

        InitializePoints(systemSize);
    }

    private void FixedUpdate()
    {
        CurrentSystemSize = _system.GetParticles(_points);
        var deltaTime = Time.fixedDeltaTime;
        var tempCenterMass = Vector3.zero;

        _largestMass = 0;

        for (var i = 0; i < CurrentSystemSize; i++)
        {
            if (_masses[i] <= 0) continue;

            if (_largestMass < _masses[i])
            {
                _largestMass = _masses[i];
            }
            
            _points[i].remainingLifetime = _points[i].startLifetime;
            
            tempCenterMass += _masses[i] / _totalMass * _points[i].position;

            _isInteractive[i] = (centerMass.position - _points[i].position).sqrMagnitude < _sqrInteractionRadius;

            if (!_isInteractive[i])
            {
                var difference = centerMass.position - _points[i].position;
                var force = GetForce(_masses[i], _totalMass, difference);
                _points[i].velocity += deltaTime / _masses[i] * force;
                continue;
            }

            for (var j = i + 1; j < CurrentSystemSize; j++)
            {
                if (!_isInteractive[j]) continue;
                
                var difference = _points[j].position - _points[i].position;

                if (difference.sqrMagnitude * 4 < Mathf.Pow(_points[i].startSize + _points[j].startSize, 2))
                {
                    JoinPoints(i, j, difference);
                    continue;
                }

                var force = GetForce(_masses[i], _masses[j], difference);
                var deltaForce = deltaTime * force;

                _points[i].velocity += deltaForce / _masses[i];
                _points[j].velocity -= deltaForce / _masses[j];
            }
            
            // Clamp speed.
            if (_points[i].velocity.sqrMagnitude > _sqrMaxSpeed)
            {
                _points[i].velocity = _points[i].velocity.normalized * maxSpeed;
            }
        }
        
        centerMass.position = tempCenterMass;
        _system.SetParticles(_points, CurrentSystemSize);
    }

    private Vector3 GetForce(float massI, float massJ, Vector3 difference)
    {
        return gravitationalConstant * massI * massJ 
               / difference.sqrMagnitude
               * difference.normalized;
    }
    
    private static float GetMass(float size) => 4f / 3f * Mathf.PI * Mathf.Pow(size, 3);

    private static float GetSize(float mass) => Mathf.Pow(3f * mass / (Mathf.PI * 4f), 1f / 3f);

    private void JoinPoints(int i, int j, Vector3 difference)
    {
        var minSize = _points[i].startSize < _points[j].startSize ? _points[i].startSize : _points[j].startSize;

        var mass = _masses[i] + _masses[j];
        
        _points[i].velocity = (_points[i].velocity * _masses[i] + _points[j].velocity * _masses[j]) / mass;
        _points[i].startSize = GetSize(mass);
        _points[i].position += _masses[j] / mass * difference;
        _masses[i] = mass;

        burstSfx.transform.position = _points[i].position;
        var burstMain = burstSfx.main;
        burstMain.startSize = minSize * 3;
        burstMain.startSpeed = _points[i].velocity.magnitude;
        burstSfx.transform.rotation.SetLookRotation(_points[i].velocity, Vector3.up);

        _soundMaker.Play(_points[i].position);
        burstSfx.Emit(1);

        _points[j].remainingLifetime = 0;
        _isInteractive[j] = false;
        _masses[j] = 0;

        _impulseSource.GenerateImpulse(minSize * 4);
    }
    
    private void InitializePoints(int systemSize)
    {
        _points = new ParticleSystem.Particle[systemSize];
        _masses = new float[systemSize];
        _isInteractive = new bool[systemSize];

        systemSize = _system.GetParticles(_points);
        _totalMass = 0;
        
        for (var i = 0; i < systemSize; i++)
        {
            var rawVelocity = _points[i].position.normalized;
            rawVelocity = new Vector3(rawVelocity.z, rawVelocity.y, -rawVelocity.x);
            _points[i].velocity = rawVelocity * Random.Range(minStartSpeed, maxStartSpeed);
            _masses[i] = GetMass(_points[i].startSize);
            _totalMass += _masses[i];
            _isInteractive[i] = true;
        }
        
        _system.SetParticles(_points, systemSize);
    }
}
