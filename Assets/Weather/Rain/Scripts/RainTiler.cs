using UnityEngine;

public class RainTiler : MonoBehaviour
{
    public Transform player;
    public float tileSize = 50f; // размер тайла по X/Z

    void Update()
    {
        if (player == null) return;

        // Сдвигаем сетку к ближайшему центру тайла
        float newX = Mathf.Floor(player.position.x / tileSize) * tileSize;
        float newZ = Mathf.Floor(player.position.z / tileSize) * tileSize;

        transform.position = new Vector3(newX, 0f, newZ);
    }
}
