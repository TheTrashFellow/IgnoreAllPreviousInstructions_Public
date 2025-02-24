using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using static UnityEditor.Experimental.GraphView.GraphView;

public class DetectionZone : MonoBehaviour
{
    public delegate void PlayerDetectedHandler(GameObject player);
    public event PlayerDetectedHandler PlayerDetected;

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            PlayerDetected?.Invoke(other.gameObject);
        }
    }
}
