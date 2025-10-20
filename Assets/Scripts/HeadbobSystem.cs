using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadbobSystem : MonoBehaviour
{
    [Range(0.001f, 0.01f)]
    public float Amount = 0.002f;

    [Range(1f, 30f)]
    public float Frequency = 10.0f;

    [Range(10f, 100f)]
    public float Smooth = 10.0f;

    [Header("Run Multipliers")]
    public float runAmplitudeMultiplier = 2f; // при беге амплитуда в 2 раза больше
    public float runFrequencyMultiplier = 1.5f; // при беге частота в 1.5 раза больше

    public CharacterController playerController;

    private Vector3 startPos;
    private Vector3 tempVec; // cached temp vector for calculations
    private float inputMagnitude; // cached per-frame value
    private float currentAmount; // cached per-frame value
    private float currentFrequency; // cached per-frame value

    void Start()
    {
        startPos = transform.localPosition;

        if (playerController == null)
        {
            playerController = GetComponentInParent<CharacterController>();
        }
    }

    void Update()
    {
        CheckForHeadbobTrigger();
        StopHeadbob();
    }

    private void CheckForHeadbobTrigger()
    {
        inputMagnitude = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).magnitude;

        if (inputMagnitude > 0.01f)
        {
            StartHeadBob();
        }
    }

    private void StartHeadBob()
    {
        currentAmount = Amount;
        currentFrequency = Frequency;

        // Если бежим (Shift) — увеличиваем амплитуду и частоту
        if (UnityEngine.InputSystem.Keyboard.current.leftShiftKey.isPressed)
        {
            currentAmount *= runAmplitudeMultiplier;
            currentFrequency *= runFrequencyMultiplier;
        }

        tempVec = Vector3.zero;
        tempVec.y += Mathf.Lerp(tempVec.y, Mathf.Sin(Time.time * currentFrequency) * currentAmount * 1.4f, Smooth * Time.deltaTime);
        tempVec.x += Mathf.Lerp(tempVec.x, Mathf.Cos(Time.time * currentFrequency / 2f) * currentAmount * 1.6f, Smooth * Time.deltaTime);

        transform.localPosition += tempVec;
    }

    private void StopHeadbob()
    {
        if (transform.localPosition == startPos) return;

        transform.localPosition = Vector3.Lerp(transform.localPosition, startPos, Smooth * Time.deltaTime);
    }
}
