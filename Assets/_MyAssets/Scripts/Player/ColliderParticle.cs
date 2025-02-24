using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderParticle : MonoBehaviour
{
    [SerializeField] private Player _player;
    [SerializeField] private AudioSource _grabSon;

    private GameManager gameManager;
    private void Awake()
    {
        gameManager = FindAnyObjectByType<GameManager>();   
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.tag == "HealthRessource")
        {
            _player.GainDeVie();
            _grabSon.Play();
        }

        if (other.gameObject.tag == "RevolverRessource")
        {

            gameManager.GainRessourceRevolverBullet();
            _grabSon.Play();
        }

        if (other.gameObject.tag == "ShotGunBulletRessource") 
        {
            gameManager.GainRessourceShotGunBullet();
            _grabSon.Play();
        }

        
    }
}
