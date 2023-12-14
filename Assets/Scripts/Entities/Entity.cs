using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Entity : MonoBehaviour
{
    protected CharacterController controller;
    protected CapsuleCollider col;

    [Header("Health")]
    [SerializeField] protected int maxHealth = 100;
    [SerializeField] protected float damageCooldown = 0.2f;
    [SerializeField] AudioClip takeDamageSound;
    [SerializeField, Range(0,1)] float takeDamageSoundVolume = 1;

    protected int currentHealth;
    protected bool damageable;

    [Header("Movement")]
    [SerializeField] float movementSpeed = 10f;
    [SerializeField] float rotationSpeed = 10f;
    [SerializeField] float gravity = 9f;

    private Vector3 targetVelocity;
    private Quaternion targetRotation;

    protected bool ordinaryMovement = true;
    protected bool dead = false;

    [Header("Ground Check")]
    [SerializeField] float groundCheckDistanceZ = 1f;
    [SerializeField] int groundCheckResolution = 2;
    [SerializeField] float groundCheckLengthY = -1f;
    [SerializeField] int movementRedirectionAngle = 50;
    [SerializeField] protected LayerMask groundCheckLayers;
    [SerializeField] protected LayerMask obstacleLayers;

    protected virtual void Awake()
    {
        controller = GetComponent<CharacterController>();
        col = GetComponent<CapsuleCollider>();
    }

    /// <summary>
    /// Sets up the standard values on the first frame of the scene.
    /// </summary>
    protected virtual void Start()
    {
        currentHealth = maxHealth;

        damageable = true;
        ordinaryMovement = true;
        dead = false;

        targetRotation = Quaternion.LookRotation(transform.forward, Vector3.up);
    }

    /// <summary>
    /// Moves and rotates the entity according to the specified direction.
    /// </summary>
    /// <param name="direction"></param>
    protected virtual void MoveEntity(Vector3 direction)
    {
        if (!ordinaryMovement || dead) { return; }

        direction = CalculateGroundedPosition(direction);

        targetVelocity = direction * movementSpeed;

        if (direction != Vector3.zero) //If the entity has no velocity, don't change the target rotation
        {
            targetRotation = Quaternion.LookRotation(direction, Vector3.up);
        }

        targetVelocity += Vector3.down * gravity; //Apply gravity

        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        controller.Move(targetVelocity * Time.deltaTime);
    }

    /// <summary>
    /// Adjusts the position so that the entity stays on the ground.
    /// </summary>
    /// <param name="desiredMoveDirection"></param>
    /// <returns></returns>
    protected Vector3 CalculateGroundedPosition(Vector3 desiredMoveDirection)
    {
        desiredMoveDirection.Normalize();

        bool safeToMove = SafeToMoveInDirection(desiredMoveDirection);

        if (safeToMove)
        {
            return desiredMoveDirection;
        }

        #region Movement Redirection
        Vector3 newMoveDirection = Vector3.zero;

        for (int i = movementRedirectionAngle / 10; i < movementRedirectionAngle; i += movementRedirectionAngle / 10) //Test multiple directions rotating the vector both left and right until it finds ground.
        {
            Vector3 directionToTestRight = Quaternion.AngleAxis(i, Vector3.up) * desiredMoveDirection;

            if (SafeToMoveInDirection(directionToTestRight))
            {
                newMoveDirection = directionToTestRight;
                break;
            }

            Vector3 directionToTestLeft = Quaternion.AngleAxis(-i, Vector3.up) * desiredMoveDirection;

            if (SafeToMoveInDirection(directionToTestLeft))
            {
                newMoveDirection = directionToTestLeft;
                break;
            }
        }
        #endregion

        return newMoveDirection;
    }

    private bool SafeToMoveInDirection(Vector3 desiredMoveDirection)
    {
        for (float i = 1 / (float)groundCheckResolution; i <= 1; i += 1 / (float)groundCheckResolution) //Check for ground in the desired direction
        {
            if (!Physics.Raycast(transform.position + (desiredMoveDirection * (groundCheckDistanceZ * i)), Vector3.down, groundCheckLengthY, groundCheckLayers))
            {
                return false;
            }
        }

        if (Physics.Raycast(transform.position, desiredMoveDirection, groundCheckDistanceZ, obstacleLayers))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Takes damage and checks if the entity survives. Also knocks the entity according to the specified direction.
    /// </summary>
    /// <param name="damage"></param>
    /// <param name="knockbackForce"></param>
    /// <param name="knockbackDuration"></param>
    public virtual void TakeDamage(int damage, Vector3 knockbackForce, float knockbackDuration)
    {
        if (!damageable || dead) { return; }

        currentHealth -= damage;

        AudioManager.PlaySound(takeDamageSound, transform.position, takeDamageSoundVolume);

        StartCoroutine(Knockback(knockbackForce, knockbackDuration));

        if (currentHealth <= 0)
        {
            Die();
        }

        StartCoroutine(DamageCooldown());
    }

    protected virtual void Die()
    {
        dead = true;
    }

    private IEnumerator DamageCooldown()
    {
        damageable = false;

        yield return new WaitForSeconds(damageCooldown);

        damageable = true;
    }

    /// <summary>
    /// Knocks the entity back whilst still keeping it on the ground
    /// </summary>
    /// <param name="force"></param>
    /// <param name="duration"></param>
    /// <returns></returns>
    private IEnumerator Knockback(Vector3 force, float duration)
    {
        ordinaryMovement = false;

        float timeSinceKnockbacked = Mathf.Epsilon;

        Vector3 knockbackDirection = force.normalized;
        float knockbackForce = force.magnitude;

        while (timeSinceKnockbacked < duration)
        {
            knockbackDirection = CalculateGroundedPosition(knockbackDirection);

            controller.Move(knockbackDirection * knockbackForce * Time.deltaTime);

            yield return new WaitForEndOfFrame();

            timeSinceKnockbacked += Time.deltaTime;
            float newForceMagnitude = knockbackForce * (1 - (timeSinceKnockbacked / duration));
            knockbackForce = newForceMagnitude;
        }

        ordinaryMovement = true;
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        for (float i = 1 / (float)groundCheckResolution; i <= 1; i += 1 / (float)groundCheckResolution) //Check for ground in the desired direction
        {
            Vector3 groundCheckPosZ = transform.position + (transform.forward * (groundCheckDistanceZ * i));
            Vector3 groundCheckPosY = groundCheckPosZ + (Vector3.down * groundCheckLengthY);
            Gizmos.DrawLine(transform.position, groundCheckPosZ);
            Gizmos.DrawLine(groundCheckPosZ, groundCheckPosY);
        }

        Gizmos.DrawLine(transform.position, transform.position + Quaternion.AngleAxis(movementRedirectionAngle, Vector3.up) * transform.forward);
        Gizmos.DrawLine(transform.position, transform.position + Quaternion.AngleAxis(-movementRedirectionAngle, Vector3.up) * transform.forward);
    }
}
