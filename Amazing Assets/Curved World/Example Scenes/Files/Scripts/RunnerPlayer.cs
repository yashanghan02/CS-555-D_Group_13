using UnityEngine;
using System.Collections;


namespace AmazingAssets.CurvedWorld.Example
{
    public class RunnerPlayer : MonoBehaviour
    {
        public enum SIDE { Left, Right }


        Vector3 initialPosition;
        Vector3 newPos;
        SIDE side;

        public KeyCode moveLeftKey = KeyCode.A;
        public KeyCode moveRightKey = KeyCode.D;

        Animation animationComp;
        public AnimationClip moveLeftAnimation;
        public AnimationClip moveRightAnimation;

        float translateOffset = 3.5f;


        void Start()
        {
            initialPosition = transform.position;

            side = SIDE.Left;
            newPos = transform.localPosition + new Vector3(0, 0, translateOffset);

            animationComp = GetComponent<Animation>();
        }
        
        void Update()
        {
            if (Input.GetKeyDown(moveLeftKey))
            {
                if (side == SIDE.Right)
                {
                    newPos = initialPosition + new Vector3(0, 0, translateOffset);
                    side = SIDE.Left;

                    animationComp.Play(moveLeftAnimation.name);
                }
            }
            else if (Input.GetKeyDown(moveRightKey))
            {
                if (side == SIDE.Left)
                {
                    newPos = initialPosition + new Vector3(0, 0, -translateOffset);
                    side = SIDE.Right;

                    animationComp.Play(moveRightAnimation.name);
                }
            }

            transform.localPosition = Vector3.Lerp(transform.localPosition, newPos, Time.deltaTime * 10);
        }
    }
}