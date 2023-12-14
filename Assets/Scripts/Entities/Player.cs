using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Entity
{
    private Vector2 input;
    private Vector3 movementDirection;

    [Header("Dash")]
    [SerializeField] float dashSpeed = 20f;
    [SerializeField] float dashDuration = 0.25f;
    [SerializeField] float dashCooldown = 0.2f;
    [SerializeField] int dashGroundCheckResolution = 5;
    [SerializeField] AudioClip dashSound;
    [SerializeField, Range(0, 1)] float dashSoundVolume = 1f;

    private bool dashOnCooldown = false;

    [Header("Attacking")]
    [SerializeField] TrailRenderer swordTrail;
    [SerializeField] LayerMask enemyLayers;
    [Space]
    [SerializeField] Attack[] attacks;

    private int currentAttackIndex = 0;
    private bool canAttack = true;
    private bool attackBuffer = false;
    private float lastAttackTime;

    private Transform mainCamera;
    private PlayerUI ui;
    private Animator anim;

    protected override void Awake()
    {
        base.Awake();

        mainCamera = Camera.main.transform;
        ui = GetComponentInChildren<PlayerUI>();
        anim = GetComponentInChildren<Animator>();
    }

    protected override void Start()
    {
        base.Start();

        if (CheckpointManager.Instance != null)
        {
            transform.position = CheckpointManager.Instance.GetSpawnPoint();
        }

        ToggleSwordTrail(false);
    }

    private void Update()
    {
        if (dead) { return; }

        anim.transform.localPosition += new Vector3(-anim.transform.localPosition.x, 0, -anim.transform.localPosition.z);

        CheckForPlayerInput();
    }

    private void CheckForPlayerInput()
    {
        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (Input.GetKeyDown(KeyCode.LeftShift) && !dashOnCooldown)
        {
            StartCoroutine(DashRoutine());
        }

        if ((Input.GetMouseButtonDown(0) || attackBuffer) && ordinaryMovement)
        {
            if (canAttack)
            {
                StartAttack();
                attackBuffer = false;
            }
            else
            {
                attackBuffer = true;
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GameController.Instance.LoadScene(1);
        }
    }

    private void FixedUpdate()
    {
        movementDirection = Quaternion.AngleAxis(mainCamera.eulerAngles.y, Vector3.up) * new Vector3(input.x, 0, input.y).normalized; //MovementDirection adjusted according to camera
        MoveEntity(movementDirection);
    }

    protected override void MoveEntity(Vector3 direction)
    {
        base.MoveEntity(direction);

        anim.SetBool("IsRunning", (controller.velocity.magnitude > 0 && input != Vector2.zero));
    }

    #region Dash

    /// <summary>
    /// Dashes the player in according to the current rotation. Ordinary movement is not available for the duration of the routine.
    /// </summary>
    /// <returns></returns>
    private IEnumerator DashRoutine()
    {
        AudioManager.PlaySound(dashSound, transform.position, dashSoundVolume);

        ordinaryMovement = false;
        damageable = false; //Makes the player invulnerable for the duration of the dash, might remove or might keep.

        if (movementDirection == Vector3.zero)
        {
            movementDirection = transform.forward;
        }

        Vector3 dashVelocity = movementDirection.normalized * dashSpeed;
        float dashEndMultiplier = CalculateDashLength(dashVelocity * dashDuration * 1.2f);

        float timeSinceDash = 0;
        while (timeSinceDash < (dashDuration * dashEndMultiplier))
        {
            controller.Move(dashVelocity * Time.deltaTime);
            timeSinceDash += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        ordinaryMovement = true;
        damageable = true;
        dashOnCooldown = true;

        yield return new WaitForSeconds(dashCooldown);

        dashOnCooldown = false;
    }

    private float CalculateDashLength(Vector3 desiredEndPosition)
    {
        float dashLengthMultiplier = 0;

        for (float i = 1; i > 0f; i -= 1 / (float)dashGroundCheckResolution) //Check for ground in the desired direction
        {
            if (Physics.Raycast(transform.position + (desiredEndPosition * i), Vector3.down * 2, groundCheckLayers)) //Check for ground
            {
                if (!Physics.Raycast(transform.position, desiredEndPosition.normalized, desiredEndPosition.magnitude * i, obstacleLayers)) //Check for obstacles
                {
                    dashLengthMultiplier = i - (1 / ((float)dashGroundCheckResolution) * 2);
                    break;
                }
            }
        }

        return dashLengthMultiplier;
    }

    #endregion

    #region Dealing Damage

    private void StartAttack()
    {
        Vector3 direction = Quaternion.AngleAxis(mainCamera.eulerAngles.y, Vector3.up) * new Vector3(input.x, 0, input.y).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = targetRotation;
        }

        if ((Time.time - lastAttackTime) < attacks[currentAttackIndex].chainTime && (currentAttackIndex + 1) < attacks.Length)
        {
            currentAttackIndex++;
        }
        else
        {
            currentAttackIndex = 0;
        }

        lastAttackTime = Time.time;

        anim.SetTrigger("Attack" + currentAttackIndex);
        AudioManager.PlaySound(attacks[currentAttackIndex].soundEffect, transform.position);
        ToggleSwordTrail(true);

        StartCoroutine(DealDamageRoutine());

        StartCoroutine(AttackCooldownRoutine(attacks[currentAttackIndex].cooldown));
    }

    private IEnumerator DealDamageRoutine()
    {
        Attack currentAttack = attacks[currentAttackIndex];
        List<RaycastHit> hitEnemies = new List<RaycastHit>();

        StartCoroutine(MoveWithAttackRoutine());

        float timeSinceStartedAttack = 0;

        while (timeSinceStartedAttack <= attacks[currentAttackIndex].timeToMove)
        {
            for (int i = -currentAttack.angle; i <= currentAttack.angle; i += 5) //Test multiple directions rotating the vector both left and right until it finds ground.
            {
                RaycastHit[] hits = Physics.RaycastAll(transform.position, Quaternion.AngleAxis(i, Vector3.up) * transform.forward, currentAttack.range, enemyLayers);

                for (int j = 0; j < hits.Length; j++)
                {
                    if (hitEnemies.Contains(hits[j])) { continue; }

                    hitEnemies.Add(hits[j]);

                    Enemy hitEnemy = null;
                    hits[j].transform.TryGetComponent<Enemy>(out hitEnemy);

                    if (hitEnemy != null)
                    {
                        Vector3 knockback = (hitEnemy.transform.position - transform.position).normalized * currentAttack.knockbackForce;
                        hitEnemy.TakeDamage(currentAttack.damage, knockback, currentAttack.knockbackDuration);
                    }
                }
            }

            yield return new WaitForSeconds(0.05f);

            timeSinceStartedAttack += 0.05f;
        }
    }

    private void ToggleSwordTrail(bool state)
    {
        swordTrail.enabled = state;
    }

    private IEnumerator AttackCooldownRoutine(float cooldownTime)
    {
        canAttack = false;

        yield return new WaitForSeconds(cooldownTime);

        canAttack = true;
    }

    private IEnumerator MoveWithAttackRoutine()
    {
        Attack currentAttack = attacks[currentAttackIndex];

        float timeMoved = 0;
        float timeToMove = currentAttack.timeToMove;
        float movementSpeed = attacks[currentAttackIndex].movementSpeed;

        ordinaryMovement = false;

        while (timeMoved < timeToMove)
        {
            Vector3 targetDirection = CalculateGroundedPosition(transform.forward);

            controller.Move(targetDirection * movementSpeed * Time.deltaTime);

            yield return new WaitForEndOfFrame();

            timeMoved += Time.deltaTime;
        }

        ordinaryMovement = true;

        ToggleSwordTrail(false);
    }

    #endregion

    #region Taking Damage

    public override void TakeDamage(int damage, Vector3 knockbackForce, float knockbackDuration)
    {
        base.TakeDamage(damage, knockbackForce, knockbackDuration);

        ui.UpdateHealthbar((float)currentHealth / (float)maxHealth);
    }

    protected override void Die()
    {
        base.Die();

        GameController.Instance.LoadScene(2);
    }
    #endregion

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("Goal"))
        {
            GameController.Instance.LoadScene(3);
        }
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + (transform.forward * dashSpeed * dashDuration));

        Vector3 desiredEndPosition = (transform.forward * dashSpeed * dashDuration);
        for (float i = 1; i > 0.1f; i -= 1 / (float)dashGroundCheckResolution)
        {
            Vector3 lineStartZ = transform.position + (desiredEndPosition * i);
            Gizmos.DrawLine(lineStartZ, lineStartZ + Vector3.down * 2);
        }
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
        public float chainTime;
        public float movementSpeed;
        public float timeToMove;
        public AudioClip soundEffect;
    }
}
