using UnityEngine;

public class RainFollower : MonoBehaviour
{
    public Transform player;

    void LateUpdate()
    {
        if (player != null)
        {
            Vector3 pos = player.position;
            pos.y = 12; // чтобы дождь всегда оставался на уровне мира, а не летал за головой
            transform.position = pos;
        }
    }
}
