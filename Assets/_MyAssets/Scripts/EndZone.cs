using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndZone : MonoBehaviour
{

    public delegate void OnEndHandler();
    public event OnEndHandler EndReached;

    // Start is called before the first frame update
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            EndReached?.Invoke();
            other.gameObject.GetComponentInParent<Player>().WinRun();
        }
    }
}
