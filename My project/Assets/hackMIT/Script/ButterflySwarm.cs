using UnityEngine;
using System.Collections.Generic;

public class ButterflySwarm : MonoBehaviour
{
    [Header("Settings")]
    public GameObject butterflyPrefab; // Assign your butterfly 3D model here
    public int butterflyCount = 20;
    public float swarmRadius = 2.0f;
    public float rotationSpeed = 30f;
    public float scatterSpeed = 5f;

    [Header("Target")]
    public Transform centerTarget; // Assign the "CenterEyeAnchor" (Player Head)

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

            // Give each butterfly a unique random offset so they don't move identically
            randomOffsets.Add(Random.insideUnitSphere);
        }
    }

    void Update()
    {
        if (centerTarget == null) return;

        // 2. Rotate the entire swarm container around the player
        if (!isScattering)
        {
            transform.position = centerTarget.position;
            transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
        }

        // 3. Update individual butterfly movements
        for (int i = 0; i < butterflies.Count; i++)
        {
            Transform b = butterflies[i];

            if (isScattering)
            {
                // Move away from the center
                Vector3 direction = (b.position - centerTarget.position).normalized;
                b.position += direction * scatterSpeed * Time.deltaTime;
                b.LookAt(b.position + direction); // Face away
            }
            else
            {
                // Gentle floating noise
                Vector3 noise = new Vector3(
                    Mathf.Sin(Time.time + randomOffsets[i].x) * 0.5f,
                    Mathf.Cos(Time.time + randomOffsets[i].y) * 0.5f,
                    Mathf.Sin(Time.time + randomOffsets[i].z) * 0.5f
                );

                // Keep them relative to the swarm center
                b.localPosition = (randomOffsets[i] * swarmRadius) + noise;

                // Make them face the direction of the swarm rotation
                b.LookAt(b.position + transform.forward);
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