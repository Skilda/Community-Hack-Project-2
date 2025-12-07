using JetBrains.Annotations;
using Oculus.Interaction.Samples;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;


public class ButterFlyToHand : MonoBehaviour
{
   // public GameObject butterflyPrefab;
    public Transform r_handMeshNode;

    public ButterflyBoids butterflyBoids;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

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
    public OVRHand handTracker;

    // --- Private State Variables ---
    private bool isPointing = false;
    private bool isAttracting = false;

    void Update()
    {
        if (handTracker == null || targetButterfly == null || handDestination == null)
        {
            Debug.LogError("Required components (Hand Tracker, Butterfly, or Destination) are not assigned!");
            return;
        }

        // 1. Detect the Pointing Gesture
        // We use GetFingerIsPinching() as a simple way to detect a focused gesture (like pointing).
        // Alternatively, use an OVRHand.GetFingerIsPinching(OVRHand.HandFinger.Index) == false
        // to detect the fully straight "pointing" gesture.

        bool isIndexStraight = !handTracker.GetFingerIsPinching(OVRHand.HandFinger.Index);

        // This is a simplified check: is the index finger straight AND are all other fingers pinched/curled?
        // The Meta Interaction SDK often has better ways (using OVRGestureConfig).
        // For this example, we'll use a basic check for pointing:
        isPointing = isIndexStraight &&
                     handTracker.GetFingerIsPinching(OVRHand.HandFinger.Middle) &&
                     handTracker.GetFingerIsPinching(OVRHand.HandFinger.Ring) &&
                     handTracker.GetFingerIsPinching(OVRHand.HandFinger.Pinky);

        // 2. Start/Stop Attraction Logic
        if (isPointing && !isAttracting)
        {
            // Start the attraction process when pointing begins
            StartAttraction();
        }
        else if (!isPointing && isAttracting)
        {
            // Stop attraction when pointing stops (optional, for continuous control)
            isAttracting = false;
        }

        // 3. Movement (Lerp)
        if (isAttracting)
        {
            AttractButterfly();
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
            // targetButterfly.transform.SetParent(handDestination);
        }
    }
}
