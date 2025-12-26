using UnityEngine;
using System;

/// <summary>
/// Singleton для отслеживания количества поднятых полен в правой руке игрока.
/// Максимум 3 полена. Дрова "прилипают" и не выбрасываются до выполнения задачи.
/// </summary>
public class WoodCollector : MonoBehaviour
{
    public static WoodCollector Instance { get; private set; }

    [SerializeField]
    private int woodCount = 0;
    private const int MAX_WOOD = 3;

    /// <summary>Событие при изменении счётчика дров (новое количество)</summary>
    public event Action<int> OnWoodCountChanged;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        woodCount = 0;
    }

    /// <summary>Добавить одно полено (если не превышен лимит)</summary>
    public bool AddWood()
    {
        if (woodCount >= MAX_WOOD)
        {
            Debug.Log("[WoodCollector] Maximum wood count reached (3)");
            return false;
        }
        woodCount++;
        Debug.Log($"[WoodCollector] Wood added. Current count: {woodCount}/{MAX_WOOD}");
        OnWoodCountChanged?.Invoke(woodCount);
        return true;
    }

    /// <summary>Удалить одно полено (при выбросе)</summary>
    public void RemoveWood()
    {
        if (woodCount > 0)
        {
            woodCount--;
            Debug.Log($"[WoodCollector] Wood removed. Current count: {woodCount}/{MAX_WOOD}");
            OnWoodCountChanged?.Invoke(woodCount);
        }
    }

    /// <summary>Проверить, собрано ли ровно N полен</summary>
    public bool HasCollectedWood(int count)
    {
        return woodCount >= count;
    }

    /// <summary>Получить текущее количество дров</summary>
    public int GetWoodCount()
    {
        return woodCount;
    }

    /// <summary>Получить оставшееся количество дров до лимита</summary>
    public int GetRemainingWood()
    {
        return Mathf.Max(0, MAX_WOOD - woodCount);
    }

    /// <summary>Сброс счётчика (при рестарте уровня)</summary>
    public void Reset()
    {
        woodCount = 0;
        OnWoodCountChanged?.Invoke(0);
    }
}
