using UnityEngine;
using UnityEngine.Audio;

public class SoundMaker : MonoBehaviour
{
    private const int SIZE = 100;
    
    [SerializeField] private AudioClip[] clips;
    [SerializeField] private AudioMixerGroup output;

    private AudioSource[] _sources;
    private int _current;

    private void Start()
    {
        _sources = new AudioSource[SIZE];

        for (var i = 0; i < SIZE; i++)
        {
            _sources[i] = new GameObject("AudioSource").AddComponent<AudioSource>();
            _sources[i].outputAudioMixerGroup = output;
            _sources[i].transform.parent = transform;
        }
    }

    public void Play(Vector3 position)
    {
        ref var source = ref _sources[_current];
        source.transform.position = position;
        source.PlayOneShot(clips[Random.Range(0, clips.Length)]);
        _current++;
        
        if (_current >= SIZE)
        {
            _current = 0;
        }
    }
}
