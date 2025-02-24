using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class WeaponManager : MonoBehaviour
{
    [SerializeField] private List<Weapon> _listWeapons;
    [SerializeField] private GameObject _weaponButtonPrefab;
    [SerializeField] private Image _weaponDisplayImage;
    [SerializeField] private TMP_Text _weaponDisplayName;

    [SerializeField] private Transform _weaponButtonParent;
    

    private Weapon selectedWeapon;

    public void Start()
    {
        DisplayWeapons();
    }

    public void DisplayWeapons()
    {
        foreach (Weapon weapon in _listWeapons)
        {
            GameObject button = Instantiate(_weaponButtonPrefab);
            button.GetComponentInChildren<TMP_Text>().text = weapon.WeaponName.ToString();
            button.GetComponentInChildren<Image>().sprite = weapon.WeaponImage;

            // Add an OnClickListener to the button
            button.GetComponent<Button>().onClick.AddListener(() => SelectWeapon(weapon));
        }
    }

    public void SelectWeapon(Weapon weapon)
    {
        selectedWeapon = weapon;
        Debug.Log("Selected Weapon: " + weapon.WeaponName.ToString());
    }
}
