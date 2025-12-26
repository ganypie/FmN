using UnityEngine;

/// <summary>
/// Отдельный singleton-скрипт для отслеживания факта открытия двери.
/// Привешивается на пустой GameObject в сцене (например, "DoorWatcher").
/// </summary>
public class DoorWatcher : MonoBehaviour
{
    public static DoorWatcher Instance { get; private set; }

    [SerializeField]
    private bool doorOpened = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>Вызывается при открытии двери (например, SimpleDoor)</summary>
    public void DoorOpened()
    {
        doorOpened = true;
    }

    /// <summary>Возвращает, была ли дверь открыта</summary>
    public bool HasDoorBeenOpened()
    {
        return doorOpened;
    }

    /// <summary>Сброс состояния (можно использовать при рестарте уровня)</summary>
    public void Reset()
    {
        doorOpened = false;
    }
}
