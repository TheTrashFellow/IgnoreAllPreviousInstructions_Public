using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using System;
using Random = UnityEngine.Random;
using Unity.VisualScripting;
using Unity.XR.CoreUtils;

public class RevolverManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject _raycastOrigin = default;
    [SerializeField] private GameObject _muzzleFlash = default;    

    [Space]
    [SerializeField] private Animator _animator = default;
    [SerializeField] private List<XRSocketInteractor> _sockets = default;
    [SerializeField] private List<SphereCollider> _colliders = default;

    [Space]
    [Header("Audio")]
    [SerializeField] private GameObject _gunshotAudioSource = default;
    [SerializeField] private List<AudioClip> _clipList;
    [SerializeField] private AudioClip _openRevolver = default;
    [SerializeField] private AudioClip _closeRevolver = default;
    [SerializeField] private AudioClip _addBullet = default;

    [Space]
    [Header("InputActionReferences")]
    [SerializeField] private InputActionReference _leftHandInput = default;
    [SerializeField] private InputActionReference _rightHandInput = default;

    [Space]
    [Header("Gameplay values")]
    [SerializeField] private int _damage = 1;

    private bool _isGunHeld = false;
    private bool _isInReloadState = false;
    private XRBaseInteractor _currentController;

    private int _currentBulletIndex = 5; 
    private bool _areBulletsAssigned = false;
    private List<GameObject> _bullets = new List<GameObject>();
    private List<GameObject> _cassings = new List<GameObject>();

    public bool IsGunHeld { get => _isGunHeld; set => _isGunHeld = value; }

    private void Start()
    {    
        AssignBullets();
    }

    private void AssignBullets()
    {
        _bullets.Clear();
        _cassings.Clear();        
        
        foreach(var socket in _sockets)
        {
            try
            {
                RevolverBullet thisBullet = socket.selectTarget.gameObject.GetComponent<RevolverBullet>();
                _bullets.Add(thisBullet.Bullet);
                _cassings.Add(thisBullet.Cassing);                                         
            }
            catch { }     
        }
        _areBulletsAssigned = true;       
        
    }

    private void Update()
    {
        if (_isGunHeld)
        {
            if (_leftHandInput.action.WasPressedThisFrame() && _currentController.gameObject.tag == "LeftHand")
            {
                InitiateReloadState();
            }
            if (_rightHandInput.action.WasPressedThisFrame() && _currentController.gameObject.tag == "RightHand")
            {
                InitiateReloadState();
            }
        }        
    }

    private void InitiateReloadState()
    {
        if (!_isInReloadState)
        {
            InstantiateOtherAudio("Open", gameObject.transform.position);
            _isInReloadState = true;
            _animator.SetBool("IsInReloadState", true);            
        }
        else
        {            
            _isInReloadState = false;
            _animator.SetBool("IsInReloadState", false);
            InstantiateOtherAudio("Close", gameObject.transform.position);

            foreach (Collider collider in _colliders)
            {
                collider.enabled = false;
            }            
        }
    }

    public void InsertBullet()
    {
        InstantiateOtherAudio("Add", gameObject.transform.position);
        if (_areBulletsAssigned)
        {            
            _currentBulletIndex++;            
            RevolverBullet thisBullet = _sockets[_currentBulletIndex].selectTarget.gameObject.GetComponent<RevolverBullet>();
            thisBullet.GetComponentInChildren<Collider>().enabled = false;
            _bullets.Add(thisBullet.Bullet);
            _cassings.Add(thisBullet.Cassing);

            if (_currentBulletIndex != 5)
            {
                _colliders[_currentBulletIndex].enabled = false;
                _colliders[_currentBulletIndex + 1].enabled = true;
            }
        }            
    }

    public void EjectCassings()
    {
        if (_isInReloadState)
        {   
            for (int i=5; i>=0; i--)
            {                
                if(i > _bullets.Count-1)
                {                       
                    try
                    {
                        if (i <= _cassings.Count - 1)
                        {
                            var casing = _cassings[i].gameObject;
                            _cassings.RemoveAt(i);
                            
                            _sockets[i].interactionManager.SelectExit(_sockets[i], _sockets[i].selectTarget);
                            casing.AddComponent<MeshCollider>().convex = true;
                            Rigidbody rb = casing.AddComponent<Rigidbody>();

                            Vector3 force = -casing.transform.forward * 3;
                            
                            rb.AddForce(force, ForceMode.Impulse);

                            StartCoroutine(DelaiDestroyCasing(casing));
                            
                        }                                           
                                             
                    }
                    catch(Exception ex) 
                    { 
                        Debug.Log(ex.ToString());
                    }
                }  
                if (i == _bullets.Count && _bullets.Count != 6)
                {
                    _colliders[i].enabled = true;                    
                }                  
            }
        }
        
    }



    public void HoldingGun(SelectEnterEventArgs args)
    {
        if(!args.interactorObject.transform.gameObject.GetComponent<XRRayInteractor>())
        {
            _isGunHeld = true;
            _currentController = args.interactorObject as XRBaseInteractor;
            if (_currentController.gameObject.tag == "LeftHand")
            {
                _animator.SetBool("IsInLeftHand", true);
            }
        }        
    }

    public void ReleasingGun()
    {
        _isGunHeld = false;
        _isInReloadState = false;
        _currentController = null;
        _animator.SetBool("IsInReloadState", false);
        _animator.SetBool("IsInLeftHand", false);
        transform.localScale = new Vector3(1f, 1f, 1f);
        if (this.gameObject.GetComponent<Rigidbody>() != null)
        {
            this.gameObject.GetComponent<Rigidbody>().useGravity = true;
            this.gameObject.GetComponent<Rigidbody>().isKinematic = false;
        }
    }

    private AudioClip GetAudioClip()
    {       
        int _randomClip = Random.Range(0, _clipList.Count);
        return _clipList[_randomClip]; 
    }

    public void ShootRaycast()
    {
        if (!_isInReloadState && _bullets.Count != 0 && _isGunHeld)
        {
            Vector3 raycastPosition = _raycastOrigin.transform.position;
            ShootBullet();
            InstantiateGunshot(raycastPosition);
            InstantiateMuzzleFlash(raycastPosition);

            Vector3 _forward = _raycastOrigin.transform.forward * 100;
            RaycastHit hit;
            Debug.DrawRay(transform.position, _forward, Color.green, 60f, true);
            if (Physics.Raycast(raycastPosition, _forward, out hit, 1000))
            {
                GameObject _hitTarget = hit.collider.gameObject;

                if (_hitTarget.tag == "Ennemy_Weakspot")
                {
                    _hitTarget.GetComponentInParent<EnemyBase>().Target_hit_head(_damage);
                }

                if (_hitTarget.tag == "Ennemy_Regularspot")
                {
                    _hitTarget.GetComponentInParent<EnemyBase>().Target_hit_body(_damage);
                }

                if(_hitTarget.tag == "Destroyable")
                {
                    _hitTarget.GetComponent<Destructible>().Break();
                }
                
            }
        }
        else
        {
            ShootBlank();
        }
    }

    private void ShootBullet()
    {        
        if (!_isInReloadState)
        {
            _animator.SetTrigger("IsShooting");
            Destroy(_bullets[_currentBulletIndex]);
            _bullets.RemoveAt(_currentBulletIndex);  
            _currentBulletIndex--;
        }        
    }

    private void ShootBlank()
    {        
        if (!_isInReloadState)
        {
            _animator.SetTrigger("IsShooting");
        }        
    }

    private IEnumerator DelaiDestroyCasing(GameObject casing)
    {
        yield return new WaitForSeconds(20);
        Destroy(casing.transform.parent.parent.gameObject);        
    }
    
    private void InstantiateGunshot(Vector3 location)
    {
        GameObject _audioGunShot = Instantiate(_gunshotAudioSource);
        _audioGunShot.transform.position = location;        
        _audioGunShot.GetComponent<AudioSource>().PlayOneShot(GetAudioClip());

        StartCoroutine(DelaiDestroyAudio(_audioGunShot));
    }

    private void InstantiateOtherAudio(string type, Vector3 location)
    {
        if(type == "Open")
        {
            GameObject _audio = Instantiate(_gunshotAudioSource);
            _audio.transform.position = location;
            _audio.GetComponent<AudioSource>().PlayOneShot(_openRevolver);
            StartCoroutine(DelaiDestroyAudio(_audio));
        }
        if(type == "Close")
        {
            GameObject _audio = Instantiate(_gunshotAudioSource);
            _audio.transform.position = location;
            _audio.GetComponent<AudioSource>().PlayOneShot(_closeRevolver);
            StartCoroutine(DelaiDestroyAudio(_audio));
        }
        if(type == "Add")
        {
            GameObject _audio = Instantiate(_gunshotAudioSource);
            _audio.transform.position = location;
            _audio.GetComponent<AudioSource>().PlayOneShot(_addBullet);
            StartCoroutine(DelaiDestroyAudio(_audio));
        }
    }

    private IEnumerator DelaiDestroyAudio(GameObject _audioSource)
    {
        yield return new WaitForSeconds(3);
        Destroy(_audioSource);
    }

    private void InstantiateMuzzleFlash(Vector3 location)
    {
        GameObject _flash = Instantiate(_muzzleFlash);
        _flash.transform.position = location;
        _flash.transform.forward = _raycastOrigin.transform.forward;
        _flash.GetComponent<ParticleSystem>().Play();

        StartCoroutine(DelaiDestroyParticle(_flash));
    }

    private IEnumerator DelaiDestroyParticle(GameObject _particleSource)
    {
        yield return new WaitForSeconds(2);
        Destroy(_particleSource);
    }
}
