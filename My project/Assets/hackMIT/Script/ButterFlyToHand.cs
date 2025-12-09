using JetBrains.Annotations;
using Oculus.Interaction.Samples;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;


public class ButterFlyToHand : MonoBehaviour
{
    // public GameObject butterflyPrefab;
    public Transform r_handMeshNode;

    //public ButterflyBoids butterflyBoids;

    // Update is called once per frame
    [Header("Target & Speed")]
    [Tooltip("The butterfly object to be moved.")]
    public GameObject targetButterfly;

    [Tooltip("The transform to move the butterfly towards (e.g., the palm or wrist joint).")]
    public Transform handDestination;

    [Tooltip("The speed at which the butterfly moves towards the hand.")]
    public float lerpSpeed = 5f;

    [Header("Pointing Setup")]
    [Tooltip("Reference to the script that manages the pointing gesture detection.")]
    public OVRHand LefthandTracker;

    [Header("Audio")]
    public AudioSource AudioSource_scene;
    public AudioClip selectedAudioClip;
    public AudioClip LastAudioClipPlayed;
    public AudioClip[] AudioClipLibrary;

    [Header("VFX")]
    public ParticleSystem pinchParticle;
    public ParticleSystem RemoveParticle;

    // --- Private State Variables ---
    private bool isPointing = false;
    private bool isAttracting = false;
    private bool isOnPlayerHand = false;
    private bool isAudioFinish = false;
    private bool isFinishedPlaying;
    private bool wasPlaying;
    private bool audioplaying;

    void Start()
    {
        //targetButterfly = butterflyBoids.butterfliesAsset[1];
        pinchParticle.Stop();
    }

    void Update()
    {
        if (LefthandTracker == null || targetButterfly == null || handDestination == null)
        {
            Debug.LogError("Required components are not assigned!", this);
            return;
        }


        if (LefthandTracker.GetFingerIsPinching(OVRHand.HandFinger.Index))
        {

            StartAttraction();
            pinchParticle.Play();
        }
        else
        {
            pinchParticle.Stop();
        }

        if (isAttracting)
        {
            AttractButterfly();
        }

        if (isOnPlayerHand)
        {
            PlayAudio();
        }

        if (isFinishedPlaying)
        {
            RemoveButterfly();
        }

        if (RemoveParticle.isEmitting == false)
        {
            RemoveParticle.Stop();
        }

        if (targetButterfly.transform.position == Vector3.zero)
        {

            targetButterfly.SetActive(true);
        }
    }

    // Called once when the pointing gesture is detected
    private void StartAttraction()
    {
        isAttracting = true;
        // Optional: Perform a specific action on the butterfly when attraction starts (e.g., set an Animator Trigger)
        // targetButterfly.GetComponent<Animator>().SetTrigger("Attract"); 
        Debug.Log("Pointing detected! Starting attraction.");
    }

    // Handles the smooth movement of the butterfly
    private void AttractButterfly()
    {
        // Smoothly move the butterfly towards the designated hand position
        targetButterfly.transform.position = Vector3.Lerp(
            targetButterfly.transform.position,
            handDestination.position,
            Time.deltaTime * lerpSpeed
        );

        // Optional: Rotate the butterfly to face the hand
        Quaternion targetRotation = Quaternion.LookRotation(handDestination.position - targetButterfly.transform.position);
        targetButterfly.transform.rotation = Quaternion.Slerp(targetButterfly.transform.rotation, targetRotation, Time.deltaTime * lerpSpeed);

        // Check if the butterfly has arrived
        if (Vector3.Distance(targetButterfly.transform.position, handDestination.position) < 0.05f)
        {
            isAttracting = false;
            Debug.Log("Butterfly arrived at hand.");
            // Optional: Parent the butterfly to the hand joint when it arrives (to hold it)
            //targetButterfly.transform.SetParent(handDestination);
            isOnPlayerHand = true;
        }
    }

    private void PlayAudio()
    {
        // Safety check for AudioSource reference
        if (AudioSource_scene == null)
        {
            Debug.LogError("AudioSource_scene is null! Cannot play audio.", this);
            return;
        }

        // 1. Select AudioClip only if none is currently selected (or if it was reset)
        if (selectedAudioClip == null && AudioClipLibrary != null)
        {
            selectedAudioClip = AudioClipLibrary[Random.Range(0, AudioClipLibrary.Length)];
        }

        // 2. Start Playing if not already playing
        if (!audioplaying && selectedAudioClip != null)
        {
            audioplaying = true;
            AudioSource_scene.clip = selectedAudioClip;
            AudioSource_scene.Play();
            wasPlaying = true; // Mark that playback has started
        }

        // 3. Detect Finish
        // We only check for stop if we previously confirmed it was playing (wasPlaying)
        if (wasPlaying && !AudioSource_scene.isPlaying)
        {
            // Playback has stopped! Signal the end.
            isFinishedPlaying = true;
            wasPlaying = false;
            audioplaying = false;

            Debug.Log("AudioClip finished! isFinishedPlaying is now TRUE.");
        }
        else if (AudioSource_scene.isPlaying)
        {
            // While playing, make sure wasPlaying is true and finished state is false
            wasPlaying = true;
            isFinishedPlaying = false;
        }

        // 4. Clean up state variables after finishing
        if (isFinishedPlaying)
        {
            isAudioFinish = true; // Still set this if needed by other systems
                                  // DO NOT SET AudioSource_scene = null; here.
        }
    }

    private void RemoveButterfly()
    {
        RemoveParticle.Play();

        


        isOnPlayerHand = false;
        targetButterfly.SetActive(false);
        targetButterfly.transform.position = Vector3.zero;

        // --- COMPLETE STATE RESET FOR NEXT CYCLE ---

        // Key fix: Reset the clip reference so a NEW one is chosen in PlayAudio()
        selectedAudioClip = null;

        // Key fix: Reset all control bools
        isFinishedPlaying = false;
        isAudioFinish = false;
        audioplaying = false;
        wasPlaying = false;
    }
}