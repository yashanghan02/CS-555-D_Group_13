using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtendPlatform : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    Transform EndPoint;
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            FindObjectOfType<WorldGenerator>().GenerateWorld(EndPoint.position);
        }
    }
}
