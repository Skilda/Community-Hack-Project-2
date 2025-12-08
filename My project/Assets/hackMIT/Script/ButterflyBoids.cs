using UnityEngine;
using System.Collections.Generic;
using Oculus.Interaction.DebugTree;

public class ButterflyFlock : MonoBehaviour
{
    // ===================================================================
    //                  SECTION GESTIONNAIRE D'ESSAIM (FlockManager)
    // ===================================================================

    [Header("1. Configuration de l'Essaim")]
    [Tooltip("Le Prefab du papillon à instancier.")]
    public GameObject boidPrefab; // <-- La variable pour set le Game Object
    public float minSize = 1, maxSize = 2;
    [Tooltip("Le point défini autour duquel les papillons doivent tourner.")]
    public Transform centerPoint;
    [Tooltip("Le nombre de papillons à créer.")]
    public int numberOfBoids = 20;
    [Tooltip("Le rayon de dispersion initial.")]
    public float spawnRadius = 5f;

    // Liste pour stocker tous les papillons créés
    private List<BoidUnit> allBoids = new List<BoidUnit>();

    // Structure interne pour stocker les données de chaque "Boid"
    private class BoidUnit
    {
        public Transform transform;
        public Animator animator;
        public Vector3 currentDirection;
        public Vector3 randomOffset;
        public float randomUpdateTimer;
    }

    // ===================================================================
    //                  SECTION PARAMÈTRES DE VOL (ButterflyBoid)
    // ===================================================================

    [Header("2. Paramètres de Vol")]
    [Tooltip("Vitesse de vol du papillon.")]
    public float moveSpeed = 5f;
    [Tooltip("Vitesse à laquelle le papillon tourne vers sa nouvelle direction.")]
    public float rotationSpeed = 4f;
    [Tooltip("Distance maximale pour ressentir le centre.")]
    public float centerForceRadius = 10f;
    [Tooltip("Force pour attirer le papillon vers le centre.")]
    public float centerForce = 0.5f;

    [Header("3. Mouvement Organique")]
    [Tooltip("Fréquence de recalcul de la direction aléatoire (plus faible = plus lent).")]
    public float randomDirectionFrequency = 0.5f;
    [Tooltip("Force (magnitude) du mouvement aléatoire ajouté.")]
    public float randomForceMagnitude = 0.1f;

    // ===================================================================
    //                          FONCTIONS DE BASE
    // ===================================================================

    void Start()
    {
        InitializeFlock();
    }

    void Update()
    {
        // Appelle la logique de mouvement pour CHAQUE papillon
        UpdateBoids();
    }

    /// <summary>
    /// Instancie tous les papillons au démarrage.
    /// </summary>
    private void InitializeFlock()
    {
        if (boidPrefab == null)
        {
            Debug.LogError("Le Prefab de Papillon (boidPrefab) n'est pas défini !");
            return;
        }

        for (int i = 0; i < numberOfBoids; i++)
        {
            Vector3 spawnPosition = centerPoint.position + Random.insideUnitSphere * spawnRadius;

            // Instanciation
            GameObject boidObject = Instantiate(boidPrefab, spawnPosition, Quaternion.identity, this.transform);

            // --- MODIFICATION CLÉ POUR LA TAILLE ---
            // 1. Calculer une taille aléatoire entre minSize et maxSize
            float randomScale = Random.Range(minSize, maxSize);
            // 2. Appliquer l'échelle uniforme au Transform
            boidObject.transform.localScale = Vector3.one * randomScale;
            // ---------------------------------------

            // Création de l'unité de Boid interne
            BoidUnit newBoid = new BoidUnit
            {
                transform = boidObject.transform,
                animator = boidObject.GetComponent<Animator>(),
                currentDirection = Random.insideUnitSphere.normalized
            };

            // Démarrer l'animation
            if (newBoid.animator != null)
            {
                newBoid.animator.SetTrigger("Fly");
            }

            allBoids.Add(newBoid);
        }
    }

    /// <summary>
    /// Met à jour la position et l'orientation de chaque papillon.
    /// </summary>
    private void UpdateBoids()
    {
        foreach (BoidUnit boid in allBoids)
        {
            // 1. Logique du mouvement organique (aléatoire)
            CalculateRandomOffset(boid);

            // 2. Calcul de la direction cible
            Vector3 targetDirection = CalculateBoidMovement(boid);

            // Lisser la transition vers la nouvelle direction
            boid.currentDirection = Vector3.Lerp(boid.currentDirection, targetDirection, Time.deltaTime);

            // 3. Regarder dans la direction du mouvement
            Quaternion targetRotation = Quaternion.LookRotation(boid.currentDirection);
            boid.transform.rotation = Quaternion.Slerp(boid.transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            // 4. Déplacer l'objet
            boid.transform.position += boid.currentDirection * moveSpeed * Time.deltaTime;
        }
    }

    // ===================================================================
    //                      LOGIQUE INDIVIDUELLE (Méthodes privées)
    // ===================================================================

    /// <summary>
    /// Calcule une nouvelle direction aléatoire à des intervalles définis pour le boid donné.
    /// </summary>
    private void CalculateRandomOffset(BoidUnit boid)
    {
        boid.randomUpdateTimer -= Time.deltaTime;

        if (boid.randomUpdateTimer <= 0f)
        {
            // Générer un nouveau vecteur aléatoire dans une sphère unitaire
            boid.randomOffset = Random.insideUnitSphere.normalized;
            // Réinitialiser le chronomètre
            boid.randomUpdateTimer = 1f / randomDirectionFrequency;
        }
    }

    /// <summary>
    /// Calcule la direction vers le centre du vol AVEC un décalage aléatoire.
    /// </summary>
    private Vector3 CalculateBoidMovement(BoidUnit boid)
    {
        // Calculer la direction vers le centre de l'essaim
        Vector3 centerDirection = centerPoint.position - boid.transform.position;
        Vector3 finalDirection;

        if (centerDirection.magnitude > centerForceRadius)
        {
            // On se dirige fortement vers le centre
            finalDirection = centerDirection.normalized * centerForce;
        }
        else
        {
            // Direction de vol actuelle + petite force aléatoire
            finalDirection = boid.currentDirection + (boid.randomOffset * randomForceMagnitude);
        }

        // On s'assure que le vecteur final est normalisé
        return finalDirection.normalized;
    }

    // ===================================================================
    //                          OUTILS DE L'ÉDITEUR
    // ===================================================================

    // Vous pouvez visualiser le point central dans l'éditeur de jeu (Gizmos)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(centerPoint.position, 0.5f);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(centerPoint.position, spawnRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(centerPoint.position, centerForceRadius);
    }
}