using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

	public void UpdatePosition(Vector2 position)
    {
        transform.position = new Vector3(position.x + 0.5f, position.y + 0.5f, transform.position.z);
    }
}
