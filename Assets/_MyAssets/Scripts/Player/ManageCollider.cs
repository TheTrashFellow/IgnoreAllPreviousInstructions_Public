using System.Collections.Generic;
using UnityEngine;

public class ManageCollider : MonoBehaviour
{
    [SerializeField] private Transform player;              // Référence au joueur
    [SerializeField] private float detectionRadius = 30f;   // Rayon de détection autour du GameObject

    private List<Collider> colliders = new List<Collider>();
    private bool collidersActive = false;

    private void Start()
    {        
        colliders.AddRange(GetComponentsInChildren<Collider>());
    }

    private void Update()
    {
        if (player == null)
        {
            return;
        }

        Vector3 playerPosition = player.position;
        float distanceToPlayer = Vector3.Distance(transform.position, playerPosition);

        if (distanceToPlayer <= detectionRadius )
        {
            //Debug.Log("test1 - Activation des colliders");
            SetCollidersState(true);
        }
        
        if (distanceToPlayer > detectionRadius )
        {
            //Debug.Log("test2 - Désactivation des colliders");
            SetCollidersState(false);
        }

        //Debug.Log("test3 - Fin de l'Update");
    }

    private void SetCollidersState(bool state)
    {
        foreach (Collider col in colliders)
        {
            if (col != null)
            {
                col.enabled = state;
                Rigidbody rb = col.GetComponent<Rigidbody>();

                if (rb != null)
                {
                    rb.isKinematic = !state;
                    rb.useGravity = state;
                }
            }
        }

        collidersActive = state;
        //Debug.Log($"[INFO] Colliders {(state ? "activés" : "désactivés")} pour {gameObject.name}");
    }

    private void OnDrawGizmosSelected()
    {
        //Gizmos.color = Color.yellow;
        //Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}