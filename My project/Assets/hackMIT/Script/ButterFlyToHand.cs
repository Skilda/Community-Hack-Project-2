using JetBrains.Annotations;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;


public class ButterFlyToHand : MonoBehaviour
{
    public GameObject butterflyPrefab;
    public Transform r_handMeshNode;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
        butterflyPrefab.transform.position = r_handMeshNode.position;

    }
}
