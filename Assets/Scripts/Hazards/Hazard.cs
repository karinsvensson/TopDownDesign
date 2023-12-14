using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hazard : MonoBehaviour
{
    protected Animator anim;

    [Header("Damage")]
    [SerializeField] int damage = 50;
    [SerializeField] Vector3 damageAreaSize;
    [SerializeField] Vector3 damageAreaOffset;
    [SerializeField] bool sphericalArea = false;

    [Header("Knockback")]
    [SerializeField] float knockbackForce;
    [SerializeField] float knockbackDuration = 0.2f;

    private Coroutine damageRoutine;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public virtual void DealDamageOnce()
    {
        DealDamage();
    }

    public void StartDealingDamage()
    {
        damageRoutine = StartCoroutine(DealDamageRoutine());
    }

    protected void StopDealingDamage()
    {
        if (damageRoutine == null) { return; }

        StopCoroutine(damageRoutine);
    }

    private IEnumerator DealDamageRoutine()
    {
        while (true)
        {
            DealDamage();

            yield return new WaitForEndOfFrame();
        }
    }

    private void DealDamage()
    {
        Collider[] hits;

        if (!sphericalArea)
        {
            hits = Physics.OverlapBox(transform.position + damageAreaOffset, damageAreaSize / 2, Quaternion.identity);
        }
        else
        {
            hits = Physics.OverlapSphere(transform.position + damageAreaOffset, damageAreaSize.x);
        }

        List<Entity> damagedEntities = new List<Entity>(); //Save all hits here to prevent duplicates

        for (int i = 0; i < hits.Length; i++) //Damage all the hit entities within the damagearea
        {
            Entity hitEntity;

            hits[i].transform.TryGetComponent<Entity>(out hitEntity);

            if (hitEntity != null && !damagedEntities.Contains(hitEntity))
            {
                Vector3 knockback = new Vector3(hitEntity.transform.position.x - transform.position.x, 0, hitEntity.transform.position.z - transform.position.z).normalized * knockbackForce;

                hitEntity.TakeDamage(damage, knockback, knockbackDuration);

                damagedEntities.Add(hitEntity);
            }
        }
    }

    private void DestroyHazard()
    {
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        if (!sphericalArea)
        {
            Gizmos.DrawWireCube(transform.position + damageAreaOffset, damageAreaSize);
        }
        else
        {
            Gizmos.DrawWireSphere(transform.position + damageAreaOffset, damageAreaSize.x);
        }
    }
}
