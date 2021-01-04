using Cinemachine;
using UnityEngine;

public class Attraction : MonoBehaviour
{
    public const int DEFAULT_SYSTEM_SIZE = 200;
    private const float MAX_SPEED = 5f;
    private const float SQR_MAX_SPEED = MAX_SPEED * MAX_SPEED;
    private const float GRAVITATIONAL_CONSTANT = 7;
    
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
        var deltaTime = Time.fixedDeltaTime;
        GravitySystem.Read(Time.fixedDeltaTime);

        for (var i = 0; i < GravitySystem.CoreSize; i++)
        {
            var pi = GravitySystem.GetPoint(GravitySystem.InCore[i]);
            
            if (pi.Mass == 0) continue;
            
            for (var j = i + 1; j < GravitySystem.CoreSize; j++)
            {
                var pj = GravitySystem.GetPoint(GravitySystem.InCore[j]);

                if (pj.Mass == 0) continue;
                
                var difference = pj.Position - pi.Position;

                if (difference.sqrMagnitude * 4 < Mathf.Pow(pi.Size + pj.Size, 2))
                {
                    var canContinue = pi.Size > pj.Size;
                    
                    var burst = canContinue
                        ? GravitySystem.JoinPoints(in pi, in pj, in difference)
                        : GravitySystem.JoinPoints(in pj, in pi, in difference);
                    
                    PlayBurstFx(burst);

                    if (canContinue) continue;

                    break;
                }

                var deltaForce = GetForce(pi.Mass, pj.Mass, difference) * deltaTime;
                
                pi.Velocity += deltaForce / pi.Mass;
                pj.Velocity -= deltaForce / pj.Mass;
            }
            
            if (pi.Velocity.sqrMagnitude > SQR_MAX_SPEED)
            {
                pi.Velocity = pi.Velocity.normalized * MAX_SPEED;
            }
        }

        for (var i = 0; i < GravitySystem.OutCoreSize; i++)
        {
            var pi = GravitySystem.GetPoint(GravitySystem.OutCore[i]);
            
            var difference = GravitySystem.CoreCenter - pi.Position;
            var force = GetForce(pi.Mass, GravitySystem.CoreMass, difference);
            pi.Velocity += deltaTime / pi.Mass * force;
            
            if (pi.Velocity.sqrMagnitude > SQR_MAX_SPEED)
            {
                pi.Velocity = pi.Velocity.normalized * MAX_SPEED;
            }
        }

        GravitySystem.Write();
    }

    private static Vector3 GetForce(float massI, float massJ, Vector3 difference)
    {
        return GRAVITATIONAL_CONSTANT * massI * massJ 
               / difference.sqrMagnitude
               * difference.normalized;
    }

    private void PlayBurstFx(Burst burst)
    {
        burstSfx.transform.position = burst.Position;
        var burstMain = burstSfx.main;
        burstMain.startSize = burst.Power;
        _soundMaker.Play(burst.Position);
        burstSfx.Emit(1);
        _impulseSource.GenerateImpulse(burst.Power);
    }
}
