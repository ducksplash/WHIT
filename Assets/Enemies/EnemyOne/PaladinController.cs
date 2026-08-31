using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using UnityEngine.UI;

public class PaladinController : MonoBehaviour
{
    [Header("Movement")]
    public float patrolDistance = 5f;
    public float walkSpeed = 2f;
    public float runSpeed = 10f;
    public float turnSpeed = 720f;

    [Header("Behavior")]
    public float idleDelay = 2f;
    public float attackRange = 2f;
    public float chaseRange = 10f;
    public float attackCooldown = 1f;
    public float attackLeashDistance = 3.5f;   // 🆕 extra buffer to resume chase after attack

    [Header("Stuck Recovery")]
    public float stuckCheckInterval = 0.5f;
    public float stuckDistanceThreshold = 0.2f;
    public float stuckRepositionTime = 1.5f;
    public LayerMask obstacleMask;

    [Header("Health")]
    public int maxHealth = 40;
    public int health = 40;
    public Slider healthBar;
    public Slider maxHealthBar;
    public Image healthBarFill;

    [Header("Combat")]
    public Vector2 attackKnockbackForce = new Vector2(5f, 1f);
    public Vector2 meleeKnockbackForce = new Vector2(5f, 1f);
    public float knockbackDuration = 0.3f;
    public Collider attackTip;

    [Header("FX")]
    public GameObject particlePoof;
    public float particleDuration = 3f;

    [Header("Ragdoll Physics")]
    public float ragdollFriction = 0.2f;
    public float jointDamping = 0.5f;

    [Header("References")]
    public Animator animator;
    public GameObject sword;
    public Rigidbody swordRB;
    [HideInInspector] public PortalController portal;

    private Rigidbody rb;
    private NavMeshAgent agent;
    private Vector3 startPosition;
    private bool isAttacking = false;
    private bool isIdling = false;
    private bool wasAttacking = false;
    private float idleTimer = 0f;
    private float stuckTimer = 0f;
    private Vector3 lastPosition;
    private float stuckCheckTimer;
    private bool isInitialized = false;
    private float attackCooldownTimer = 0f;
    private bool isBeingKnockedBack = false;
    private bool isDead = false;
    private float ragdollDampTime = 0f;

    private enum State { Roaming, Chasing, Attacking }
    private State currentState;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        PhysicsMaterial highFrictionMaterial = new PhysicsMaterial("HighFriction")
        {
            dynamicFriction = 1f,
            staticFriction = 1f,
            bounciness = 0f,
            frictionCombine = PhysicsMaterialCombine.Maximum
        };

        agent = GetComponent<NavMeshAgent>();
        agent.enabled = false;
        agent.updatePosition = true;
        agent.updateRotation = true;
        agent.speed = walkSpeed;
        agent.angularSpeed = turnSpeed;
        agent.acceleration = 40f;
        agent.stoppingDistance = attackRange - 0.2f;
        agent.autoBraking = true;
        agent.autoTraverseOffMeshLink = false;
        agent.baseOffset = 0.1f;
        agent.radius = 0.25f;
        agent.height = 1.5f;

        CapsuleCollider capsule = GetComponent<CapsuleCollider>();
        if (capsule != null)
        {
            capsule.radius = 0.25f;
            capsule.height = 1.5f;
            capsule.center = new Vector3(0f, 0.75f, 0f);
            capsule.material = highFrictionMaterial;
        }

        if (attackTip != null) attackTip.enabled = false;

        startPosition = transform.position;
        animator.applyRootMotion = false;
        animator.SetBool("IsIdle", true);
        animator.SetBool("CanAttack", true);
        animator.SetBool("WasHit", false);
        idleTimer = idleDelay;

        if (healthBar != null && healthBarFill == null)
        {
            healthBarFill = healthBar.fillRect?.GetComponent<Image>();
        }

