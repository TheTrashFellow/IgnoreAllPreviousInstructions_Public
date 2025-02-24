using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private int bulletID;
    [SerializeField] private string bulletName;
    [SerializeField] private Sprite bulletImage;
    [SerializeField] private GameObject bulletModel;

    public int BulletID { get => bulletID; set => bulletID = value; }
    public string BulletName { get => bulletName; set => bulletName = value; }
    public Sprite BulletImage { get => bulletImage; set => bulletImage = value; }
    public GameObject BulletModel { get => bulletModel; set => bulletModel = value; }
}
