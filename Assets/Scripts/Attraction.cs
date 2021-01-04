using Cinemachine;
using UnityEngine;

public class Attraction : MonoBehaviour
{
    public const int DEFAULT_SYSTEM_SIZE = 200;
    private const float MAX_SPEED = 5f;
    private const float SQR_MAX_SPEED = MAX_SPEED * MAX_SPEED;
    private const float GRAVITATIONAL_CONSTANT = 7;
    
    [SerializeField] private ParticleSystem burstSfx;
    [SerializeField] private Transform viewPoint;

    private CinemachineImpulseSource _impulseSource;
    private SoundMaker _soundMaker;

    public GravitySystemData SystemData { get; private set; }

    private void Start()
    {
        var systemSize = PlayerPrefs.GetInt(Menu.SYSTEM_SIZE_PREF, DEFAULT_SYSTEM_SIZE);
        var system =  GetComponent<ParticleSystem>();
        
        SystemData = new GravitySystemData(system, systemSize);
        _soundMaker = GetComponent<SoundMaker>();
        _impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    private void FixedUpdate()
    {
        var deltaTime = Time.fixedDeltaTime;
        SystemData.Read(Time.fixedDeltaTime);
        viewPoint.position = SystemData.MassCenter;

        for (var i = 0; i < SystemData.CoreSize; i++)
        {
            var pi = SystemData.GetPoint(SystemData.InCore[i]);
            
            if (pi.Mass == 0) continue;
            
            for (var j = i + 1; j < SystemData.CoreSize; j++)
            {
                var pj = SystemData.GetPoint(SystemData.InCore[j]);

                if (pj.Mass == 0) continue;
                
                var difference = pj.Position - pi.Position;

                if (difference.sqrMagnitude * 4 < Mathf.Pow(pi.Size + pj.Size, 2))
                {
                    var burst = GravitySystemData.JoinPoints(in pi, in pj, in difference);
                    PlayBurstFx(burst);
                    
                    continue;
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

        for (var i = 0; i < SystemData.OutCoreSize; i++)
        {
            var pi = SystemData.GetPoint(SystemData.OutCore[i]);
            
            var difference = -pi.Position;
            var force = GetForce(pi.Mass, SystemData.TotalMass - pi.Mass, difference);
            pi.Velocity += deltaTime / pi.Mass * force;
            
            if (pi.Velocity.sqrMagnitude > SQR_MAX_SPEED)
            {
                pi.Velocity = pi.Velocity.normalized * MAX_SPEED;
            }
        }

        SystemData.Write();
    }

    private Vector3 GetForce(float massI, float massJ, Vector3 difference)
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
