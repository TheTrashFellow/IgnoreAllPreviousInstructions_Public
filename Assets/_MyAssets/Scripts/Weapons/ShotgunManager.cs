using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using Random = UnityEngine.Random;

public class ShotgunManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject _raycastOrigin = default;
    [SerializeField] private GameObject _muzzleFlash = default;

    [Space]
    [SerializeField] private Animator _animator = default;
    [SerializeField] private List<XRSocketInteractor> _sockets = default;
    [SerializeField] private List<SphereCollider> _colliders = default;
    [SerializeField] private List<GameObject> _shells = default;

    [Space]
    [Header("Audio")]
    [SerializeField] private GameObject _gunshotAudioSource = default;
    [SerializeField] private AudioClip _gunshot = default;
    [SerializeField] private AudioClip _shotgunClose = default;
    [SerializeField] private AudioClip _shotgunOpen = default;
    [SerializeField] private AudioClip _shotgunAdd = default;

    [Space]
    [Header("InputActionReferences")]
    [SerializeField] private InputActionReference _leftHandInput = default;
    [SerializeField] private InputActionReference _rightHandInput = default;

    [Space]
    [Header("Gameplay values")]
    [SerializeField] private int _damage = 1;
    [SerializeField] private int _pellets = 20;

    private bool _isGunHeld = false;
    private bool _isInReloadState = false;
    private XRBaseInteractor _currentController;

    private int _currentShellIndex = 1;
    private bool _areBulletsAssigned = false;   
    private List<GameObject> _Shells = new List<GameObject>();

    private void Start()
    {
        AssignBullets();
    }

    private void AssignBullets()
    {
        _Shells.Clear();
        int index = 0;
        foreach (var socket in _sockets)
        {
            try
            {
                IXRSelectInteractable obj = socket.GetOldestInteractableSelected();
                ShotgunShell thisShell = obj.transform.gameObject.GetComponent<ShotgunShell>(); //socket.selectTarget.gameObject.GetComponent<ShotgunShell>();
                _Shells.Add(thisShell.gameObject);
            }
            catch {
                
            }
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
            InstantiateAudio("Open", gameObject.transform.position);
            _isInReloadState = true;
            _animator.SetBool("IsInReloadState", true);
        }
        else
        {
            InstantiateAudio("Close", gameObject.transform.position);
            _isInReloadState = false;
            _animator.SetBool("IsInReloadState", false);

            foreach (Collider collider in _colliders)
            {
                collider.enabled = false;
            }
        }
    }

    public void InsertShell()
    {
        InstantiateAudio("Add", gameObject.transform.position);
        if (_areBulletsAssigned)
        {
            _currentShellIndex++;
            ShotgunShell thisShell = _sockets[_currentShellIndex].selectTarget.gameObject.GetComponent<ShotgunShell>();
            thisShell.GetComponentInChildren<Collider>().enabled = false;
            thisShell.GetComponent<Rigidbody>().isKinematic = true;
            _Shells.Add(thisShell.gameObject);            

            if (_currentShellIndex != 1)
            {
                _colliders[_currentShellIndex].enabled = false;
                _colliders[_currentShellIndex + 1].enabled = true;
            }
        }
    }

    public void ShootRaycast()
    {
        if (!_isInReloadState && _currentShellIndex != -1 && _isGunHeld)
        {
            Vector3 raycastPosition = _raycastOrigin.transform.position;
            float spread = 15f;                       
            ShootBullet();
            InstantiateAudio("Shoot", raycastPosition);
            InstantiateMuzzleFlash(raycastPosition);
            Vector3 _forward = _raycastOrigin.transform.forward * 100;

            for (int i=0; i < _pellets; i++)
            {
                Vector3 randomDirection = GetRandomSpreadDirection(spread, _forward);

                RaycastHit hit;
                Debug.DrawRay(transform.position, randomDirection, Color.green, 60f, true);
                if (Physics.Raycast(raycastPosition, randomDirection, out hit, 1000))
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

                    if (_hitTarget.tag == "Destroyable")
                    {
                        _hitTarget.GetComponent<Destructible>().Break();
                    }
                    Debug.Log(_hitTarget.name);
                }
            }            
            
        }
        else
        {
            ShootBlank();
        }
    }

    private Vector3 GetRandomSpreadDirection(float spreadAngle, Vector3 forward)
    {
        Quaternion randomRotation = Quaternion.Euler(
           Random.Range(-spreadAngle, spreadAngle), // Random pitch
           Random.Range(-spreadAngle, spreadAngle), // Random yaw
           0f // Keep roll at 0 for a consistent spread
       );
        return randomRotation * forward; // Rotate the barrel's forward vector
    }

    private void ShootBullet()
    {
        if (!_isInReloadState)
        {
            _Shells[_currentShellIndex].GetComponent<ShotgunShell>().OnShot();
            
            //_animator.SetTrigger("IsShooting");
            //_Shells.RemoveAt(_currentShellIndex);  
            _currentShellIndex--;
        }
    }

    private void ShootBlank()
    {
        if (!_isInReloadState)
        {
            _animator.SetTrigger("IsShooting");
        }
    }

    public void EjectShells()
    {
        if (_isInReloadState)
        {
            for (int i = _Shells.Count; i >= 0; i--)
            {
                if(i > _currentShellIndex)
                {                    
                    try
                    {
                        if (i == _Shells.Count - 1)
                        {
                            _sockets[i].interactionManager.SelectExit(_sockets[i], _sockets[i].selectTarget);

                            
                            //_Shells[i].GetComponent<MeshCollider>().enabled = true;
                            Rigidbody rb = _Shells[i].GetComponent<Rigidbody>();
                            Vector3 force = -_Shells[i].transform.forward * 3;

                            rb.isKinematic = false;
                            rb.AddForce(force, ForceMode.Impulse);

                            GameObject shell = _Shells[i];
                            _Shells.RemoveAt(i);

                            StartCoroutine(DestroyShell(shell));                            
                        }
                        
                    }
                    catch { }
                    
                }                
            }
            if(_currentShellIndex != 1)
            {
                _colliders[_currentShellIndex + 1].enabled = true;
            }            
        }

    }

    public void HoldingGun(SelectEnterEventArgs args)
    {
        if (!args.interactorObject.transform.gameObject.GetComponent<XRRayInteractor>())
        {
            _isGunHeld = true;
            _currentController = args.interactorObject as XRBaseInteractor;
        }           
    }
   

    public void ReleasingGun()
    {
        _isGunHeld = false;
        _isInReloadState = false;
        _currentController = null;
        _animator.SetBool("IsInReloadState", false);        
        transform.localScale = new Vector3(1f, 1f, 1f);
        if (this.gameObject.GetComponent<Rigidbody>() != null)
        {
            this.gameObject.GetComponent<Rigidbody>().useGravity = true;
            this.gameObject.GetComponent<Rigidbody>().isKinematic = false;
        }
    }

    private void InstantiateAudio(string type, Vector3 location)
    {
        if (type == "Shoot")
        {
            GameObject _audio = Instantiate(_gunshotAudioSource);
            _audio.transform.position = location;
            _audio.GetComponent<AudioSource>().PlayOneShot(_gunshot);
            StartCoroutine(DelaiDestroyAudio(_audio));
        }
        if (type == "Open")
        {
            GameObject _audio = Instantiate(_gunshotAudioSource);
            _audio.transform.position = location;
            _audio.GetComponent<AudioSource>().PlayOneShot(_shotgunOpen);
            StartCoroutine(DelaiDestroyAudio(_audio));

        }
        if (type == "Close")
        {
            GameObject _audio = Instantiate(_gunshotAudioSource);
            _audio.transform.position = location;
            _audio.GetComponent<AudioSource>().PlayOneShot(_shotgunClose);
            StartCoroutine(DelaiDestroyAudio(_audio));
        }
        if (type == "Add")
        {
            GameObject _audio = Instantiate(_gunshotAudioSource);
            _audio.transform.position = location;
            _audio.GetComponent<AudioSource>().PlayOneShot(_shotgunAdd);
            StartCoroutine(DelaiDestroyAudio(_audio));
        }
    }

    private IEnumerator DestroyShell(GameObject shell)
    {
        yield return new WaitForSeconds(20);
        Destroy(shell.gameObject);
    }

    private void InstantiateMuzzleFlash(Vector3 location)
    {
        GameObject _flash = Instantiate(_muzzleFlash);
        _flash.transform.position = location;
        _flash.transform.forward = _raycastOrigin.transform.forward;
        _flash.GetComponent<ParticleSystem>().Play();

        StartCoroutine(DelaiDestroyParticle(_flash));
    }

    private IEnumerator DelaiDestroyParticle(GameObject particuleSource)
    {
        yield return new WaitForSeconds(3);
        Destroy(particuleSource);
    }

    private IEnumerator DelaiDestroyAudio(GameObject _audioSource)
    {
        yield return new WaitForSeconds(3);
        Destroy(_audioSource);
    }
}
