using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class StepSound : MonoBehaviour
{
    [System.Serializable]
    public class SurfaceSound
    {
        public string tag;
        public AudioClip[] stepClips;
        public AudioClip jumpClip;
        public AudioClip landClip;
        [Range(0f, 1f)] public float stepVolume = 0.5f;
    }

    [Header("Settings")]
    public SurfaceSound[] surfaces;
    public float walkStepInterval = 0.5f;   // интервал шагов при ходьбе
    public float runStepInterval = 0.3f;    // интервал шагов при беге
    public float runPitch = 1.2f;           // pitch при беге
    public float walkPitch = 1f;            // pitch при ходьбе

    private CharacterController controller;
    private AudioSource audioSource;

    private float stepTimer;
    private bool wasGrounded;
    private bool jumpedByKey;

    [Header("Jump Settings")]
    public KeyCode jumpKey = KeyCode.Space;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
            audioSource.playOnAwake = false;
    }

    private void Update()
    {
        // Не обрабатываем звуки шагов во время паузы
        if (PauseManager.Instance != null && PauseManager.Instance.IsPaused)
            return;
            
        bool isGrounded = controller.isGrounded;

        // --- прыжок (только если пробел нажат) ---
        if (Input.GetKeyDown(jumpKey) && isGrounded)
        {
            PlayJumpSound();
            jumpedByKey = true;
        }

        // --- приземление (только после "искусственного" прыжка) ---
        if (!wasGrounded && isGrounded && jumpedByKey)
        {
            PlayLandSound();
            jumpedByKey = false;
        }

        // --- шаги ---
        if (isGrounded && controller.velocity.magnitude > 0.2f)
        {
            stepTimer += Time.deltaTime;

            bool isRunning = Input.GetKey(KeyCode.LeftShift) && controller.velocity.magnitude > 4f;
            float currentInterval = isRunning ? runStepInterval : walkStepInterval;

            if (stepTimer >= currentInterval)
            {
                PlayStepSound(isRunning);
                stepTimer = 0f;
            }
        }

        wasGrounded = isGrounded;
    }

    private void PlayStepSound(bool isRunning)
    {
        SurfaceSound surface = GetSurfaceSound();
        if (surface == null || surface.stepClips.Length == 0) return;

        AudioClip clip = surface.stepClips[Random.Range(0, surface.stepClips.Length)];
        audioSource.clip = clip;
        audioSource.volume = surface.stepVolume;
        audioSource.pitch = isRunning ? runPitch : walkPitch;
        audioSource.Play();
    }

    private void PlayJumpSound()
    {
        SurfaceSound surface = GetSurfaceSound();
        if (surface != null && surface.jumpClip != null)
        {
            audioSource.PlayOneShot(surface.jumpClip, surface.stepVolume);
        }
    }

    private void PlayLandSound()
    {
        SurfaceSound surface = GetSurfaceSound();
        if (surface != null && surface.landClip != null)
        {
            audioSource.PlayOneShot(surface.landClip, surface.stepVolume);
        }
    }

    private SurfaceSound GetSurfaceSound()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 2f))
        {
            foreach (var s in surfaces)
            {
                if (hit.collider.CompareTag(s.tag))
                    return s;
            }
        }
        return null;
    }
}
