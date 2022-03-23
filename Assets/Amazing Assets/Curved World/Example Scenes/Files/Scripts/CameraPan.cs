using UnityEngine;
using System.Collections;

namespace AmazingAssets.CurvedWorld.Example
{
    public class CameraPan : MonoBehaviour
    {
        public float moveSpeed = 1;
        
        void Update()
        {
            Vector3 newPos = transform.position + new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")) * moveSpeed * Time.deltaTime;

            newPos.x = Mathf.Clamp(newPos.x, -35, 35f);
            newPos.z = Mathf.Clamp(newPos.z, -35, 35f);


            transform.position = newPos;
        }
    }
}