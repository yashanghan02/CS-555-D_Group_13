using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControls : MonoBehaviour
{
    Vector2 initial, final;
    public List<Vector3> Positions;
    int positionIndex;
    public float walk_speed = 5;
    public float turnspeed = 1;
    public bool b_walk;
    public Animator[] anim;
    public bool isMale, isFemale;

    public playerCloth[] FemaleClothes, MaleCloths;
    // Start is called before the first frame update
    void Start()
    {
        positionIndex = 1; //middle
    }
    void walk()
    {
        if (b_walk)
        {
            transform.Translate(0, 0, walk_speed * Time.deltaTime);
        }
    }
    // Update is called once per frame
    void Update()
    {
        walk();
        
        if (Input.GetMouseButtonDown(0))
        {
            initial = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            final = Input.mousePosition;
            checkSwipe();
        }

        this.transform.position = Vector3.Lerp(this.transform.position,new Vector3(Positions[positionIndex].x,transform.position.y,transform.position.z),turnspeed*Time.deltaTime);
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
[System.Serializable]

public class playerCloth
{
    public GameObject Torso, Legs, Head, Shoes;
}
