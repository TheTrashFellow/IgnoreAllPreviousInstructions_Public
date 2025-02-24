using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderDegat : MonoBehaviour
{
    public delegate void PlayerDamageHandler(GameObject player);
    public event PlayerDamageHandler DamagePlayer;

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            DamagePlayer?.Invoke(other.gameObject);
        }
    }
}
