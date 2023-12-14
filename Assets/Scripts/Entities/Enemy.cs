using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Entity
{
    private Transform player;
    private Animator anim;

    [Header("Vision")]
    [SerializeField] float visionRange = 10f;

    private bool foundPlayer = false;

    [Header("Attacking")]
    [SerializeField] Attack attack;
    [SerializeField] float attackRange = 5f;

    private bool canAttack = true;

    [Header("Drop")]
    [SerializeField] GameObject objectToDrop;

    protected override void Start()
    {
        base.Start();

        player = FindObjectOfType<Player>().transform;
        anim = GetComponentInChildren<Animator>();

        StartCoroutine(CheckIfPlayerIsWithinRange());
    }

    /// <summary>
    /// Continuosly checks if the player is within the enemys range of vision.
    /// </summary>
    /// <returns></returns>
    IEnumerator CheckIfPlayerIsWithinRange()
    {
        while (true)
        {
            if (GetDistanceToPlayer() <= visionRange)
            {
                foundPlayer = true;
                break;
            }

            yield return new WaitForSeconds(1f);
        }
    }

    /// <summary>
    /// Returns the distance from the enemy to the player.
    /// </summary>
    /// <returns></returns>
    private float GetDistanceToPlayer()
    {
        if (player == null) { Debug.LogError("Player not found"); return Mathf.Infinity; }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        return distanceToPlayer;
    }

    private void Update()
    {
        if (!foundPlayer) { return; }

        if (GetDistanceToPlayer() <= attackRange)
        {
            StartAttack();
        }
        else
        {
            ordinaryMovement = true; //Change later
        }
    }

    protected override void MoveEntity(Vector3 direction)
    {
        base.MoveEntity(direction);

        anim.SetBool("IsRunning", (controller.velocity.magnitude > 0));
    }

    private void FixedUpdate()
    {
        if (!foundPlayer) { return; }

        Vector3 movementDirection = (player.position - transform.position).normalized;
        movementDirection = new Vector3(movementDirection.x, 0, movementDirection.z);
        MoveEntity(movementDirection);
    }

    private void StartAttack()
    {
        ordinaryMovement = false;

        if (!canAttack) { return; }

        anim.SetTrigger("Attack");

        StartCoroutine(AttackCooldownRoutine(attack.cooldown));
    }

    private IEnumerator AttackCooldownRoutine(float cooldownTime)
    {
        canAttack = false;

        yield return new WaitForSeconds(cooldownTime);

        canAttack = true;
    }

    public void EndAttack()
    {
        ordinaryMovement = true;
    }

    public void DealDamage()
    {
        StartCoroutine(MoveWithAttackRoutine());

        AudioManager.PlaySound(attack.soundEffect, transform.position);

        Attack currentAttack = attack;

        for (int i = -currentAttack.angle; i <= currentAttack.angle; i += 5) //Test multiple directions rotating the vector both left and right until it finds ground.
        {
            RaycastHit[] hits = Physics.RaycastAll(transform.position, Quaternion.AngleAxis(i, Vector3.up) * transform.forward, currentAttack.range, LayerMask.GetMask("Player"));

            for (int j = 0; j < hits.Length; j++)
            {
                Player hitPlayer = null;
                hits[j].transform.TryGetComponent<Player>(out hitPlayer);

                if (hitPlayer != null)
                {
                    Vector3 knockback = (hitPlayer.transform.position - transform.position).normalized * currentAttack.knockbackForce;
                    hitPlayer.TakeDamage(currentAttack.damage, knockback, currentAttack.knockbackDuration);
                    return;
                }
            }
        }
    }

    private IEnumerator MoveWithAttackRoutine()
    {
        Attack currentAttack = attack;

        float timeMoved = 0;
        float timeToMove = currentAttack.timeToMove;
        float movementSpeed = attack.movementSpeed;

        while (timeMoved < timeToMove)
        {
            Vector3 targetDirection = CalculateGroundedPosition(transform.forward);

            controller.Move(targetDirection * movementSpeed * Time.deltaTime);

            yield return new WaitForEndOfFrame();

            timeMoved += Time.deltaTime;
        }
    }

    protected override void Die()
    {
        if (objectToDrop != null)
        {
            Instantiate(objectToDrop, transform.position, Quaternion.identity);
        }

        Destroy(gameObject); //Change this for later
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        Gizmos.color = Color.white;

        Gizmos.DrawWireSphere(transform.position, visionRange);

        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    [System.Serializable]
    public struct Attack
    {
        public int damage;
        public float knockbackForce;
        public float knockbackDuration;
        public int angle;
        public float range;
        public float cooldown;
        public float movementSpeed;
        public float timeToMove;
        public AudioClip soundEffect;
    }
}
