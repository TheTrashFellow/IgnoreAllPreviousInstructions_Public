using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBase : MonoBehaviour
{

    public virtual void Target_hit_head(int damage)
    {

    }

    public virtual void Target_hit_body(int damage)
    {

    }


    public virtual void CheckHitPoints()
    {

    }

    public virtual void Death()
    {

    }

    public float moveSpeed = 5f; // Speed at which the object moves
    public float lifetime = 5f; // Time before the object gets destroyed (5 seconds)

    private void Start()
    {
        // Destroy this object after the specified lifetime
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        // Move the object forward (in the object's local space)
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
    }


}
