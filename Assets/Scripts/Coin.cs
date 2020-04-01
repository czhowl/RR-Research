using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
    void OnCollisionEnter ()  //Plays Sound Whenever collision detected
    {
        GetComponent<AudioSource>().Play ();
    }

    void Update(){
        if(transform.position.y < -10.0f) Destroy(gameObject);
    }
}
