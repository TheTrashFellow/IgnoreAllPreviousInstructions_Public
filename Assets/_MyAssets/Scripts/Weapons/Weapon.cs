using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Weapon : MonoBehaviour
{
    [SerializeField] private int weaponID;
    [SerializeField] private string weaponName;
    [SerializeField] private Sprite weaponImage;
    [SerializeField] private GameObject weaponModel;

    public int WeaponID { get => weaponID; set => weaponID = value; }
    public string WeaponName { get => weaponName; set => weaponName = value; }
    public Sprite WeaponImage { get => weaponImage; set => weaponImage = value; }
    public GameObject WeaponModel { get => weaponModel; set => weaponModel = value; }
}
