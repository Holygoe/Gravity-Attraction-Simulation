using UnityEngine;
using UnityEngine.Audio;

public class SoundMaker : MonoBehaviour
{
    private const int SIZE = 300;
    
    [SerializeField] private AudioClip[] clips;
    [SerializeField] private AudioMixerGroup output;

    private AudioSource[] _sources;
    private int _currentSource;

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

    public void Play(Vector3 position, float volume)
    {
        var source = _sources[_currentSource];

        source.transform.position = position;
        source.PlayOneShot(clips[Random.Range(0, clips.Length)], volume);

        _currentSource++;

        if (_currentSource >= SIZE)
        {
            _currentSource = 0;
        }
    }
}