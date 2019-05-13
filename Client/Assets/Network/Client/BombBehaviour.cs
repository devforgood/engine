using System.Collections;
using TanksMP;
using UnityEngine;

public class BombBehaviour : MonoBehaviour
{
    public GameObject explosionPrefab;
    public LayerMask levelMask;


    public void Explode()
    {
        //Create a first explosion at the bomb position
        GameObject go = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        //For every direction, start a chain of explosions
        StartCoroutine(CreateExplosions(Vector3.forward));
        StartCoroutine(CreateExplosions(Vector3.right));
        StartCoroutine(CreateExplosions(Vector3.back));
        StartCoroutine(CreateExplosions(Vector3.left));
    }

    private IEnumerator CreateExplosions(Vector3 direction)
    {
        for (int i = 1; i < 5 + 1; ++i)
        { //The 3 here dictates how far the raycasts will check, in this case 3 tiles far
            Physics.Raycast(transform.position + new Vector3(0, .5f, 0), direction, out RaycastHit hit, i, levelMask); //Raycast in the specified direction at i distance, because of the layer mask it'll only hit blocks, not players or bombs

            if (!hit.collider)
            { // Free space, make a new explosion
                GameObject go = Instantiate(explosionPrefab, transform.position + (i * direction), explosionPrefab.transform.rotation);

                //DestroyCheckDown(go.transform.position);
            }
            else
            { //Hit a block, stop spawning in this direction
                //DestroyBlock(hit);
                break;
            }
            yield return new WaitForSeconds(.05f); //Wait 50 milliseconds before checking the next location
        }

    }



}
