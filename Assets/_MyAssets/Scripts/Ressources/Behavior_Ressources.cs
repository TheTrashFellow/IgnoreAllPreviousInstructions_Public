using System.Collections;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public float followSpeed = 5f;
    private Transform target;
    [SerializeField] private ParticleSystem targetParticleSystem = default;
    private GameManager gameManager;

    private bool _isTargetFound = false;

    private void Awake()
    {
        gameManager = FindAnyObjectByType<GameManager>();
    }

    void Start()
    {
        // Chercher le joueur par son tag "Player"
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            target = playerObject.transform;
        }

        //GameManager.Instance.OnRetireRevolverRessource += RetirerRessourceRevolverHandler;
        //GameManager.Instance.OnAjouteRevolverRessource += AjouterRessourceRevolverHandler;
        //GameManager.Instance.OnAjouteVie += AjouterVieHandler;
        //GameManager.Instance.OnRetireVie += RetirerVieHandler;
    }

    void Update()
    {
        if (target != null && !_isTargetFound)
        {
            transform.position = Vector3.MoveTowards(transform.position, target.position, followSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {

            _isTargetFound = true;
            var system = targetParticleSystem.emission;
            system.enabled = false;  

            //StartCoroutine(Destroy());
        }
    }

    private IEnumerator Destroy()
    {
        yield return new WaitForSeconds(3);
        //GameManager.Instance.OnRetireRevolverRessource -= RetirerRessourceRevolverHandler;
        //GameManager.Instance.OnAjouteRevolverRessource -= AjouterRessourceRevolverHandler;
        //GameManager.Instance.OnRetireVie -= RetirerVieHandler;
        //GameManager.Instance.OnAjouteVie -= AjouterVieHandler;

        Destroy(this.gameObject);
    }
}
