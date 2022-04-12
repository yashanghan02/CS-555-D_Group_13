using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{

    public bool isGood;

    public GameObject[] itemModel;
    public int[] HealthAffect;
    // Start is called before the first frame update
    private void Start()
    {
        Instantiate(itemModel[Random.Range(0,itemModel.Length-1)], this.transform);
    }

}
    
