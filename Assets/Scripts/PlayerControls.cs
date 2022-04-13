using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum clothType
{
    naked,ceo,elegant,casual
}

public class PlayerControls : MonoBehaviour
{
    Vector2 initial, final;
    public List<Vector3> Positions;
    public clothType current;
    int positionIndex;
    public float walk_speed = 5;
    public float turnspeed = 1;
    public bool b_walk;
    public Animator[] anim;
    public bool isMale, isFemale;
    [SerializeField]
    int Age = 99;

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
            SetDynamicSpeed();
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

    void SetDynamicSpeed()
    {
        if (Age>=90) {
            setAnimBlend("Blend", 0f);
            SetAnim(true, "Tired");
            walk_speed = 3;
            
        }
        if(Age < 90 && Age >= 80)
        {
            setAnimBlend("Blend", 0f);
            SetAnim(true, "Tired");walk_speed = 4;
            
        }

        if(Age<80 && Age>= 60)
        {
            setAnimBlend("Blend", .5f);
            SetAnim(false, "Walk", true);
            walk_speed = 5;
           

        }
        if (Age < 60 && Age >= 40)
        {
            SetAnim(true, "Blending");
            SetAnim(false, "Walk",true);
          
            setAnimBlend("Blend", .5f);
            walk_speed = 7;
           
        }
        if(Age<40 && Age >= 20)
        {
            SetAnim(true, "Walk", false) ;
            setAnimBlend("Blend", 1);
            walk_speed = 8;
        }
    }

    

    void SetAnim(bool trigger, string value,bool boolean=false)
    {
        if (!trigger)
        {
            foreach(var x in anim)
            {
                x.SetBool(value, boolean);
            }
        }
        else
        {
            foreach (var x in anim)
            {
                x.SetTrigger(value);
            }
        }
    }

    void setAnimBlend(string value,float blendvalue)
    {
        foreach (var x in anim)
        {
            x.SetFloat(value, blendvalue);
        }
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


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Collectible"))
        {
            var item = other.GetComponent<Item>();
            if (item.isGood)
            {
                Age-=item.thisItemHealth;
            }
            else
            {
                Age += item.thisItemHealth;
            }
            UIManager.instance.SendScore(Age);
            Destroy(other.gameObject);
        }
    }


}
[System.Serializable]

public class playerCloth
{
    public clothType type;
    public GameObject Torso, Legs, Head, Shoes;
}
