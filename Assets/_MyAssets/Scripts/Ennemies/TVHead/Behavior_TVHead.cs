using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Device;
using UnityEngine.Video;
using UnityEngine.XR;

public class Behavior_TVHead : EnemyBase
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
    //private VideoPlayer _videoPlayer = default(VideoPlayer);

    [Space]
    [Header("Audio")]
    [SerializeField] private AudioSource _audioSource = default;
    [SerializeField] private AudioClip _idleAudio = default;
    [SerializeField] private AudioClip _foundAudio = default;
    [SerializeField] private AudioClip _screenBreakAudio = default;

    [Space]
    [Header("Stats")]
    [SerializeField] private int despawnTime = 5;
    [SerializeField] private int _hitPoints = 2;
    [SerializeField] private float _speed = 1f;

    [Space]
    [Header("AI Components")]
    [SerializeField] private GameObject _agentObject = default;
    private NavMeshAgent _agent = default;
    [SerializeField] private DetectionZone _detectionZone = default;
    [SerializeField] private ColliderDegat _colliderDegat = default;
    [SerializeField] private Renderer _detectionIndicator = default;
    [SerializeField] private float roamRadius = 5f;
    [SerializeField] private bool _isRumbleActivated = false;

    private Collider[] allColliders;
    private Rigidbody[] allRB;
    private bool _areCollidersEnabled = true;

    private Renderer[] allRenderers;
    private bool _areRenderersEnabled = true;

    

    /*
    [Space]
    [SerializeField] private Player _player = default;
    */

    private Animator _animator;

    public GameObject _player;
    
    public bool _isPlayerDetected = false;    
    private Vector3 _initialPosition;
    private Vector3 _destinationPosition;
    private bool _isDestinationSet = false;
    public bool _isDead = false;
    private bool _isBroken = false;

    public delegate void OnEnnemieDestroyHandler();
    public event OnEnnemieDestroyHandler EnnemieDestroyed;

    private void Awake()
    {
        InstantiateAgent();
    }

    private void InstantiateAgent()
    {
        GameObject _agentInstance;
                   
        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 10f, NavMesh.AllAreas) && Physics.CheckSphere(transform.position, 2f, LayerMask.GetMask("Default")))
        {
            _agentInstance = Instantiate(_agentObject, transform.parent);
            _agentInstance.GetComponent<NavMeshAgent>().Warp(hit.position);
            _agent = _agentInstance.GetComponent<NavMeshAgent>();            
        }
        else
        {                
            Debug.Log("No Navmesh Found");            
            Destroy(gameObject);           
        }
    }

    private void Start()
    {
        
        _player = FindAnyObjectByType<XROrigin>().gameObject;
        allColliders = GetComponentsInChildren<Collider>();
        allRB = GetComponentsInChildren<Rigidbody>();
        allRenderers = GetComponentsInChildren<Renderer>();
        

        _initialPosition = transform.position;
        _animator = GetComponent<Animator>();   
        _detectionZone.PlayerDetected += OnPlayerDetected;
        _colliderDegat.DamagePlayer += OnDamagePlayer;
        //_videoPlayer = _screen.GetComponent<VideoPlayer>();
        //ChangeVideoByUrl(_videoParDefault);
        if (_isPlayerDetected)
        {
            OnPlayerDetected(_player);
        }
        else
        {
            Roaming();
        }        
    }

    private void Update()
    {
        PerformanceCheck();
       

        if (_agent.enabled == true)
        {
            transform.position = _agent.transform.position;
            transform.rotation = _agent.transform.rotation;
            if(_isPlayerDetected)
                _agent.destination = _player.transform.position;
        }  

        if(!_agent.hasPath && _agent.enabled == true && DistanceToPlayer() < 50)
        {
            Roaming();
        }
    }

    private float DistanceToPlayer()
    {
        return Vector3.Distance(transform.position, _player.transform.position);
    }

    private void PerformanceCheck()
    {
        float distanceToPlayer = DistanceToPlayer();
        //Debug.Log("Position Ennemie : " + transform.position + "    Position Joueur : " + _player.transform.position + "    Distance : " + distanceToPlayer);
        if(distanceToPlayer >= 50 && _areRenderersEnabled)
        {            
            ToggleRenderers(false);
        }

        if (distanceToPlayer < 50 && !_areRenderersEnabled)
        {            
            ToggleRenderers(true);
        }

        if (distanceToPlayer > 25 && _areCollidersEnabled)
        {            
            ToggleColliders(false);
        }

        if (distanceToPlayer <= 25 && !_areCollidersEnabled)
        {            
            ToggleColliders(true);
        }
    }

    private void ToggleColliders(bool state)
    {
        foreach (var rb in allRB)
        {
            rb.useGravity = state;
            rb.isKinematic = !state;
        }
        foreach (var col in allColliders)
        {
            col.enabled = state;
        }
        _areCollidersEnabled = state;
        
    }

    private void ToggleRenderers(bool state)
    {
        foreach (var ren in allRenderers)
        {
            ren.enabled = state;
        }
        _areRenderersEnabled = state;
        
    }


    private void Roaming()
    {
        StartCoroutine(NextDestination());
        
    }

    private IEnumerator NextDestination()
    {
        yield return new WaitForSeconds(1);
        _isDestinationSet = false;
        if (!_isPlayerDetected)
        {           
            while (!_isDestinationSet)
            {
                Vector3 randomPoint = _initialPosition + (Random.insideUnitSphere * roamRadius);
                NavMeshHit hit;                
                // Ensure the random point is on the NavMesh
                if (NavMesh.SamplePosition(randomPoint, out hit, roamRadius, NavMesh.AllAreas))
                {
                    // Set the agent's destination to the random point
                    _agent.SetDestination(hit.position);
                    _isDestinationSet = true;
                }
                
            }
        }
    }

    private void OnPlayerDetected(GameObject player)
    {        
        _player = player;
        _detectionIndicator.material.SetColor("_EmissionColor", new Color(25, 0, 0));
        _audioSource.clip = _foundAudio;
        _audioSource.Play();
        //ChangeVideoByUrl(_videoAttack);
        _isPlayerDetected = true;
    }

    private void OnDamagePlayer(GameObject player)
    {
        if (!_isDead)
        {
            _player = player;

            if (_isRumbleActivated)
            {
                TriggerHapticFeedback();
            }

            _player.GetComponentInParent<Player>().BaisseDeVie();
            EnnemieDestroyed?.Invoke();
            Destroy(_agent.gameObject);
            Destroy(this.gameObject);
        }        
        
        //SI LE TEMPS FAIRE EFFET VISUEL
    }

    public void TriggerHapticFeedback()
    {        
            StartCoroutine(HapticFeedbackCoroutine());              
    }

    private IEnumerator HapticFeedbackCoroutine()
    {
        InputDevice leftHand = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        InputDevice rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

        if (leftHand.isValid)
        {
            leftHand.SendHapticImpulse(0, 1f, 2f);
        }

        if (rightHand.isValid)
        {
            rightHand.SendHapticImpulse(0, 1f, 2f);
        }

        yield return new WaitForSeconds(0.5f);
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
        EnnemieDestroyed?.Invoke();
        _isDead = true;
        _agent.enabled = false;
        _detectionZone.gameObject.SetActive(false);
        _colliderDegat.gameObject.SetActive(false);
        _detectionIndicator.material.DisableKeyword("_EMISSION");        
        if(!_isBroken)
            DeactivateScreen();
        
        StartCoroutine(Despawn());
        EnableRagdoll();
    }

    public void EnableRagdoll()
    {
        _animator.enabled = false;
    }

    IEnumerator Despawn()
    {
        yield return new WaitForSeconds(despawnTime);
        
        Destroy(_agent.gameObject);
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

    /*public void ChangeVideoByUrl(string url)
    {
        if (_videoPlayer != null)
        {
            _videoPlayer.Stop();       
            _videoPlayer.url = url;    
            _videoPlayer.Play();       
        }
    }*/
}
