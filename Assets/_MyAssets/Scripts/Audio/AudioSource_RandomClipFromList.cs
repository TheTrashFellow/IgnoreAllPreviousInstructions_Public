using System.Collections.Generic;
using UnityEngine;

public class AudioSource_RandomClipFromList : MonoBehaviour
{
    [SerializeField] private List<AudioClip> clipList;
    
    private int _randomClip;
    private void Awake()
    {
        Switch();
    }

    public void Switch()
    {
        _randomClip = Random.Range(0, clipList.Count);
        this.gameObject.GetComponent<AudioSource>().clip = clipList[_randomClip];
    }

    public AudioClip GetClip()
    {
        return clipList[_randomClip];
    }
}
