using UnityEngine;

public class DemoTaskSetup : MonoBehaviour
{
    public TaskManager taskManager;
    public Transform player;
    public LetterInteractable letter;
    public FlashlightController flashlight;
    public Generator generator;
    public PileItem woodPile;           // Используем новый PileItem вместо WoodPile
    public Fireplace fireplace;
    public Car car;
    public Transform home;
    public ScreenFaderInGame screenFaderInGame; // заменили FadeController

    void Start()
    {
        if (taskManager == null)
        {
            Debug.LogError("[DemoTaskSetup] Assign TaskManager in inspector!");
            return;
        }

        var ordered = new System.Collections.Generic.List<Task>()
        {
            new Task()
            {
                taskName = "Кто-то стучит в дверь",
                completionCondition = () => DoorWatcher.Instance != null && DoorWatcher.Instance.HasDoorBeenOpened()
            },
            new Task()
            {
                taskName = "У порога лежит письмо",
                completionCondition = () => letter == null
            },
            new Task()
            {
                taskName = "Фонарик лежит на столе",
                completionCondition = () => flashlight != null && flashlight.IsPickedUp()
            },
            new Task()
            {
                taskName = "Генератор заглох. Нужно заправить",
                completionCondition = () => generator != null && generator.IsFilled()
            },
            new Task()
            {
                taskName = "Дома холодно. Надо взять с собой полена",
                completionCondition = () => WoodCollector.Instance != null && WoodCollector.Instance.HasCollectedWood(3),
                // Динамическое обновление текста при изменении счётчика дров
                onTaskCreate = () =>
                {
                    if (WoodCollector.Instance != null)
                    {
                        WoodCollector.Instance.OnWoodCountChanged += UpdateWoodTaskText;
                        UpdateWoodTaskText(WoodCollector.Instance.GetWoodCount());
                    }
                }
            },
            new Task()
            {
                taskName = "Дровам место в камине",
                completionCondition = () => fireplace != null && fireplace.HasWoodBeenPlaced()
            },
            new Task()
            {
                taskName = "Автомобиль сигналит... странно",
                completionCondition = () => car != null && car.HasAlarmBeenChecked()
            },
            new Task()
            {
                taskName = "Беги",
                completionCondition = () => player != null && home != null && Vector3.Distance(player.position, home.position) < 2f,
                onComplete = () =>
                {
                    // Блокируем игрока и камеру
                    var movement = player.GetComponent<PlayerMovement>();
                    if (movement != null) movement.enabled = false;

                    var look = Camera.main != null ? Camera.main.GetComponent<MouseLook>() : null; // конкретно выключаем MouseLook
                    if (look != null) look.enabled = false;

                    // Запускаем плавный фейд на чёрный экран через ScreenFaderInGame
                    if (screenFaderInGame != null)
                        screenFaderInGame.StartCoroutine(screenFaderInGame.FadeOutCoroutine());
                }
            }
        };

        // Устанавливаем задачи строго в указанном порядке через SetTasks
        taskManager.SetTasks(ordered, true);
    }

    /// <summary>Обновляет текст задачи с текущим счётчиком дров</summary>
    private void UpdateWoodTaskText(int currentWoodCount)
    {
        int remaining = Mathf.Max(0, 3 - currentWoodCount);
        string taskText = remaining > 0 
            ? $"Дома холодно. Надо взять с собой ещё {remaining} полен"
            : "Дома холодно. Готово!";

        if (taskManager != null && taskManager.tasks != null && taskManager.tasks.Count > 4)
        {
            taskManager.tasks[4].taskName = taskText;
            taskManager.RefreshUI();
        }    }
}