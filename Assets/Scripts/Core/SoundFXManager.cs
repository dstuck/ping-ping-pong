using UnityEngine;

public class SoundFXManager : MonoBehaviour
{
    public static SoundFXManager instance;

    [SerializeField] private AudioSource soundFXObject;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip[] hitTableClips;
    [SerializeField] private AudioClip[] paddleClips;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public void PlaySoundFXClip(AudioClip audioClip, Transform spawnTransform, float volume = 1.0f)
    {
        if (audioClip == null || soundFXObject == null || spawnTransform == null)
        {
            Debug.LogWarning("[SoundFXManager] Missing clip, prefab, or spawn transform.");
            return;
        }

        AudioSource audioSource = Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);
        audioSource.clip = audioClip;
        audioSource.volume = volume;
        audioSource.Play();

        float clipLength = audioClip.length;
        Destroy(audioSource.gameObject, clipLength);
    }

    public void PlayRandomSoundFXClip(AudioClip[] audioClips, Transform spawnTransform, float volume = 1.0f)
    {
        if (audioClips == null || audioClips.Length == 0)
        {
            Debug.LogWarning("[SoundFXManager] No clips provided for random selection.");
            return;
        }

        AudioClip randomClip = audioClips[Random.Range(0, audioClips.Length)];
        PlaySoundFXClip(randomClip, spawnTransform, volume);
    }

    public void PlayHitTableSound(Transform spawnTransform = null, float volume = 1.0f)
    {
        Transform targetTransform = spawnTransform != null ? spawnTransform : transform;
        PlayRandomSoundFXClip(hitTableClips, targetTransform, volume);
    }

    public void PlayPaddleSound(Transform spawnTransform = null, float volume = 1.0f)
    {
        Transform targetTransform = spawnTransform != null ? spawnTransform : transform;
        PlayRandomSoundFXClip(paddleClips, targetTransform, volume);
    }
}
