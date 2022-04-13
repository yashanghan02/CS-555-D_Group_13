using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{

    public bool isGood;

    public GameObject[] itemModel;
    public int[] HealthAffect;
    public int thisItemHealth;
    // Start is called before the first frame update
    private void Start()
    {
        int randomVal = Random.Range(0, itemModel.Length - 1);
        Instantiate(itemModel[randomVal], this.transform);
        thisItemHealth = HealthAffect[randomVal];
    }   

}
    
