// --------------------------------------
// This script is totally optional. It is an example of how you can use the
// destructible versions of the objects as demonstrated in my tutorial.
// Watch the tutorial over at http://youtube.com/brackeys/.
// --------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destructible : MonoBehaviour {

    [SerializeField] private GameObject[] destroyedVersions = default;
	[SerializeField] private GameObject audioSource = default;
	public GameObject _player = default;

    private GameObject box;
	private GameObject audio;

	public void Break()
	{

        int randomNumber = 0;
        randomNumber = Random.Range(0, destroyedVersions.Length);

        if (_player.GetComponent<Player>()._vieActuelle == _player.GetComponent<Player>()._vieMaximale)
		{
            int randomNumberBullet = Random.Range(1, destroyedVersions.Length);
            box = Instantiate(destroyedVersions[randomNumberBullet], transform.position, transform.rotation);
		}
		else
		{
            box = Instantiate(destroyedVersions[randomNumber], transform.position, transform.rotation);
        }
		
		//audio = Instantiate(audioSource, transform.position, transform.rotation);
		// Remove the current object
		StartCoroutine(Destroy());
    }

	private IEnumerator Destroy()
	{
		gameObject.GetComponent<Collider>().enabled = false;
		gameObject.GetComponent<MeshRenderer>().enabled = false;
		yield return new WaitForSeconds(5);
		Destroy(box);		
		Destroy(audio);
		Destroy(gameObject);
    }

}
