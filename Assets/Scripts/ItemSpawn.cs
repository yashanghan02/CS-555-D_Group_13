using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawn : MonoBehaviour
{
    // Start is called before the first frame update

    public Transform[] places;
    public GameObject[] GoodItems;
    public GameObject[] BadItems;
    public Transform player;

    public float startTime=3,interval=2;

    private void Start()
    {
        InvokeRepeating(nameof(Spawn), startTime, interval);
    }


    void Spawn()
    {
        foreach (var x in places)
        {
            int value = Random.Range(0, 9);
            if (value > 4)
            {
                Instantiate(GoodItems[Random.Range(0, GoodItems.Length - 1)],new Vector3(0,0,player.position.z)+ x.position,Quaternion.identity);

            }
            else
            {
                Instantiate(BadItems[Random.Range(0, BadItems.Length - 1)], new Vector3(0, 0, player.position.z) + x.position, Quaternion.identity);
            }
        }
    }
}
