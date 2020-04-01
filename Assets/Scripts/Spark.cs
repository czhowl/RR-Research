using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spark : MonoBehaviour
{
    ParticleSystem ps;
    void Start()
    {
        ps = GetComponent<ParticleSystem>();
    }

    void Update()
    {
        if(!ps.IsAlive()){     // self destruction
            Destroy(gameObject);
        }
    }
}
