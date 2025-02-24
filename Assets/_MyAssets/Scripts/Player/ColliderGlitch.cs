using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderGlitch : MonoBehaviour
{
    public List<GameObject> _ennemies;

    private void Awake()
    {
        _ennemies = new List<GameObject>();
        
    }

    private void OnTriggerEnter(Collider other)
    {        
        if (other.gameObject.tag == "GlitchEffect")
        {
            
            _ennemies.Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        
        if (other.gameObject.tag == "GlitchEffect")
        {
            
            _ennemies.Remove(other.gameObject);
        }
    }
}
