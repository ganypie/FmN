using UnityEngine;
using System.Collections.Generic;

// Система управления предметами в руках игрока
// Левая рука: только фонарик
// Правая рука: фонарик, генератор, дрова (макс 3), другие предметы
public class HeldItem : MonoBehaviour
{
    [Header("Left Hand")]
    public GameObject CurrentItemLeft { get; private set; }  // левая рука (фонарик)

    [Header("Right Hand")]
    public GameObject CurrentItem { get; private set; }      // основной предмет в правой руке
    private List<GameObject> woodInRightHand = new List<GameObject>(); // дрова в правой руке (макс 3)

    /// <summary>Поднять предмет в правую руку (обычный предмет)</summary>
    public void Pickup(GameObject item)
    {
        // Если в правой руке уже есть дрова — не заменяем их
        if (woodInRightHand.Count > 0)
        {
            Debug.Log("[HeldItem] Right hand is occupied by wood. Cannot pick up other items.");
            return;
        }

        CurrentItem = item;
        Debug.Log($"[HeldItem] Picked up (right hand): {item.name}");
    }

    /// <summary>Поднять полено дров (специальный метод)</summary>
    public bool PickupWood(GameObject woodItem)
    {
        // Если в правой руке есть предмет, не дрова — выбросить его
        if (CurrentItem != null && woodInRightHand.Count == 0)
        {
            Drop();
        }

        // Если уже есть 3 полена — отказываем
        if (woodInRightHand.Count >= 3)
        {
            Debug.Log("[HeldItem] Already holding 3 wood pieces. Cannot pickup more.");
            return false;
        }

        woodInRightHand.Add(woodItem);
        CurrentItem = woodItem; // Последнее поднятое полено считается "текущим"

        // Позиционируем полено в руке в зависимости от количества уже поднятых
        PositionWoodInHand(woodItem, woodInRightHand.Count);

        Debug.Log($"[HeldItem] Wood picked up (right hand). Count: {woodInRightHand.Count}/3");
        return true;
    }

    /// <summary>Позиционировать полено в руке в зависимости от его номера (1, 2, 3)</summary>
    private void PositionWoodInHand(GameObject woodItem, int woodIndex)
    {
        // Если у полена есть PickupableItem — используем его смещение
        var pickupable = woodItem.GetComponent<PickupableItem>();
        if (pickupable != null)
        {
            // Смещаем полено вверх/в сторону в зависимости от индекса
            // 1-е полено: в центре (базовая позиция)
            // 2-е полено: рядом (чуть в сторону и выше)
            // 3-е полено: поверх (ещё выше)
            Vector3 offset = Vector3.zero;
            switch (woodIndex)
            {
                case 1:
                    offset = new Vector3(0, 0, 0);           // первое полено в центре
                    break;
                case 2:
                    offset = new Vector3(0.2f, 0.1f, 0);     // второе полено рядом (вправо и чуть вверх)
                    break;
                case 3:
                    offset = new Vector3(-0.2f, 0.25f, 0);   // третье полено поверх (влево и выше)
                    break;
            }
            pickupable.SetHandOffset(offset);
            pickupable.UpdateHandPosition();
            Debug.Log($"[HeldItem] Positioned wood #{woodIndex} with offset {offset}");
        }
    }

    /// <summary>Проверить, держим ли мы дрова</summary>
    public bool IsHoldingWood()
    {
        return woodInRightHand.Count > 0;
    }

    /// <summary>Получить количество дров в руке</summary>
    public int GetWoodCount()
    {
        return woodInRightHand.Count;
    }

    /// <summary>Бросить/убрать предмет из правой руки (обычный)</summary>
    public void Drop()
    {
        if (woodInRightHand.Count == 0)
        {
            CurrentItem = null;
            Debug.Log("[HeldItem] Dropped item (right hand)");
        }
        else
        {
            Debug.Log("[HeldItem] Cannot drop: holding wood. Wood is stuck until placed in fireplace.");
        }
    }

    /// <summary>Бросить одно полено (при размещении в камине)</summary>
    public void DropWood(GameObject woodItem)
    {
        if (woodInRightHand.Contains(woodItem))
        {
            woodInRightHand.Remove(woodItem);
            CurrentItem = woodInRightHand.Count > 0 ? woodInRightHand[woodInRightHand.Count - 1] : null;
            Debug.Log($"[HeldItem] Wood dropped. Count: {woodInRightHand.Count}/3");
        }
    }

    /// <summary>Очистить все дрова (при выполнении задачи)</summary>
    public void ClearAllWood()
    {
        woodInRightHand.Clear();
        CurrentItem = null;
        Debug.Log("[HeldItem] All wood cleared");
    }

    /// <summary>Поднять фонарик в левую руку</summary>
    public void PickupFlashlight(GameObject flashlight)
    {
        CurrentItemLeft = flashlight;
        Debug.Log($"[HeldItem] Picked up flashlight (left hand): {flashlight.name}");
    }

    /// <summary>Бросить фонарик из левой руки</summary>
    public void DropFlashlight()
    {
        CurrentItemLeft = null;
        Debug.Log("[HeldItem] Dropped flashlight (left hand)");
    }
}
