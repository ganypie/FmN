using UnityEngine;

public interface IInteractable
{
    // Проверяет, можно ли взаимодействовать в текущий момент
    bool CanInteract(Transform interactor);

    // Действие при взаимодействии
    void Interact(Transform interactor);
}
