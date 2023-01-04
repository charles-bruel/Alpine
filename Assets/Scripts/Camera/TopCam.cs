using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TODO: Polish this
public class TopCam : MonoBehaviour
{
    private Vector2 startScreenPos;
    private Vector3 startWorldPos;
    private int ticks = 0;
    private Camera Camera;

    void Start() {
        Camera = GetComponent<Camera>();
    }

    void LateUpdate() {
        if(Input.GetMouseButton(0)) {
            if(ticks++ == 0) {
                startScreenPos = Transform(Input.mousePosition);
                startWorldPos = transform.position;
            } else {
                Vector2 dif = Transform(Input.mousePosition) - startScreenPos;
                transform.position = startWorldPos - new Vector3(dif.x, 0, dif.y);
            }
        } else {
            ticks = 0;
        }

        Camera.orthographicSize += -Input.mouseScrollDelta.y * 10;
        if(Camera.orthographicSize < 10) {
            Camera.orthographicSize = 10;
        }
        if(Camera.orthographicSize > 1000) {
            Camera.orthographicSize = 1000;
        }
    }

    private Vector2 Transform(Vector3 mp) {
        return ((Vector2) (mp) / Screen.height * Camera.orthographicSize * 2);
    }
}
