using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/*
public class GrabTransferHandler : MonoBehaviour
{
    [SerializeField] private XRInteractionManager xrManager = default;
    [SerializeField] private XRBaseInteractor _leftHand = default;
    [SerializeField] private XRBaseInteractor _rightHand = default;
    [SerializeField] private XRRayInteractor _leftRay = default;
    [SerializeField] private XRRayInteractor _rightRay = default;

    [Space]
    [Header("Weapons")]
    [SerializeField] private ShotgunManager shotgun = default;

    private XRGrabInteractable grabInteractable;
   
    public void OnRaySelectLeft(SelectEnterEventArgs args)
    {
        var interactable = args.interactableObject;
        xrManager.SelectExit(_leftRay, interactable);
        xrManager.SelectEnter(_leftHand, interactable);
        string objectName = interactable.transform.gameObject.name;
        if (objectName.Contains("Coach"))
        {
            interactable.transform.gameObject.GetComponent<ShotgunManager>().HoldingGun(_leftHand);
        }
    }

    public void OnRaySelectRight(SelectEnterEventArgs args)
    {
        var interactable = args.interactableObject;
        xrManager.SelectExit(_rightRay, interactable);
        xrManager.SelectEnter(_rightHand, interactable);
        string objectName = interactable.transform.gameObject.name;
        if (objectName.Contains("Coach"))
        {
            interactable.transform.gameObject.GetComponent<ShotgunManager>().HoldingGun(_leftHand);
        }
    }
    /*
    public void OnHoverEnteredLeft(HoverEnterEventArgs args)
    {
        var interactable = args.interactableObject as IXRSelectInteractable;
        
        if(interactable != null && interactable.isSelected)
        {
            foreach(var interactor in interactable.interactorsSelecting)
            {
                if(interactor is XRRayInteractor)
                {
                    xrManager.SelectExit(interactor, interactable);
                    xrManager.SelectEnter(_leftHand, interactable);
                }
            }
        }
        
    }

    public void OnHoverEnteredRight(HoverEnterEventArgs args)
    {
        var interactable = args.interactableObject as IXRSelectInteractable;

        if (interactable != null && interactable.isSelected)
        {
            foreach (var interactor in interactable.interactorsSelecting)
            {
                if (interactor is XRRayInteractor)
                {
                    xrManager.SelectExit(interactor, interactable);
                    xrManager.SelectEnter(_rightHand, interactable);
                }
            }
        }

    }
    
}*/
