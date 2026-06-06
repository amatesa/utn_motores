using System;
using System.Collections.Generic;
using UnityEngine;


[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
public class LanternProtectionZone : MonoBehaviour
{

    public event Action<GameObject> OnEnemyEnteredZone;


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


    public void SetActive(bool active)
    {
        isActive = active;

        if (triggerCollider != null)
            triggerCollider.enabled = active;

        if (!active)
            ClearTrackedEnemies();
    }


    public void SetProtectionEfficiency(float efficiency)
    {
        protectionEfficiency = Mathf.Clamp01(efficiency);
    }


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
