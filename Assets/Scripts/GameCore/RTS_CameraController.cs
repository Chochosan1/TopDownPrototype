using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chochosan
{
    public class RTS_CameraController : MonoBehaviour
    {
        private Transform thisTransform;
        [SerializeField] Transform initialCameraSpawnPoint;
        public float panSpeed;
        public float scrollSpeed;
        public float smoothSpeed;

        [Tooltip("Offset from the screen border.")]
        public float panBorderThickness; //offset from the screen border

        [Header("Camera position limits")]
        [Tooltip("Maps the X and Z camera position limits. Y corresponds to Z in that case.")]
        public Vector2 panLimit;        //holds the X and Z limit values

        public float minY, maxY;

        private void Start()
        {
            //test
            thisTransform = gameObject.GetComponent<Transform>();
            Vector3 initialSpawnPos = new Vector3(initialCameraSpawnPoint.position.x, thisTransform.position.y, initialCameraSpawnPoint.position.z);
        }

        private void Update()
        {
            Vector3 pos = thisTransform.position;

            if (Input.mousePosition.y >= Screen.height - panBorderThickness) //top border
            {
                pos.z += panSpeed /** Time.deltaTime*/;
            }
            else if (Input.mousePosition.y <= panBorderThickness) //bottom border
            {
                pos.z -= panSpeed /** Time.deltaTime*/;
            }
            else if (Input.mousePosition.x >= Screen.width - panBorderThickness) //right border
            {
                pos.x += panSpeed /** Time.deltaTime*/;
            }
            else if (Input.mousePosition.x <= panBorderThickness) //left border
            {
                pos.x -= panSpeed /** Time.deltaTime*/;
            }



         //   if (!BuilderController.isPlacingBuilding) //must not currently be placing a building because buildings rotate using scroll wheel
       //     {
                float scroll = Input.GetAxis("Mouse ScrollWheel");
                pos.y -= scroll * scrollSpeed /** Time.deltaTime*/; //subtract from Y so that the scrolling is not reversed
         //   }


            //boundaries; the position can't go beyond the min/max values
            pos.x = Mathf.Clamp(pos.x, -panLimit.x, panLimit.x);
            pos.y = Mathf.Clamp(pos.y, minY, maxY);
            pos.z = Mathf.Clamp(pos.z, -panLimit.y, panLimit.y);

            transform.position = Vector3.Lerp(transform.position, pos, smoothSpeed * Time.deltaTime);
        }
    }
}
