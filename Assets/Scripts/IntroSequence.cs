using UnityEngine;
using System.Collections;

public class IntroSequence : MonoBehaviour
{
    public PlayerMovement player;
    public AudioSource knockSound;
    public float delayBeforeKnock = 1.5f;
    public float delayBeforeControl = 3f;

    void Start()
    {
        StartCoroutine(Intro());
    }

    IEnumerator Intro()
    {
        player.SetMovement(false);

        yield return new WaitForSeconds(delayBeforeKnock);

        knockSound.Play();

        yield return new WaitForSeconds(delayBeforeControl);

        player.SetMovement(true);
    }
}
