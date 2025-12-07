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

    public ButterflyBoids butterflyBoids;

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
    public AudioClip[] AudioClipLibrary;

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
        targetButterfly = butterflyBoids.butterfliesAsset[1];
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

        if(isFinishedPlaying && targetButterfly.activeInHierarchy == false) 
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

        if (!audioplaying)
        {
            audioplaying = true;
            AudioSource_scene.clip = selectedAudioClip;

            AudioSource_scene.Play();
        }

        if (wasPlaying && !AudioSource_scene.isPlaying)
        {
            // La lecture s'est arrêtée ! C'est le signal que le clip est terminé.
            isFinishedPlaying = true;
            wasPlaying = false; //Réinitialiser l'état de lecture

            Debug.Log("AudioClip terminé ! isFinishedPlaying est maintenant VRAI.");

            // Vous pouvez ajouter ici l'appel à une autre fonction si nécessaire
            // CallFunctionOnFinish();
        }
        else if (AudioSource_scene.isPlaying)
        {
            // Si l'audio joue, on met à jour wasPlaying.
            wasPlaying = true;
            // On s'assure que le booléen est Faux pendant la lecture
            isFinishedPlaying = false;
        }

        if (isFinishedPlaying)
        {
            isAudioFinish = true;
            audioplaying= false;
            AudioSource_scene = null;
        }

    }

    private void RemoveButterfly()
    {
        targetButterfly.SetActive(false);
        targetButterfly.transform.position = Vector3.zero;
    }
}
