using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomTalk : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource audioSource;
    public List<AudioClip> sfxList = new List<AudioClip>();

    [Header("Delay Settings")]
    public float minDelay = 0f;
    public float maxDelay = 1f;

    private void Start()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        StartCoroutine(PlayRandomSFX());
    }

    private IEnumerator PlayRandomSFX()
    {
        float delay = Random.Range(minDelay, maxDelay);
        yield return new WaitForSeconds(delay);

        if (sfxList == null || sfxList.Count == 0)
        {
            Debug.LogWarning("RandomTalk: No SFX assigned!");
            yield break;
        }

        if (audioSource == null)
        {
            Debug.LogWarning("RandomTalk: No AudioSource found!");
            yield break;
        }

        AudioClip chosen = sfxList[Random.Range(0, sfxList.Count)];
        audioSource.PlayOneShot(chosen);
    }
}
