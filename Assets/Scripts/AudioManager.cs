using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AudioManager
{
    public static void PlaySound(AudioClip clip, Vector3 position, float volume = 1f)
    {
        if (clip == null) { return; }

        AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position, volume);
    }
}
