using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikePlatform : Hazard
{
    [Header("Spike Platform")]

    [SerializeField] float activeDuration = 1f;
    [SerializeField] float deactiveDuration = 1f;
    [SerializeField, Range(0, 1)] float activationOffset;
    [Space]
    [SerializeField] AudioClip activationSound;
    [SerializeField, Range(0, 1)] float activationSoundVolume;
    [SerializeField] AudioClip deactivationSound;
    [SerializeField, Range(0, 1)] float deactivationSoundVolume;

    private void Start()
    {
        StartCoroutine(SpikePlatformRoutine());
    }

    private IEnumerator SpikePlatformRoutine()
    {
        yield return new WaitForSeconds(activeDuration * activationOffset);

        while (true)
        {
            bool nextState = !anim.GetBool("Active");

            anim.SetBool("Active", nextState);

            if (nextState)
            {
                StartDealingDamage();
                AudioManager.PlaySound(activationSound, transform.position, activationSoundVolume);
                yield return new WaitForSeconds(activeDuration);
            }
            else
            {
                StopDealingDamage();
                AudioManager.PlaySound(deactivationSound, transform.position, deactivationSoundVolume);
                yield return new WaitForSeconds(deactiveDuration);
            }

            yield return new WaitForEndOfFrame(); //Safety-net
        }
    }
}
