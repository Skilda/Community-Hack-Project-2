using JetBrains.Annotations;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;


public class ButterflyBoids : MonoBehaviour
{
    [Header("Settings")]
    public GameObject butterflyPrefab; // Assign your butterfly 3D model here
    public int butterflyCount = 20;
    public float swarmRadius = 2.0f;
    public float rotationSpeed = 30f;
    public float scatterSpeed = 5f;

    

    [Header("Target")]
    public Transform centerTarget; // Assign the "CenterEyeAnchor" (Player Head)
    public Transform r_handMeshNode;
    public Transform l_handMeshNode;

    private List<Transform> butterflies = new List<Transform>();
    private List<Vector3> randomOffsets = new List<Vector3>();
    private bool isScattering = false;

    void Start()
    {

        // 1. Spawn the butterflies
        for (int i = 0; i < butterflyCount; i++)
        {
            Vector3 randomPos = Random.insideUnitSphere * swarmRadius;
            GameObject b = Instantiate(butterflyPrefab, transform.position + randomPos, Quaternion.identity);
            b.transform.parent = this.transform; // Keep hierarchy clean
            butterflies.Add(b.transform);

            b.GetComponent<Animator>().SetBool("PlayFly",true);
            // Give each butterfly a unique random offset so they don't move identically
            randomOffsets.Add(Random.insideUnitSphere);
        }
    }

    void Update()
    {
        if (centerTarget == null) return;

        

        // 1. Rotate the entire swarm container around the player (Orbit)
        // We do this first so the parent moves, carrying the children
        if (!isScattering)
        {
            transform.position = centerTarget.position;
            transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
        }

        // 2. Update individual butterfly movements
        for (int i = 0; i < butterflies.Count; i++)
        {
            Transform b = butterflies[i];

            // Store the position BEFORE we move it this frame
            Vector3 previousPosition = b.position;

            // --- Calculate New Position ---
            if (isScattering)
            {
                // Calculate move away from center
                Vector3 directionToScatter = (b.position - centerTarget.position).normalized;
                b.position += directionToScatter * scatterSpeed * Time.deltaTime;
            }
            else
            {
                // Calculate local noise movement
                Vector3 noise = new Vector3(
                    Mathf.Sin(Time.time + randomOffsets[i].x) * 0.5f,
                    Mathf.Cos(Time.time + randomOffsets[i].y) * 0.5f,
                    Mathf.Sin(Time.time + randomOffsets[i].z) * 0.5f
                );

                // Apply position relative to the rotating parent
                b.localPosition = (randomOffsets[i] * swarmRadius) + noise;
            }

            // --- Calculate Rotation ---
            // Determine the vector between where we were and where we are now
            Vector3 moveDirection = b.position - previousPosition;

            // Only rotate if we actually moved (to avoid errors when standing still)
            if (moveDirection.sqrMagnitude > 0.0001f)
            {
                // Smoothly rotate towards the movement direction
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                b.rotation = Quaternion.Slerp(b.rotation, targetRotation, Time.deltaTime * 10f);
            }
        }
    }

    // Call this function to trigger the reaction
    public void TriggerScatter()
    {
        isScattering = !isScattering; // Toggle state
        Debug.Log("Butterflies Scattering: " + isScattering);
    }

}