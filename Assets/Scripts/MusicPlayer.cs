using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class MusicPlayer : SingletonPersistent<MusicPlayer>
{
    private AudioSource audioSource;

    [SerializeField] AudioClip music;

    protected override void Awake()
    {
        base.Awake();

        if (music == null) { return; }

        audioSource = GetComponent<AudioSource>();
        audioSource.clip = music;
        audioSource.Play();
    }

    private void Update()
    {
        transform.position = Camera.main.transform.position;
    }
}
