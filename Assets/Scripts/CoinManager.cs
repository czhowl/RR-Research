using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinManager : MonoBehaviour
{
    public GameObject coin;
    public GameObject smoke;
    List<Transform> coins;
    public List<Transform> Coins{
        get { return coins; }
    }
    // Start is called before the first frame update
    void Start()
    {
        coins = new List<Transform>();   
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0)){
            Vector3 randomPos = new Vector3(Random.Range(-1.0f, 1.0f), 0.0f, Random.Range(-1.0f, 1.0f));
            randomPos = Vector3.Normalize(randomPos) * Random.Range(0.0f, 3.0f);
            Vector3 randomHeight = new Vector3(0.0f, 2.0f + Random.Range(-1.0f, 1.0f), 0.0f);
            coins.Add(Instantiate(coin, randomPos + randomHeight, Random.rotation).transform);
            Instantiate(smoke, randomPos + randomHeight, Random.rotation);
        }
        Coins.RemoveAll(x => x == null);
    }
}
