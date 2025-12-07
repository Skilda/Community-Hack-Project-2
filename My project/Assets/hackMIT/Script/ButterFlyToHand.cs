using JetBrains.Annotations;
using Oculus.Interaction.Samples;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;


public class ButterFlyToHand : MonoBehaviour
{
   // public GameObject butterflyPrefab;
    public Transform r_handMeshNode;

    public ButterflyBoids butterflyBoids;

    // Update is called once per frame
    [Header("Target & Speed")]
    [Tooltip("The butterfly object to be moved.")]
    public Transform targetButterfly;

    [Tooltip("The transform to move the butterfly towards (e.g., the palm or wrist joint).")]
    public Transform handDestination;

    [Tooltip("The speed at which the butterfly moves towards the hand.")]
    public float lerpSpeed = 5f;

    [Header("Pointing Setup")]
    [Tooltip("Reference to the script that manages the pointing gesture detection.")]
    public OVRHand LefthandTracker;

    // --- Private State Variables ---
    private bool isPointing = false;
    private bool isAttracting = false;

    void Start()
    {
        targetButterfly = butterflyBoids.butterfliesList[1];
    }

    void Update()
    {
        if (LefthandTracker == null || targetButterfly == null || handDestination == null)
        {
            Debug.LogError("Required components are not assigned!", this);
            return;
        }


        if ( !LefthandTracker.GetFingerIsPinching(OVRHand.HandFinger.Index) && LefthandTracker.GetFingerIsPinching(OVRHand.HandFinger.Middle))
        {
            //butterflyBoids.butterfliesList[1].transform.position = r_handMeshNode.position;

            StartAttraction();
        }

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
        targetButterfly.position = Vector3.Lerp(
            targetButterfly.position,
            handDestination.position,
            Time.deltaTime * lerpSpeed
        );

        // Optional: Rotate the butterfly to face the hand
        Quaternion targetRotation = Quaternion.LookRotation(handDestination.position - targetButterfly.position);
        targetButterfly.rotation = Quaternion.Slerp(targetButterfly.rotation, targetRotation, Time.deltaTime * lerpSpeed);

        // Check if the butterfly has arrived
        if (Vector3.Distance(targetButterfly.position, handDestination.position) < 0.05f)
        {
            isAttracting = false;
            Debug.Log("Butterfly arrived at hand.");
            // Optional: Parent the butterfly to the hand joint when it arrives (to hold it)
            // targetButterfly.transform.SetParent(handDestination);
        }
    }
}
