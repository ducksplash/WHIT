using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;

public class PortalController : MonoBehaviour
{
    [Header("Portal Details")] 
    public PortalType selectedPortal = PortalType.RockyPortal;

    [Header("Health")]
    public int maxHealth = 40;
    public int health = 40;

    [Header("Enemy Spawning")]
    public GameObject enemyPrefab;
    public int minEnemies = 1;
    public int maxEnemies = 3;
    private List<GameObject> spawnedEnemies = new List<GameObject>();

    [Header("FX")]
    public GameObject portalAperture;
    public GameObject portalcrackLayer;
    public List<Rigidbody> rbList;

    private bool isDead;
    private Material crackMaterial;

    [Header("Crack Settings (min–max)")]
    public float minDensity = 2f;
    public float maxDensity = 25f;
    public float minWidth = 0.0016f;
    public float maxWidth = 0.1f;
    public float minEmission = 0f;
    public float maxEmission = 10f;

    public Collider portalCollider;
    
    void Start()
    {
        if (portalcrackLayer != null)
        {
            Renderer rend = portalcrackLayer.GetComponent<Renderer>();
            if (rend != null)
            {
                crackMaterial = rend.material;
            }
        }

        ApplyCrackValues(1f); // full health = minimal cracks
        StartCoroutine(MonitorPlayerDistance());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (health <= 0 || isDead) return;

        // if (other.CompareTag("PlayerMelee"))
        // {
        //     // int meleeDamage = other.GetComponent<Weapon>()?.baseDamage ?? 1;
        //     // health = Mathf.Max(0, health - meleeDamage);
        //     // GameMaster.Instance.PlayerOne.playerCollisionScript.canAttack = false;
        //     // UpdateCracksByHealth();
        //     //
        //     // if (health <= 0)
        //     // {
        //     //     Die();
        //     // }
        // }
    }

    private void UpdateCracksByHealth()
    {
        portalcrackLayer.SetActive(true);
        if (crackMaterial == null) return;

        float healthFraction = Mathf.Clamp01((float)health / maxHealth);
        ApplyCrackValues(healthFraction);
    }

    private void ApplyCrackValues(float healthFraction)
    {
        if (crackMaterial == null) return;

        float density = Mathf.Lerp(maxDensity, minDensity, healthFraction); 
        float width = Mathf.Lerp(maxWidth, minWidth, healthFraction);
        float emission = Mathf.Lerp(maxEmission, minEmission, healthFraction);

        crackMaterial.SetFloat("_Density", density);
        crackMaterial.SetFloat("_CrackWidth", width);
        crackMaterial.SetFloat("_EmissionStrength", emission);
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;
        portalCollider.enabled = false;

        portalAperture.SetActive(false);
        portalcrackLayer.SetActive(false);
        Debug.Log("Portal died");

        StopAllCoroutines();

        foreach (var rb in rbList)
        {
            rb.useGravity = true;
            rb.isKinematic = false;
            rb.constraints = RigidbodyConstraints.None;
        }

        StartCoroutine(FinallyDie());
    }

    private IEnumerator FinallyDie()
    {
        yield return new WaitForSeconds(3);
        Destroy(gameObject);
    }

    private IEnumerator MonitorPlayerDistance()
    {
        while (Player.Instance == null) yield return null;

        while (!isDead)
        {
            float distance = Vector3.Distance(transform.position, Player.Instance.transform.position);
            if (distance <= 5f && spawnedEnemies.Count < maxEnemies)
            {
                yield return new WaitForSeconds(10f);
                SpawnEnemies();
            }

            yield return new WaitForSeconds(1f);
        }
    }

    private void SpawnEnemies()
    {
        if (isDead) return;
        int enemyCount = Random.Range(minEnemies, maxEnemies + 1);
        int toSpawn = Mathf.Min(enemyCount - spawnedEnemies.Count, maxEnemies - spawnedEnemies.Count);

        for (int i = 0; i < toSpawn; i++)
        {
            Vector2 offset = Random.insideUnitCircle * Random.Range(2f, 3f);
            Vector3 spawnPos = new Vector3(transform.position.x + offset.x, transform.position.y, transform.position.z + offset.y);

            // Enhanced NavMesh sampling with fallback
            if (FindValidSpawnPosition(ref spawnPos))
            {
                GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
                if (enemy.TryGetComponent(out ScorpionController scorpion))
                    scorpion.portal = this;
                else if (enemy.TryGetComponent(out PaladinController paladin))
                    paladin.portal = this;
                spawnedEnemies.Add(enemy);
            }
        }
    }

    private bool FindValidSpawnPosition(ref Vector3 position)
    {
        // Initial sampling with increased range
        if (NavMesh.SamplePosition(position, out NavMeshHit hit, 10f, NavMesh.AllAreas))
        {
            position = hit.position;
            return true;
        }

        // Fallback: Try multiple attempts with random offsets
        for (int i = 0; i < 5; i++)
        {
            Vector3 randomOffset = new Vector3(Random.Range(-2f, 2f), 0f, Random.Range(-2f, 2f));
            Vector3 testPos = position + randomOffset;
            if (NavMesh.SamplePosition(testPos, out NavMeshHit hitFallback, 15f, NavMesh.AllAreas))
            {
                position = hitFallback.position;
                return true;
            }
        }

        // Last resort: Use portal position as base
        if (NavMesh.SamplePosition(transform.position, out NavMeshHit portalHit, 15f, NavMesh.AllAreas))
        {
            position = portalHit.position;
            return true;
        }

        return false;
    }

    public void RemoveEnemy(GameObject enemy)
    {
        spawnedEnemies.Remove(enemy);
        if (spawnedEnemies.Count < maxEnemies && Vector3.Distance(transform.position, Player.Instance.transform.position) <= 10f)
        {
            SpawnEnemies();
        }
    }
}

public enum PortalType
{
    RockyPortal,
    FuturisticPortal
}