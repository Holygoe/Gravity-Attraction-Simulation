using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class Attraction : MonoBehaviour
{
    public const int DEFAULT_SYSTEM_SIZE = 200;
    

    [SerializeField] private ParticleSystem burstSfx;

    private CinemachineImpulseSource _impulseSource;
    private SoundMaker _soundMaker;

    public GravitySystem GravitySystem { get; private set; }

    private void Start()
    {
        var systemSize = PlayerPrefs.GetInt(InformationPanel.SYSTEM_SIZE_PREF, DEFAULT_SYSTEM_SIZE);
        var system =  GetComponent<ParticleSystem>();
        
        GravitySystem = new GravitySystem(system, systemSize);
        _soundMaker = GetComponent<SoundMaker>();
        _impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    private void FixedUpdate()
    {
        GravitySystem.Read(Time.fixedDeltaTime);

        var bursts = GravitySystem.UpdateCore();
        PlayBurstFx(bursts);
        GravitySystem.UpdateOutCore();

        GravitySystem.Write();
    }

    private void PlayBurstFx(IEnumerable<Burst> bursts)
    {
        foreach (var burst in bursts)
        {
            var size = Mathf.Pow(burst.Power, 1f / 1.2f) * 4;
            
            var param = new ParticleSystem.EmitParams
            {
                velocity = burst.Velocity * 0.5f,
                startSize = size,
                startLifetime = size * 3,
                position = burst.Position,
            };
            
            _soundMaker.Play(burst.Position, burst.Power * 20);
            burstSfx.Emit(param, 1);
            _impulseSource.GenerateImpulse(size * 2);
        }
    }
}
