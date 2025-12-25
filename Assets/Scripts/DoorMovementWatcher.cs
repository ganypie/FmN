using UnityEngine;

public class DoorMovementWatcher : MonoBehaviour
{
    public StartSequenceController startSequence;
    public float movementThreshold = 0.01f; // чувствительность

    private Vector3 initialPos;
    private Quaternion initialRot;
    private bool triggered = false;

    void Start()
    {
        initialPos = transform.position;
        initialRot = transform.rotation;
    }

    void Update()
    {
        if (triggered) return;

        // Проверяем движение двери
        float posDelta = Vector3.Distance(transform.position, initialPos);
        float rotDelta = Quaternion.Angle(transform.rotation, initialRot);

        if (posDelta > movementThreshold || rotDelta > movementThreshold)
        {
            triggered = true;
            if (startSequence != null)
                startSequence.OnDoorOpened();
        }
    }
}
