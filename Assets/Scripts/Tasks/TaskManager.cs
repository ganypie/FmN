using UnityEngine;
using System;
using System.Collections.Generic;
using TMPro;

public class TaskManager : MonoBehaviour
{
    public List<Task> tasks = new List<Task>();
    private int currentTaskIndex = 0;

    public UnityEngine.UI.Text taskUIText; // Ссылка на UI Text
    public TMP_Text taskTMPText; // Ссылка на TextMeshPro (опционально)

    void Start()
    {
        // Ensure we start from the first incomplete task
        currentTaskIndex = GetNextIncompleteIndex(0);
        ShowCurrentTask();
    }

    /// <summary>
    /// Установить список задач для менеджера. Этот метод гарантирует, что
    /// порядок задач будет соблюдён и индекс текущей задачи будет корректно установлен.
    /// </summary>
    public void SetTasks(List<Task> newTasks, bool resetIndex = true)
    {
        if (newTasks == null) throw new System.ArgumentNullException(nameof(newTasks));
        tasks = newTasks;
        if (resetIndex)
        {
            currentTaskIndex = GetNextIncompleteIndex(0);
            ShowCurrentTask();
        }
    }

    void Update()
    {
        if (tasks == null || tasks.Count == 0) return;

        if (currentTaskIndex >= tasks.Count) return;

        Task currentTask = tasks[currentTaskIndex];
        if (currentTask == null)
        {
            // skip null entries
            currentTaskIndex = GetNextIncompleteIndex(currentTaskIndex + 1);
            ShowCurrentTask();
            return;
        }

        if (currentTask.completionCondition != null)
        {
            bool completed = false;
            try
            {
                completed = currentTask.completionCondition.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"[TaskManager] completionCondition threw for task '{currentTask.taskName}': {e}");
            }

            if (completed)
            {
                CompleteCurrentTask();
            }
        }
    }

    private int GetNextIncompleteIndex(int startIndex)
    {
        if (tasks == null) return 0;
        for (int i = startIndex; i < tasks.Count; i++)
        {
            if (tasks[i] != null && !tasks[i].isCompleted)
                return i;
        }
        return tasks.Count; // no more tasks
    }

    void ShowCurrentTask()
    {
        string text = "";
        Task current = null;
        if (currentTaskIndex < tasks.Count && tasks[currentTaskIndex] != null)
        {
            current = tasks[currentTaskIndex];
            text = current.taskName;
        }

        // Устанавливаем в оба поля, если они назначены
        if (taskUIText != null) taskUIText.text = text;
        if (taskTMPText != null) taskTMPText.text = text;

        Debug.Log($"[TaskManager] Current task: {text}");

        // Вызываем onTaskCreate при активации задачи
        if (current != null && current.onTaskCreate != null)
        {
            try
            {
                current.onTaskCreate.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"[TaskManager] onTaskCreate threw for task '{current.taskName}': {e}");
            }
        }

        // Уведомляем подписчиков об изменении задачи
        OnTaskChanged?.Invoke(current);
    }

    void CompleteCurrentTask()
    {
        Task currentTask = tasks[currentTaskIndex];
        if (currentTask == null) return;

        currentTask.isCompleted = true;

        try
        {
            currentTask.onComplete?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogError($"[TaskManager] onComplete threw for task '{currentTask.taskName}': {e}");
        }

        currentTaskIndex = GetNextIncompleteIndex(currentTaskIndex + 1);
        ShowCurrentTask();
    }

    /// <summary>
    /// Принудительно завершить текущую задачу (удобно для тестирования).
    /// </summary>
    public void ForceCompleteCurrentTask()
    {
        if (currentTaskIndex < tasks.Count && tasks[currentTaskIndex] != null)
        {
            Debug.Log($"[TaskManager] Force completing task: {tasks[currentTaskIndex].taskName}");
            CompleteCurrentTask();
        }
    }

    /// <summary>
    /// Возвращает имя текущей задачи (или пустую строку если задач нет)
    /// </summary>
    public string GetCurrentTaskName()
    {
        if (currentTaskIndex < tasks.Count && tasks[currentTaskIndex] != null)
            return tasks[currentTaskIndex].taskName;
        return string.Empty;
    }

    /// <summary>
    /// Принудительно обновить UI (используется при динамическом изменении текста задачи)
    /// </summary>
    public void RefreshUI()
    {
        string text = "";
        if (currentTaskIndex < tasks.Count && tasks[currentTaskIndex] != null)
            text = tasks[currentTaskIndex].taskName;

        if (taskUIText != null) taskUIText.text = text;
        if (taskTMPText != null) taskTMPText.text = text;
    }

    /// <summary>
    /// Событие оповещает подписчиков, когда текущая задача изменяется.
    /// </summary>
    public event Action<Task> OnTaskChanged;
}

// Простая модель задачи, используемая TaskManager и DemoTaskSetup
[Serializable]
public class Task
{
    public string taskName;
    public Func<bool> completionCondition;
    public Action onComplete;
    public Action onTaskCreate; // Вызывается при активации задачи
    public bool isCompleted = false;
}
