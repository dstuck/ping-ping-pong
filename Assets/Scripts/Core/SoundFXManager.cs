using UnityEngine;

public class SoundFXManager : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource ballHitTableSource;
    [SerializeField] private AudioSource opponentHitSource;
    [SerializeField] private AudioSource playerSwingSource;
    [SerializeField] private AudioSource ballReturnSource;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip ballHitTableClip;
    [SerializeField] private AudioClip opponentHitClip;
    [SerializeField] private AudioClip playerSwingClip;
    [SerializeField] private AudioClip ballReturnClip;

    private void Start()
    {
        // Create audio sources if they don't exist
        if (ballHitTableSource == null)
        {
            GameObject go = new GameObject("BallHitTableAudioSource");
            go.transform.SetParent(transform);
            ballHitTableSource = go.AddComponent<AudioSource>();
        }

        if (opponentHitSource == null)
        {
            GameObject go = new GameObject("OpponentHitAudioSource");
            go.transform.SetParent(transform);
            opponentHitSource = go.AddComponent<AudioSource>();
        }

        if (playerSwingSource == null)
        {
            GameObject go = new GameObject("PlayerSwingAudioSource");
            go.transform.SetParent(transform);
            playerSwingSource = go.AddComponent<AudioSource>();
        }

        if (ballReturnSource == null)
        {
            GameObject go = new GameObject("BallReturnAudioSource");
            go.transform.SetParent(transform);
            ballReturnSource = go.AddComponent<AudioSource>();
        }
    }

    public void PlayBallHitTableSound()
    {
        if (ballHitTableSource != null && ballHitTableClip != null)
        {
            ballHitTableSource.PlayOneShot(ballHitTableClip);
        }
    }

    public void PlayOpponentHitSound()
    {
        if (opponentHitSource != null && opponentHitClip != null)
        {
            opponentHitSource.PlayOneShot(opponentHitClip);
        }
    }

    public void PlayPlayerSwingSound()
    {
        if (playerSwingSource != null && playerSwingClip != null)
        {
            playerSwingSource.PlayOneShot(playerSwingClip);
        }
    }

    public void PlayBallReturnSound()
    {
        if (ballReturnSource != null && ballReturnClip != null)
        {
            ballReturnSource.PlayOneShot(ballReturnClip);
        }
    }
}

