using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

//public class WeaponButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
//{
//    private Weapon weapon;
//    private UIManagerInGame uiManager;
//    private bool isNear = false;

//    // Initialize the button with weapon and UIManager
//    public void Initialize(Weapon weapon, UIManagerInGame uiManager)
//    {
//        this.weapon = weapon;
//        this.uiManager = uiManager;
//    }

//    void Update()
//    {
//        // Check if the user clicked on the button while being near
//        if (isNear && Input.GetMouseButtonDown(0))  // Left-click to grab
//        {
//            // Select the weapon when clicked
//            uiManager.SelectWeapon(uiManager.UICanvas, weapon);
//            Debug.Log("Weapon Selected: " + weapon.WeaponName);
//        }
//    }

//    // This will be triggered when the mouse enters the button's collider area
//    public void OnPointerEnter(PointerEventData eventData)
//    {
//        isNear = true;  // The player is close enough to interact
//    }

//    // This will be triggered when the mouse exits the button's collider area
//    public void OnPointerExit(PointerEventData eventData)
//    {
//        isNear = false;  // The player is no longer near the button
//    }
//}
