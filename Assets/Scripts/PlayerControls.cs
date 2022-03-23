using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControls : MonoBehaviour
{
    Vector2 initial, final;
    public List<Vector3> Positions;
    int positionIndex;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            initial = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            final = Input.mousePosition;
            checkSwipe();
        }

        this.transform.position = Positions[positionIndex];
    }


    void checkSwipe()
    {
       
        if(initial.x > final.x)
        {
            Debug.Log("left");
            GoLeft();
        }
        else
        {
            Debug.Log("right");
            GoRight();
        }
    }

    public void GoLeft()
    {
        if (positionIndex > 0)
        {
            positionIndex--;
        }
    }

    public void GoRight()
    {
        if (positionIndex < Positions.Count-1)
        {
            positionIndex++;
        }
    }


    
}
