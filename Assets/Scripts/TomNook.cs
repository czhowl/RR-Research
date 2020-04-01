using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TomNook : MonoBehaviour
{
    public Transform goal;
    NavMeshAgent nav;
    Animator anim;
    CoinManager coinManager;
    void Start () {
        nav = GetComponent<NavMeshAgent>();
        nav.updateRotation = false;
        anim = GetComponent<Animator>();
        // nav.destination = goal.position; 
    }

    void Update (){
        if(coinManager == null) coinManager = Object.FindObjectOfType<CoinManager>();
        float closestDist = Mathf.Infinity;
        Transform closestCoin = null;
        // print(coinManager.Coins);
        foreach(Transform t in coinManager.Coins){
            if(t != null && Vector3.Distance(t.position, transform.position) < closestDist){
                closestDist = Vector3.Distance(t.position, transform.position);
                closestCoin = t;
            }
        }
        if(closestCoin != null) nav.destination = closestCoin.position; 

        float blend = Mathf.Clamp(nav.velocity.magnitude/nav.speed, 0.0f, 1.0f);
        anim.SetFloat("Blend", blend);
        if(nav.velocity.sqrMagnitude > Mathf.Epsilon)
        {
            transform.rotation = Quaternion.LookRotation(nav.velocity.normalized);
        }
    }

    void OnCollisionEnter(Collision other) {
        if(other.gameObject.CompareTag("coin")){
            print("get coin");
            Destroy(other.gameObject);
        }
    }
}
