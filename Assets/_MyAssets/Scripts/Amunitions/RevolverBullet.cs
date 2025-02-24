using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RevolverBullet : MonoBehaviour
{
    [SerializeField] GameObject _bullet = default;
    [SerializeField] GameObject _cassing = default;

    public GameObject Bullet
    {
        get { return _bullet; }
        set { _bullet = value; }
    }

    public GameObject Cassing
    {
        get { return _cassing; }
        set { _cassing = value; }
    }
}
