using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnZone : MonoBehaviour
{
    [SerializeField] private GameObject _TVHead = default;
    [SerializeField] private GameObject _Scout = default;
    [SerializeField] private List<Collider> _colliders = default;
    [SerializeField] private int _maxEnnemies = 5;
    [SerializeField] private GameObject _ennemieContainer = default;
    
    private List<Behavior_TVHead> _heads = new List<Behavior_TVHead>();
    private List<Behavior_Scout> _scouts = new List<Behavior_Scout>();

    public delegate void OnEnnemieDestroyHandler();
    public event OnEnnemieDestroyHandler EnnemieDestroyed;

    private void Start()
    {
        
    }

    public int TotalEnnemies()
    {
        return _heads.Count + _scouts.Count;
    }

    public void DifficultyAdd(int numEnnemies, bool scoutAllowed)
    {
        if (!scoutAllowed)
        {
            for(int i = 0; i < numEnnemies; i++)
            {
                SpawnTVHeadLocal();
            }
        }
        else
        {
            for (int i = 0; i < numEnnemies; i++)
            {
                if(Random.Range(1,10) > 5)
                    SpawnTVHeadLocal();
                else
                    SpawnScoutLocal();
            }
        }
    }
    
    public void SpawnTVHeadLocal()
    {
        if(_heads.Count + _scouts.Count < _maxEnnemies)
        {
            int randomCollider = GetRandomCollider();
            Vector3 randomPoint = GetRandomPoint(randomCollider);
            StartCoroutine(InstantiateTVHead(randomPoint));
        }
        else
        {

        }
    }
    public void SpawnScoutLocal()
    {
        if (_heads.Count + _scouts.Count < 5)
        {
            int randomCollider = GetRandomCollider();
            Vector3 randomPoint = GetRandomPoint(randomCollider);
            StartCoroutine(InstantiateScout(randomPoint));
        }
        else
        {
        }
    }

    private int GetRandomCollider()
    {
        return Random.Range(0, _colliders.Count);
    }

    private Vector3 GetRandomPoint(int colliderNumber)
    {
        Collider collider = _colliders[colliderNumber];
        Bounds bounds = collider.bounds;

        Vector3 randomPoint = new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y),
            Random.Range(bounds.min.z, bounds.max.z)
            );

        if (collider.bounds.Contains(randomPoint) && Physics.CheckSphere(randomPoint, 0.1f, LayerMask.GetMask("Default")))
        {
            return randomPoint;
        }
        else
            return new Vector3(0, 0, 0);

        //return GetRandomPoint(colliderNumber);
    }

    private IEnumerator InstantiateTVHead(Vector3 randomPoint)
    {
        yield return new WaitForSeconds(1);

        float randomDegree = Random.Range(0f, 360f);
        Quaternion rotation = Quaternion.Euler(0f, randomDegree, 0f);

        GameObject thisEnnemie = Instantiate(_TVHead, randomPoint, rotation, _ennemieContainer.transform);
        _heads.Add(thisEnnemie.GetComponent<Behavior_TVHead>());
        _heads[_heads.Count - 1].EnnemieDestroyed += RemoveTVHead;
    }

    private void RemoveTVHead()
    {
        EnnemieDestroyed?.Invoke();
        StartCoroutine(ClearTVList());
    }

    private IEnumerator ClearTVList() 
    {
        yield return new WaitForSeconds(1);
        _heads.RemoveAll(item => item == null);
    }

    private IEnumerator InstantiateScout(Vector3 randomPoint)
    {
        yield return new WaitForSeconds(1);

        float randomDegree = Random.Range(0f, 360f);
        Quaternion rotation = Quaternion.Euler(0f, randomDegree, 0f);

        Instantiate(_Scout, randomPoint, rotation, _ennemieContainer.transform);
        _scouts[_scouts.Count - 1].EnnemieDestroyed += RemoveScout;
    }

    private void RemoveScout()
    {
        EnnemieDestroyed?.Invoke();
        StartCoroutine(ClearScoutList());
    }

    private IEnumerator ClearScoutList()
    {
        yield return new WaitForSeconds(1);
        _scouts.RemoveAll(item => item == null);
    }
}
