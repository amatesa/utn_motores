using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Trigger zone around the player while the lantern is active.
/// It does not control enemy AI directly; it exposes nearby enemy pressure and
/// protection signals so future enemy systems can choose to retreat.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
public class LanternProtectionZone : MonoBehaviour
{
    /// <summary>
    /// Raised when an enemy enters the lantern protection zone.
    /// </summary>
    public event Action<GameObject> OnEnemyEnteredZone;

    /// <summary>
    /// Raised when an enemy exits the lantern protection zone.
    /// </summary>
    public event Action<GameObject> OnEnemyExitedZone;

    [Header("Detection")]
    [SerializeField] private string enemyTag = "Enemy";
    [SerializeField] private Collider triggerCollider;

    [Header("Runtime")]
    [SerializeField] private bool isActive;
    [SerializeField] private float protectionEfficiency = 1f;

    [Header("Debug")]
    [SerializeField] private bool logDebugMessages = false;

    private readonly HashSet<GameObject> enemiesInZone = new HashSet<GameObject>();

    public bool IsActive => isActive;
    public int EnemyPressureCount => enemiesInZone.Count;
    public float ProtectionEfficiency => protectionEfficiency;
    public float EnemyPressureMultiplier => 1f + EnemyPressureCount;

    private void Awake()
    {
        if (triggerCollider == null)
            triggerCollider = GetComponent<Collider>();

        triggerCollider.isTrigger = true;
        SetActive(false);
    }

    /// <summary>
    /// Enables or disables the protection trigger.
    /// </summary>
    public void SetActive(bool active)
    {
        isActive = active;

        if (triggerCollider != null)
            triggerCollider.enabled = active;

        if (!active)
            ClearTrackedEnemies();
    }

    /// <summary>
    /// Sets the current phase-scaled protection efficiency for future enemy reactions.
    /// </summary>
    public void SetProtectionEfficiency(float efficiency)
    {
        protectionEfficiency = Mathf.Clamp01(efficiency);
    }

    /// <summary>
    /// Returns true when the supplied enemy is currently inside the active zone.
    /// </summary>
    public bool ContainsEnemy(GameObject enemy)
    {
        return enemy != null && enemiesInZone.Contains(enemy);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isActive || !TryResolveEnemy(other, out GameObject enemy))
            return;

        if (enemiesInZone.Add(enemy))
        {
            OnEnemyEnteredZone?.Invoke(enemy);
            Log($"Enemy entered zone: {enemy.name}");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!TryResolveEnemy(other, out GameObject enemy))
            return;

        if (enemiesInZone.Remove(enemy))
        {
            OnEnemyExitedZone?.Invoke(enemy);
            Log($"Enemy exited zone: {enemy.name}");
        }
    }

    private bool TryResolveEnemy(Collider other, out GameObject enemy)
    {
        enemy = null;

        if (other == null)
            return false;

        if (other.CompareTag(enemyTag))
        {
            enemy = other.gameObject;
            return true;
        }

        if (other.attachedRigidbody != null && other.attachedRigidbody.CompareTag(enemyTag))
        {
            enemy = other.attachedRigidbody.gameObject;
            return true;
        }

        Transform root = other.transform.root;
        if (root != null && root.CompareTag(enemyTag))
        {
            enemy = root.gameObject;
            return true;
        }

        return false;
    }

    private void ClearTrackedEnemies()
    {
        if (enemiesInZone.Count == 0)
            return;

        List<GameObject> enemies = new List<GameObject>(enemiesInZone);
        enemiesInZone.Clear();

        foreach (GameObject enemy in enemies)
        {
            if (enemy != null)
                OnEnemyExitedZone?.Invoke(enemy);
        }
    }

    private void Log(string message)
    {
        if (logDebugMessages)
            Debug.Log($"[LanternProtectionZone] {message}");
    }
}
