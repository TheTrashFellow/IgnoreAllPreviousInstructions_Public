using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickaxe : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Destroyable")
        {
            other.gameObject.GetComponent<Destructible>().Break();
        }
    }
}
