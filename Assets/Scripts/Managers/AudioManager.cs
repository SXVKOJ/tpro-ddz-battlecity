using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f;
    public bool loop = false;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    
    [SerializeField] private List<Sound> sounds;
    private Dictionary<string, AudioSource> audioSources = new Dictionary<string, AudioSource>();
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioSources();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializeAudioSources()
    {
        foreach (Sound sound in sounds)
        {
            GameObject soundObject = new GameObject(sound.name);
            soundObject.transform.SetParent(transform);
            
            AudioSource audioSource = soundObject.AddComponent<AudioSource>();
            audioSource.clip = sound.clip;
            audioSource.volume = sound.volume;
            audioSource.loop = sound.loop;
            
            audioSources[sound.name] = audioSource;
        }
    }
    
    public void PlaySound(string soundName)
    {
        if (audioSources.ContainsKey(soundName))
        {
            audioSources[soundName].Play();
        }
    }
    
    public void StopSound(string soundName)
    {
        if (audioSources.ContainsKey(soundName))
        {
            audioSources[soundName].Stop();
        }
    }
}