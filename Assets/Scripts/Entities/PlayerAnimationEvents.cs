using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationEvents : MonoBehaviour
{
    [SerializeField] ParticleSystem footstepEffectL;
    [SerializeField] ParticleSystem footstepEffectR;
    [SerializeField] AudioClip footstepSound;
    [SerializeField, Range(0, 1)] float footstepSoundVolume = 1f;

    private void OnLeftFootDown()
    {
        AudioManager.PlaySound(footstepSound, transform.position, footstepSoundVolume);

        if (footstepEffectL == null) { return; }

        footstepEffectL.Play();
    }

    private void OnRightFootDown()
    {
        AudioManager.PlaySound(footstepSound, transform.position, footstepSoundVolume);

        if (footstepEffectR == null) { return; }

        footstepEffectR.Play();
    }
}
