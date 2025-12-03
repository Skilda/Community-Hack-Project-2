using UnityEngine;

[RequireComponent(typeof(AudioSource), typeof(Collider))]
public class AudioSwitch : MonoBehaviour
{
    [Header("Assign Two Audio Clips")]
    public AudioClip clipA;
    public AudioClip clipB;

    private AudioSource audioSource;
    private bool playA = true; // toggles which clip plays next

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        // Ensure collider is a trigger
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Choose clip based on toggle
        audioSource.clip = playA ? clipA : clipB;
        audioSource.Play();

        // Flip to the other clip for next time
        playA = !playA;
    }
}
