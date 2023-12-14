using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosiveBarrel : Hazard
{
    [Header("Explosive Barrel")]
    [SerializeField] float chargeTime = 1f;
    [SerializeField] GameObject explosionEffect;
    [Space]
    [SerializeField] AudioClip explosionSound;
    [SerializeField, Range(0, 1)] float explosionSoundVolume;

    private void Start()
    {
        anim.speed = 1f / chargeTime;
    }

    private void StartCharging()
    {
        anim.SetTrigger("Charge");
    }

    private void OnCollisionEnter(Collision collision)
    {
        StartCharging();
    }

    public override void DealDamageOnce()
    {
        base.DealDamageOnce();

        AudioManager.PlaySound(explosionSound, transform.position, explosionSoundVolume);

        if (explosionEffect == null) { return; }

        Instantiate(explosionEffect, transform.position, transform.rotation);
    }
}
