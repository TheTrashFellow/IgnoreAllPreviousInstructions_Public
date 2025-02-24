using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public class Behavior_Scout : EnemyBase
{    
    [Header("Stats")]
    [SerializeField] private int despawnTime = 10;
    [SerializeField] private int _hitPoints = 1;
    [SerializeField] private float _speed = 3.5f;

    [Space]
    [Header("AI Components")]
    [SerializeField] private GameObject _agentObject = default;
    private NavMeshAgent _agent = default;
    [SerializeField] private DetectionZone _detectionZone = default;
    [SerializeField] private Light _detectionIndicator = default;
    [SerializeField] private Light _detectionPointLight = default;
    
    [Range(4.5f, 20f)][SerializeField] private float roamRadius = 10f;
    [SerializeField] private float _safeDistance = 8f;

    [Space]
    [Header("Visual")]
    [SerializeField] private GameObject _light = default;
    [SerializeField] private ParticleSystem _explosion = default;

    [Space]
    [Header("Audio")]
    [SerializeField] private AudioSource _audioSource = default;
    [SerializeField] private AudioClip _audioFound = default;
    [SerializeField] private AudioClip _explosion1 = default;
    [SerializeField] private AudioClip _explosion2 = default;

    [Space]
    [Header("Pour Spawn Ennemies")]
    [SerializeField] private GameObject _TVHead = default;
    [SerializeField] private Collider _spawnCollider = default;
    [SerializeField] private int _maxEnnemies = 3;
    private int currentEnnemies = 0;
    [SerializeField] private float _delaiSpawn = 2;

    private Collider[] allColliders;
    private Rigidbody[] allRB;
    private bool _areCollidersEnabled = true;

    private Renderer[] allRenderers;
    private Light[] allLights;
    private bool _areRenderersEnabled = true;

    private Animator _animator;

    private GameObject _player;
    private bool _isPlayerDetected = false;
    private Vector3 _initialPosition;
    private Vector3 _destinationPosition;
    private bool _isDestinationSet = false;
    private bool _isAtSafeDistance = false;

    public delegate void OnEnnemieDestroyHandler();
    public event OnEnnemieDestroyHandler EnnemieDestroyed;

    private void Awake()
    {
        GameObject _agentInstance = Instantiate(_agentObject, transform.parent);
        _agentInstance.transform.position = transform.position;
        _agentInstance.transform.rotation = transform.rotation;
        _agent = _agentInstance.GetComponent<NavMeshAgent>();        
    }

    private void Start()
    {
        _player = FindAnyObjectByType<XROrigin>().gameObject;
        allColliders = GetComponentsInChildren<Collider>();
        allRB = GetComponentsInChildren<Rigidbody>();
        allRenderers = GetComponentsInChildren<Renderer>();
        allLights = GetComponentsInChildren<Light>();

        //_spawnCollider.gameObject.transform.parent = transform.parent;

        _initialPosition = transform.position;
        _animator = GetComponentInChildren<Animator>();
        _detectionZone.PlayerDetected += OnPlayerDetected;
        Roaming();
    }

    private void Update()
    {
        PerformanceCheck();
        //_spawnCollider.transform.position = transform.position;

        if (_agent.enabled == true)
        {
            transform.position = _agent.transform.position;            

            if (_isPlayerDetected)
            {
                transform.LookAt(_player.transform);
                _light.transform.LookAt(_player.transform);

                float distanceToTarget = Vector3.Distance(_agent.transform.position, _player.transform.position);
                if (distanceToTarget < _safeDistance )
                {
                    //_isAtSafeDistance = false;
                    Vector3 directionAway = (_agent.transform.position - _player.transform.position).normalized;
                    Vector3 newPosition = _agent.transform.position + directionAway * (_safeDistance - distanceToTarget);

                    _agent.stoppingDistance = 0;
                    _agent.speed = 10;
                    _agent.autoBraking = false;
                    _agent.destination = newPosition;                    
                }
                if(distanceToTarget >= _safeDistance)
                {
                    //_isAtSafeDistance = true;
                    _agent.stoppingDistance = _safeDistance;
                    _agent.speed = _speed;
                    _agent.autoBraking = true;
                    _agent.destination = _player.transform.position;
                }
            }
            else
            {
                transform.rotation = _agent.transform.rotation;                
            }
                
        }

        if (!_agent.hasPath && _agent.enabled == true && !_spin)
        {
            _agent.stoppingDistance = 0;
            _agent.transform.forward = _agent.transform.right;
            Roaming();
        }
        else
        {
            SpinCheck();
        }
    }

    private void PerformanceCheck()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, _player.transform.position);
        //Debug.Log("Position Ennemie : " + transform.position + "    Position Joueur : " + _player.transform.position + "    Distance : " + distanceToPlayer);
        if (distanceToPlayer >= 50 && _areRenderersEnabled)
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
        foreach(Light light in allLights)
        {
            light.enabled = state;
        }
        foreach (var ren in allRenderers)
        {
            ren.enabled = state;
        }
        _areRenderersEnabled = state;        
    }

    private float DistanceToPlayer()
    {
        return Vector3.Distance(transform.position, _player.transform.position);
    }

    private bool _spin = false;
    private void Roaming()
    {
        _isDestinationSet = false;
        if (!_isPlayerDetected)
        {            
            Vector3 nextPoint = _initialPosition + (_agent.transform.forward * roamRadius);
            NavMeshHit hit;

            while (!_isDestinationSet)
            {
                // Ensure the random point is on the NavMesh
                if (NavMesh.SamplePosition(nextPoint, out hit, roamRadius, NavMesh.AllAreas))
                {
                    // Set the agent's destination to the random point
                    _agent.SetDestination(hit.position);
                    _isDestinationSet = true;                    
                } 
                else
                {
                    Destroy(_agent.gameObject);
                    Destroy(gameObject);
                    break;
                }
            }
        }
    }

    private bool _isSpinning = false;
    private void SpinCheck()
    {
        if (!_isSpinning)
        {
            StartCoroutine(Spin());
            _isSpinning = true;
        }
             
    }

    private IEnumerator Spin()
    {
        yield return new WaitForSeconds(2);
        _agent.transform.Rotate(0, 45, 0);
        _isSpinning = false;
    }

    private void OnPlayerDetected(GameObject player)
    {        
        _animator.SetTrigger("Player Detected");
        _audioSource.clip = _audioFound;        
        _audioSource.Play();
        _player = player;   
        _isPlayerDetected = true;
        StartCoroutine(SpawnEnnemies());
    }

    override public void Target_hit_head(int damage)
    {
        _hitPoints -= (damage * 2);
        CheckHitPoints();
    }

    public override void CheckHitPoints()
    {
        if(_hitPoints <= 0)
        {
            Death();
        }
    }

    override public void Death()
    {
        _agent.enabled = false;
        _detectionIndicator.enabled = false;
        _detectionZone.gameObject.SetActive(false);
        _explosion.Play();       
        _audioSource.loop = false;        
        _audioSource.clip = _explosion1;
        _audioSource.volume = 1f;
        _audioSource.Play();

        StartCoroutine(Despawn());
        EnableRagdoll();
    }

    public void EnableRagdoll()
    {
        Vector3 test = gameObject.GetComponentInChildren<Rigidbody>().velocity;
        test.y /= 40;
        test.x = gameObject.transform.forward.x;
        test.z = gameObject.transform.forward.z;
        Debug.Log("X : " + test.x + " ET Z : " + test.z);
        _animator.enabled = false;
        gameObject.GetComponentInChildren<Rigidbody>().velocity = new Vector3(0f,0f,0f);
        gameObject.GetComponentInChildren<Rigidbody>().velocity = test ;
    }

    IEnumerator Despawn()
    {
        
        List<GameObject> gameObjects = new List<GameObject>();
        gameObject.GetChildGameObjects(gameObjects);
        
        yield return new WaitForSeconds(despawnTime);
        foreach (GameObject go in gameObjects)
        {
            if (go.name == "GFX")
            {
                go.active = false;
            }
        }
        _explosion.gameObject.transform.localScale *= 2;
        _explosion.Play();
        _audioSource.clip = _explosion2;
        _audioSource.Play();
        yield return new WaitForSeconds(5);
        EnnemieDestroyed?.Invoke();
        Destroy(_agent.gameObject);
        Destroy(gameObject);
    }

    private IEnumerator SpawnEnnemies()
    {
        bool spawned = true;
        while(_agent.enabled == true)
        {
            if(spawned)
                yield return new WaitForSeconds(_delaiSpawn);
            spawned = false;
            if(currentEnnemies < 3)
            {
                Bounds bounds = _spawnCollider.bounds;

                Vector3 randomPoint = new Vector3(
                    Random.Range(bounds.min.x, bounds.max.x),0,
                    /*Random.Range(bounds.min.y, bounds.max.y),*/
                    Random.Range(bounds.min.z, bounds.max.z)
                    );

                bool _isFarEnough = Vector3.Distance(randomPoint, _player.GetComponentInParent<Player>().gameObject.transform.position) > 5f;

                if (_spawnCollider.bounds.Contains(randomPoint) && Physics.CheckSphere(randomPoint, 0.1f, LayerMask.GetMask("Default")) && _isFarEnough)
                {
                    float randomDegree = Random.Range(0f, 360f);
                    Quaternion rotation = Quaternion.Euler(0f, randomDegree, 0f);
                    GameObject thisEnnemie = Instantiate(_TVHead, randomPoint, rotation, gameObject.transform.parent);
                    currentEnnemies++;
                    thisEnnemie.GetComponent<Behavior_TVHead>()._player = _player;
                    thisEnnemie.GetComponent<Behavior_TVHead>()._isPlayerDetected = true;
                    thisEnnemie.GetComponent<Behavior_TVHead>().EnnemieDestroyed += ReduceEnnemies;
                    spawned = true;
                }
                else
                    spawned = false;                
            }
            yield return null;
        }
    }

    private void ReduceEnnemies()
    {
        currentEnnemies--;
    }
}
