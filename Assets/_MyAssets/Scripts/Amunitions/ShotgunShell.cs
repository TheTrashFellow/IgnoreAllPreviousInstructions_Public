using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ShotgunShell : MonoBehaviour
{
    [SerializeField] public GameObject _cassing = default;
    
    /*private void Start()
    {
        XRInteractionManager manager = FindFirstObjectByType<XRInteractionManager>();
        XRBaseInteractable interactable = GetComponent<XRBaseInteractable>();

        if (manager != null && interactable != null)
        {
            manager.UnregisterInteractable(interactable as IXRInteractable);
            manager.RegisterInteractable(interactable as IXRInteractable);
        }
    }*/

    public void OnShot()
    {
        gameObject.GetComponent<MeshRenderer>().enabled = false;
        _cassing.SetActive(true);
    }
}