        UpdateHealthBars();
        StartCoroutine(InitializeAfterDungeon());
    }

    private void UpdateHealthBars()
    {
        if (healthBar != null)
        {
            float healthPercentage = (float)health / maxHealth;
            healthBar.value = healthPercentage;
            if (healthBarFill != null)
            {
                Color targetColor = healthPercentage > 0.5f ? Color.green : healthPercentage > 0.25f ? new Color(1f, 0.5f, 0f) : Color.red;
                targetColor.a = 1f;
                healthBarFill.color = targetColor;
            }
        }
        if (maxHealthBar != null)
        {
            maxHealthBar.value = 1f;
        }
    }

    void Update()
    {
        if (isDead || health <= 0) return;
        if (!isInitialized || !agent.isActiveAndEnabled || !agent.isOnNavMesh) return;

        if (attackCooldownTimer > 0f) attackCooldownTimer -= Time.deltaTime;

        stuckCheckTimer -= Time.deltaTime;
        if (stuckCheckTimer <= 0f && currentState != State.Attacking && !isBeingKnockedBack)
        {
            if (Vector3.Distance(transform.position, lastPosition) < stuckDistanceThreshold && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                stuckTimer += stuckCheckInterval;
                if (stuckTimer >= stuckRepositionTime)
                {
                    RepositionIfStuck();
                }
            }
            else
            {
                stuckTimer = 0f;
            }
            lastPosition = transform.position;
            stuckCheckTimer = stuckCheckInterval;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, Player.Instance.transform.position);
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.IsName("Attack") && stateInfo.normalizedTime < 1f)
        {
            if (!wasAttacking)
            {
                animator.SetBool("CanAttack", false);
                isAttacking = true;
                StartCoroutine(EnableAttack());
                wasAttacking = true;
            }
            agent.isStopped = true;
            currentState = State.Attacking;
            return;
        }

        if (wasAttacking && !animator.IsInTransition(0) && stateInfo.normalizedTime >= 1f)
        {
            animator.SetBool("CanAttack", true);
            isAttacking = false;
            animator.SetBool("IsAttacking", false);
            wasAttacking = false;
            agent.isStopped = false;
            attackCooldownTimer = attackCooldown;
        }

        if (isAttacking && distanceToPlayer > attackRange + 0.5f)
        {
            isAttacking = false;
            animator.SetBool("IsAttacking", false);
            animator.SetBool("CanAttack", true);
            wasAttacking = false;
            agent.isStopped = false;
            attackCooldownTimer = attackCooldown;
            currentState = distanceToPlayer <= chaseRange && CanSeePlayer() ? State.Chasing : State.Roaming;
        }

        if (distanceToPlayer <= attackRange && CanSeePlayer() && animator.GetBool("CanAttack") && !stateInfo.IsName("Attack") && !animator.IsInTransition(0) && attackCooldownTimer <= 0f)
        {
            currentState = State.Attacking;
        }
        else if (distanceToPlayer <= chaseRange && CanSeePlayer())
        {
            currentState = State.Chasing;
        }
        else
        {
            currentState = State.Roaming;
        }

        switch (currentState)
        {
            case State.Roaming: HandleRoaming(); break;
            case State.Chasing: HandleChasing(); break;
            case State.Attacking: HandleAttacking(); break;
        }

        float smoothedSpeed = Mathf.Lerp(animator.GetFloat("Speed"), agent.velocity.magnitude / runSpeed, Time.deltaTime * 20f);
        animator.SetFloat("Speed", smoothedSpeed);
        animator.SetBool("IsIdle", isIdling && !isAttacking && !isBeingKnockedBack);
        animator.SetBool("IsWalking", currentState == State.Roaming && !isIdling && !isAttacking && !isBeingKnockedBack);
        animator.SetBool("IsRunning", currentState == State.Chasing && !isAttacking && !isBeingKnockedBack);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (health <= 0 || isDead) return;

        // damage
        
    }

    private IEnumerator ApplyKnockback()
    {
        if (isDead) yield break;

        if (isBeingKnockedBack) yield break;

        isBeingKnockedBack = true;
        if (agent.isActiveAndEnabled) agent.enabled = false;
        rb.isKinematic = false;

        Vector3 direction = (transform.position - Player.Instance.transform.position).normalized;
        Vector3 knockback = direction * meleeKnockbackForce.x + Vector3.up * meleeKnockbackForce.y;
        rb.AddForce(knockback, ForceMode.Impulse);

        yield return new WaitForSeconds(knockbackDuration);

        animator.SetBool("WasHit", false);

        rb.isKinematic = true;
        if (!isDead && agent != null)
        {
            agent.enabled = true;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(new Vector3(transform.position.x, Mathf.Max(transform.position.y, 0.1f), transform.position.z), out hit, 1f, NavMesh.AllAreas))
            {
                agent.Warp(hit.position);
            }
        }
        isBeingKnockedBack = false;

        if (!isDead && currentState == State.Chasing && agent.isActiveAndEnabled) agent.SetDestination(Player.Instance.transform.position);
        else if (!isDead && currentState == State.Roaming && agent.isActiveAndEnabled) agent.SetDestination(GetValidPatrolTarget());
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;

        Debug.Log("Paladin died");

        StopAllCoroutines();

        foreach (MonoBehaviour script in GetComponents<MonoBehaviour>())
        {
            if (script != this)
            {
                script.enabled = false;
            }
        }

        if (agent != null && agent.isActiveAndEnabled)
        {
            Destroy(agent);
        }

        if (animator != null)
        {
            animator.enabled = false;
            animator.applyRootMotion = false;
        }

        rb.isKinematic = true;

        if (healthBar != null && healthBar.gameObject.transform.parent != null)
        {
            healthBar.gameObject.transform.parent.gameObject.SetActive(false);
        }

        if (sword != null && swordRB != null)
        {
            sword.transform.SetParent(null);
            swordRB.useGravity = true;
            swordRB.isKinematic = false;
            swordRB.linearVelocity = Vector3.zero;
            swordRB.angularVelocity = Vector3.zero;
            if (attackTip != null) attackTip.enabled = false;
        }

        PhysicsMaterial ragdollMaterial = new PhysicsMaterial("RagdollLowFriction")
        {
            dynamicFriction = ragdollFriction,
            staticFriction = ragdollFriction,
            bounciness = 0f,
            frictionCombine = PhysicsMaterialCombine.Minimum
        };
        
        CapsuleCollider capsule = GetComponent<CapsuleCollider>();
        if (capsule != null)
        {
            Collider playerCollider = Player.Instance?.GetComponent<Collider>();
            if (playerCollider != null)
            {
                Physics.IgnoreCollision(capsule, playerCollider, true);
            }
            capsule.enabled = false;
        }

        if (particlePoof != null)
        {
            GameObject poof = Instantiate(particlePoof, transform.position, Quaternion.identity);
            ParticleSystem ps = poof.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                var main = ps.main;
                main.duration = particleDuration;
                main.startLifetime = particleDuration;
                ps.Play();
            }
        }
        
        if (portal != null) portal.RemoveEnemy(gameObject);
        Destroy(gameObject);
    }

    private void HandleRoaming()
    {
        if (isDead || !agent.isActiveAndEnabled || !agent.isOnNavMesh) return;

        agent.speed = walkSpeed;
        agent.isStopped = false;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.2f)
        {
            isIdling = true;
            idleTimer -= Time.deltaTime;
            if (idleTimer <= 0f)
            {
                isIdling = false;
                idleTimer = idleDelay;
                Vector3 target = GetValidPatrolTarget();
                if (NavMesh.CalculatePath(transform.position, target, NavMesh.AllAreas, agent.path))
                    agent.SetDestination(target);
            }
            else
            {
                agent.isStopped = true;
            }
        }
    }

    private void HandleChasing()
    {
        if (isDead || !agent.isActiveAndEnabled || !agent.isOnNavMesh) return;

        float distanceToPlayer = Vector3.Distance(transform.position, Player.Instance.transform.position);

        // If we’ve reached attack distance, stop chasing and switch state
        if (distanceToPlayer <= attackRange)
        {
            agent.isStopped = true;
            currentState = State.Attacking;
            return;
        }

        agent.speed = runSpeed;
        agent.isStopped = false;

        Vector3 targetPos = Player.Instance.transform.position;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPos, out hit, 15f, NavMesh.AllAreas) && IsPositionClear(hit.position))
        {
            if (NavMesh.CalculatePath(transform.position, hit.position, NavMesh.AllAreas, agent.path))
            {
                agent.SetDestination(hit.position);
            }
        }
        else
        {
            Vector3 patrolTarget = GetValidPatrolTarget();
            if (NavMesh.CalculatePath(transform.position, patrolTarget, NavMesh.AllAreas, agent.path))
            {
                agent.SetDestination(patrolTarget);
            }
        }
        isIdling = false;
    }

    private void HandleAttacking()
    {
        if (isDead || !agent.isActiveAndEnabled || !agent.isOnNavMesh) return;

        float distanceToPlayer = Vector3.Distance(transform.position, Player.Instance.transform.position);

        // 🧭 If the player moves out of leash range, resume chasing
        if (distanceToPlayer > attackLeashDistance)
        {
            agent.isStopped = false;
            currentState = State.Chasing;
            return;
        }

        // 🛑 Stop the agent so it doesn't push the player
        agent.isStopped = true;

        // 👀 Rotate toward the player but do not move forward
        Vector3 dir = (Player.Instance.transform.position - transform.position).normalized;
        dir.y = 0;
        if (dir.magnitude > 0.01f)
        {
            Quaternion lookRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRot, turnSpeed * Time.deltaTime);
        }

        // Only attack when cooldown is ready
        if (!isAttacking && animator.GetBool("CanAttack") && !animator.GetCurrentAnimatorStateInfo(0).IsName("Attack") && attackCooldownTimer <= 0f)
        {
            isAttacking = true;
            animator.SetBool("IsAttacking", true);
        }
    }

    private IEnumerator EnableAttack()
    {
        if (isDead) yield break;

        if (attackTip != null) attackTip.enabled = true;

        float attackDuration = 1f;
        foreach (var clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name.ToLower().Contains("attack"))
            {
                attackDuration = clip.length;
                break;
            }
        }

        yield return new WaitForSeconds(attackDuration / 2.5f);

        if (!isDead && Player.Instance != null && Vector3.Distance(transform.position, Player.Instance.transform.position) <= attackRange + 0.5f)
        {
            Vector3 knockback = transform.forward * attackKnockbackForce.x + Vector3.up * attackKnockbackForce.y;
            // if (Player.Instance.playerCollisionScript != null)
            // {
            //     Player.Instance.playerCollisionScript.ApplyKnockback(knockback);
            // }
        }

        if (attackTip != null) attackTip.enabled = false;
        yield return new WaitForSeconds(attackDuration - (attackDuration / 2.5f));

        isAttacking = false;
        animator.SetBool("IsAttacking", false);
        animator.SetBool("CanAttack", true);
        wasAttacking = false;
        if (agent.isActiveAndEnabled) agent.isStopped = false;
        attackCooldownTimer = attackCooldown;
    }

    private void SnapToValidSpawn()
    {
        if (isDead) return;

        NavMeshHit hit;
        Vector3 pos = transform.position;
        pos.y = 0.1f + (Mathf.FloorToInt(pos.y / DungeonGenerator.Instance.ceilingHeight) * DungeonGenerator.Instance.ceilingHeight);
        if (NavMesh.SamplePosition(pos, out hit, 20f, NavMesh.AllAreas) && IsPositionClear(hit.position))
        {
            transform.position = hit.position;
            startPosition = hit.position;
            return;
        }

        if (DungeonGenerator.Instance != null && NavMesh.SamplePosition(DungeonGenerator.Instance.spawnPoint, out hit, 20f, NavMesh.AllAreas) && IsPositionClear(hit.position))
        {
            transform.position = hit.position;
            startPosition = hit.position;
            return;
        }

        for (int i = 0; i < 10; i++)
        {
            Vector3 randomPos = pos + Random.insideUnitSphere * 10f;
            randomPos.y = pos.y;
            if (NavMesh.SamplePosition(randomPos, out hit, 10f, NavMesh.AllAreas) && IsPositionClear(hit.position))
            {
                transform.position = hit.position;
                startPosition = hit.position;
                return;
            }
        }
        Debug.LogWarning($"Could not find clear spawn position for Paladin at {pos}.");
    }

    private bool IsPositionClear(Vector3 pos)
    {
        return !Physics.CheckSphere(pos + Vector3.up * 0.75f, 0.3f, obstacleMask, QueryTriggerInteraction.Ignore);
    }

    private Vector3 GetValidPatrolTarget()
    {
        NavMeshHit hit;
        Vector3 target = startPosition + (Random.value > 0.5f ? transform.forward : -transform.forward) * patrolDistance;
        if (NavMesh.SamplePosition(target, out hit, 20f, NavMesh.AllAreas) && IsPositionClear(hit.position))
            return hit.position;

        for (float radius = 2f; radius <= 30f; radius += 1f)
        {
            for (int i = 0; i < 8; i++)
            {
                Vector3 randomPoint = startPosition + Random.insideUnitSphere * radius;
                randomPoint.y = startPosition.y;
                if (NavMesh.SamplePosition(randomPoint, out hit, radius, NavMesh.AllAreas) && IsPositionClear(hit.position))
                    return hit.position;
            }
        }

        return DungeonGenerator.Instance != null ? DungeonGenerator.Instance.spawnPoint : startPosition;
    }

    private void RepositionIfStuck()
    {
        if (isDead || !agent.isActiveAndEnabled || !agent.isOnNavMesh) return;

        NavMeshHit hit;
        Vector3 currentPos = transform.position;
        Vector3 targetPos = currentState == State.Chasing ? Player.Instance.transform.position : GetValidPatrolTarget();
        for (float radius = 1f; radius <= 10f; radius += 0.5f)
        {
            for (int i = 0; i < 8; i++)
            {
                Vector3 randomPos = targetPos + Random.insideUnitSphere * radius;
                randomPos.y = currentPos.y;
                if (NavMesh.SamplePosition(randomPos, out hit, radius, NavMesh.AllAreas) && IsPositionClear(hit.position))
                {
                    if (NavMesh.CalculatePath(hit.position, targetPos, NavMesh.AllAreas, new NavMeshPath()))
                    {
                        if (agent.isActiveAndEnabled && agent.isOnNavMesh)
                        {
                            agent.Warp(hit.position);
                            stuckTimer = 0f;
                            agent.SetDestination(targetPos);
                            return;
                        }
                    }
                }
            }
        }
        Debug.LogWarning($"Could not unstuck Paladin at {transform.position}.");
    }

    private IEnumerator InitializeAfterDungeon()
    {
        while (DungeonGenerator.Instance == null || !DungeonGenerator.Instance.IsNavMeshReady)
            yield return null;

        SnapToValidSpawn();
        if (agent != null)
        {
            agent.enabled = true;
            if (!agent.Warp(transform.position))
            {
                SnapToValidSpawn();
                agent.Warp(transform.position);
            }

            agent.isStopped = false;
        }
        isIdling = false;
        animator.SetBool("IsIdle", false);
        animator.SetBool("IsWalking", true);
        lastPosition = transform.position;
        stuckCheckTimer = stuckCheckInterval;
        isInitialized = true;

        currentState = State.Roaming;
    }

    private bool CanSeePlayer()
    {
        if (isDead) return false;

        if (Player.Instance == null || Player.Instance.transform == null) return false;

        Vector3 origin = transform.position + Vector3.up * 1.5f;
        Vector3 target = Player.Instance.transform.position + Vector3.up * 1.5f;
        Vector3 dir = (target - origin).normalized;
        float distance = Vector3.Distance(origin, target);

        if (Physics.Raycast(origin, dir, out RaycastHit hit, distance, ~0, QueryTriggerInteraction.Ignore))
        {
            return hit.collider.gameObject == Player.Instance.transform.gameObject || hit.collider.transform.IsChildOf(Player.Instance.transform);
        }
        return false;
    }

    void OnAnimatorMove()
    {
        if (isDead) return;
    }
}