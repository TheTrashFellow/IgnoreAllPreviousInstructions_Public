using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.XR.CoreUtils;
//using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Device;
using UnityEngine.Video;
using UnityEngine.XR;
//using static UnityEditor.Experimental.GraphView.GraphView;

public class TVHead_Rails : EnemyBase
{
    [Header("For Head")]
    [SerializeField] private GameObject screenExplosionParticleSystem;
    [SerializeField] private GameObject screenOff;
    [SerializeField] private GameObject screenOn;
    [SerializeField] private GameObject shards;
    [SerializeField] private Collider _weakspot;
    [SerializeField] private GameObject _screen;
    [SerializeField] private string _videoAttack;
    [SerializeField] private string _videoParDefault;
    private VideoPlayer _videoPlayer = default(VideoPlayer);

    [Space]
    [Header("Audio")]
    [SerializeField] private AudioSource _audioSource = default;
    [SerializeField] private AudioClip _idleAudio = default;
    [SerializeField] private AudioClip _foundAudio = default;
    [SerializeField] private AudioClip _screenBreakAudio = default;

    [Space]
    [Header("Stats")]
    [SerializeField] private int despawnTime = 10;
    [SerializeField] private int _hitPoints = 2;
    [SerializeField] private float _speed = 1f;

    [Space]
    [Header("AI Components")]
    [SerializeField] private Animator _rails = default;
    [SerializeField] private Renderer _detectionIndicator = default;
    

    private Animator _animator;


    public GameObject _player;
    public bool _isPlayerDetected = false;    
    private Vector3 _initialPosition;
    private Vector3 _destinationPosition;
    private bool _isDestinationSet = false;
    

    private bool _isBroken = false;

    private void Awake()
    {
        
    }

    private void Start()
    {                
        _animator = GetComponent<Animator>();  
        _videoPlayer = _screen.GetComponent<VideoPlayer>();
        ChangeVideoByUrl(_videoParDefault);        
    }


    override public void Target_hit_head(int damage)
    {
        _hitPoints -= (damage*2);
        Headshot();
        CheckHitPoints();
    }

    override public void Target_hit_body(int damage)
    {
        _hitPoints -= damage;
        CheckHitPoints();
    }

    override public void CheckHitPoints()
    {
        if(_hitPoints <= 0)
        {
            Death();
        }
    }

    override public void Death()
    {    
        _detectionIndicator.material.DisableKeyword("_EMISSION");        
        if(!_isBroken)
            DeactivateScreen();
        
        StartCoroutine(Despawn());
        EnableRagdoll();
    }

    public void EnableRagdoll()
    {
        _animator.enabled = false;
        _rails.enabled = false;
    }

    IEnumerator Despawn()
    {
        yield return new WaitForSeconds(despawnTime);       
        Destroy(gameObject);
    }

    public void DeactivateScreen()
    {
        _audioSource.Stop();
        screenOn.SetActive(false);
        screenOff.SetActive(true);
    }


    public void Headshot()
    {
        _audioSource.loop = false;
        _audioSource.clip = _screenBreakAudio;
        _audioSource.Play();
        _weakspot.enabled = false;
        _isBroken = true;
        screenOff.SetActive(false);
        screenOn.SetActive(false);
        shards.SetActive(true);
        Rigidbody[] shardRBs = GetComponentsInChildren<Rigidbody>();
        screenExplosionParticleSystem.SetActive(true);
        foreach (Rigidbody shardRB in shardRBs)
        {
            float randomForce = Random.Range(1, 5);
            float randomRotationX = Random.Range(-20, 20);
            float randomRotationY = Random.Range(-20, 20);
            float randomRotationZ = Random.Range(-20, 20);
            shardRB.transform.Rotate(randomRotationX, randomRotationY, randomRotationZ);
            shardRB.AddRelativeForce(Vector3.forward * randomForce, ForceMode.Impulse);
        }
        shards.transform.SetParent(null, true);
        StartCoroutine(RemoveShards(shards));
    }

    IEnumerator RemoveShards(GameObject shards)
    {
        yield return new WaitForSeconds(2);
        Destroy(shards);
    }

    public void ChangeVideoByUrl(string url)
    {
        if (_videoPlayer != null)
        {
            _videoPlayer.Stop();       
            _videoPlayer.url = url;    
            _videoPlayer.Play();       
        }
    }
}
