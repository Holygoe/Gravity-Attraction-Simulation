using Cinemachine;
using UnityEngine;

public class Attraction : MonoBehaviour
{
    public const int DEFAULT_SYSTEM_SIZE = 200;

    [SerializeField] private ParticleSystem burstSfx;

    private CinemachineImpulseSource _impulseSource;
    private SoundMaker _soundMaker;
    private Burst[] _bursts;

    public GravitySystem GravitySystem { get; private set; }

    private void Start()
    {
        var systemSize = 2000; //PlayerPrefs.GetInt(InformationPanel.SYSTEM_SIZE_PREF, DEFAULT_SYSTEM_SIZE);
        var system =  GetComponent<ParticleSystem>();
        
        GravitySystem = new GravitySystem(system, systemSize);
        _bursts = new Burst[systemSize];
        _soundMaker = GetComponent<SoundMaker>();
        _impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    private void FixedUpdate()
    {
        GravitySystem.Read(Time.fixedDeltaTime);

        var burstCount = GravitySystem.UpdateCore(_bursts);
        PlayBurstFx(burstCount);
        GravitySystem.UpdateOutCore();

        GravitySystem.Write();
    }

    private void PlayBurstFx(int burstCount)
    {
        for (var i = 0; i < burstCount; i++)
        {
            if (_bursts[i].Energy <= 0) continue;
            
            var flashSize = Mathf.Sqrt(_bursts[i].Energy * 10);

            var param = new ParticleSystem.EmitParams
            {
                velocity = _bursts[i].Velocity * 0.5f,
                startSize = flashSize,
                startLifetime = flashSize,
                position = _bursts[i].Position,
            };

            _soundMaker.Play(_bursts[i].Position, flashSize * 2);
            burstSfx.Emit(param, 1);
            _impulseSource.GenerateImpulse(flashSize * 2);
        }
    }
}
