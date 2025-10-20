using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Простая система инвентаря игрока для управления поднятыми предметами
/// Позволяет иметь несколько предметов в "руках" одновременно
/// </summary>
public class PlayerInventory : MonoBehaviour
{
    [Header("Inventory Settings")]
    [SerializeField] private int maxItems = 3; // Максимальное количество предметов
    [SerializeField] private Transform[] handPositions; // Позиции для предметов в руках
    [SerializeField] private float itemSpacing = 0.3f; // Расстояние между предметами

    [Header("UI")]
    [SerializeField] private UnityEngine.UI.Text inventoryText; // Текст для отображения инвентаря

    // Список поднятых предметов
    private List<PickupableItem> inventory = new List<PickupableItem>();

    // Singleton для легкого доступа
    public static PlayerInventory Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Если не заданы позиции рук, создаем их автоматически
        if (handPositions == null || handPositions.Length == 0)
        {
            CreateDefaultHandPositions();
        }

        UpdateInventoryUI();
    }

    void Update()
    {
        // Не обрабатываем ввод инвентаря во время паузы
        if (PauseManager.Instance != null && PauseManager.Instance.IsPaused)
            return;
            
        // Проверяем нажатия клавиш для управления инвентарем
        HandleInventoryInput();
        
        // Проверяем нажатия клавиш 1-3 для выбора предметов
        HandleItemSelection();
    }

    private void CreateDefaultHandPositions()
    {
        // Создаем пустые объекты для позиций рук
        handPositions = new Transform[maxItems];
        GameObject handContainer = new GameObject("HandPositions");
        handContainer.transform.SetParent(transform);
        handContainer.transform.localPosition = new Vector3(0.3f, 0.5f, 0.5f); // Справа от игрока

        for (int i = 0; i < maxItems; i++)
        {
            GameObject handPos = new GameObject($"HandPosition_{i}");
            handPos.transform.SetParent(handContainer.transform);
            handPos.transform.localPosition = new Vector3(0, 0, i * itemSpacing);
            handPositions[i] = handPos.transform;
        }
    }

    private void HandleInventoryInput()
    {
        // Бросаем все предметы по нажатию G
        if (Input.GetKeyDown(KeyCode.G))
        {
            DropAllItems();
        }

        // Циклично переключаемся между предметами по нажатию Tab
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            CycleItems();
        }
    }

    /// <summary>
    /// Добавляет предмет в инвентарь
    /// </summary>
    public bool AddItem(PickupableItem item)
    {
        if (item == null || inventory.Count >= maxItems)
        {
            return false;
        }

        if (inventory.Contains(item))
        {
            return false; // Уже есть в инвентаре
        }

        inventory.Add(item);
        UpdateInventoryUI();
        return true;
    }

    /// <summary>
    /// Удаляет предмет из инвентаря
    /// </summary>
    public bool RemoveItem(PickupableItem item)
    {
        if (item == null || !inventory.Contains(item))
        {
            return false;
        }

        inventory.Remove(item);
        UpdateInventoryUI();
        return true;
    }

    /// <summary>
    /// Бросает все предметы из инвентаря
    /// </summary>
    public void DropAllItems()
    {
        // Создаем копию списка, чтобы избежать ошибок при изменении во время итерации
        var itemsToDrop = new List<PickupableItem>(inventory);

        foreach (var item in itemsToDrop)
        {
            if (item != null)
            {
                item.ForceDrop();
            }
        }

        inventory.Clear();
        UpdateInventoryUI();
    }

    /// <summary>
    /// Переключается между предметами в инвентаре
    /// </summary>
    private void CycleItems()
    {
        if (inventory.Count <= 1) return;

        // Перемещаем первый предмет в конец
        var firstItem = inventory[0];
        inventory.RemoveAt(0);
        inventory.Add(firstItem);

        // Обновляем позиции всех предметов
        UpdateItemPositions();
        UpdateInventoryUI();
    }

    /// <summary>
    /// Обновляет позиции всех предметов в руках
    /// </summary>
    private void UpdateItemPositions()
    {
        for (int i = 0; i < inventory.Count && i < handPositions.Length; i++)
        {
            if (inventory[i] != null && handPositions[i] != null)
            {
                inventory[i].transform.SetParent(handPositions[i]);
                inventory[i].transform.localPosition = Vector3.zero;
                inventory[i].transform.localRotation = Quaternion.identity;
            }
        }
    }

    /// <summary>
    /// Обновляет UI инвентаря
    /// </summary>
    private void UpdateInventoryUI()
    {
        if (inventoryText == null) return;

        string text = $"Инвентарь: {inventory.Count}/{maxItems}\n";
        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i] != null)
            {
                text += $"{i + 1}. {inventory[i].name}\n";
            }
        }

        inventoryText.text = text;
    }

    /// <summary>
    /// Получает предмет по индексу
    /// </summary>
    public PickupableItem GetItem(int index)
    {
        if (index >= 0 && index < inventory.Count)
        {
            return inventory[index];
        }
        return null;
    }

    /// <summary>
    /// Проверяет, есть ли место в инвентаре
    /// </summary>
    public bool HasSpace()
    {
        return inventory.Count < maxItems;
    }

    /// <summary>
    /// Получает количество предметов в инвентаре
    /// </summary>
    public int ItemCount => inventory.Count;

    /// <summary>
    /// Получает максимальное количество предметов
    /// </summary>
    public int MaxItems => maxItems;

    /// <summary>
    /// Обрабатывает нажатия клавиш 1-3 для выбора предметов
    /// </summary>
    private void HandleItemSelection()
    {
        if (inventory.Count == 0) return;

        // Проверяем нажатия клавиш 1-3
        for (int i = 1; i <= 3 && i <= inventory.Count; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i))
            {
                SelectItem(i - 1); // Индексы начинаются с 0
            }
        }
    }

    /// <summary>
    /// Выбирает предмет по индексу
    /// </summary>
    private void SelectItem(int index)
    {
        if (index < 0 || index >= inventory.Count) return;

        var item = inventory[index];
        if (item == null) return;

        // Перемещаем выбранный предмет в начало списка
        inventory.RemoveAt(index);
        inventory.Insert(0, item);

        // Обновляем позиции всех предметов
        UpdateItemPositions();
        UpdateInventoryUI();

        Debug.Log($"Selected item: {item.name}");
    }
}
