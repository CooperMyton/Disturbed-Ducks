using System.Collections.Generic;
using UnityEngine;

public class DuckFamilyController : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject familyDuckPrefab;

    [Tooltip("Optional model prefab placed under each family duck. Leave empty to use the prefab visuals.")]
    [SerializeField] private GameObject familyDuckFlightModel;
    [SerializeField] private GameObject familyDuckCrashedModel;

    [Header("Formation")]
    [SerializeField] private float followSharpness = 18f;

    private readonly List<GameObject> _familyDucks = new List<GameObject>();

    private Rigidbody _leaderRb;
    private Collider _leaderCollider;
    private bool _inFormation;
    private bool _hasCrashed;

    private void Awake()
    {
        _leaderRb = GetComponent<Rigidbody>();
        _leaderCollider = GetComponent<Collider>();
    }

    private void FixedUpdate()
    {
        if (!_inFormation || _hasCrashed)
            return;

        DuckFamilyAbility ability = GetFamilyAbility();
        if (ability == null)
            return;

        for (int i = 0; i < _familyDucks.Count; i++)
        {
            GameObject duck = _familyDucks[i];
            if (duck == null)
                continue;

            Rigidbody rb = duck.GetComponent<Rigidbody>();
            if (rb == null)
                continue;

            Vector3 targetPosition = GetFormationPosition(i, ability);
            Quaternion targetRotation = transform.rotation;

            float followT = 1f - Mathf.Exp(-followSharpness * Time.fixedDeltaTime);

            rb.MovePosition(Vector3.Lerp(
                rb.position,
                targetPosition,
                followT
            ));

            rb.MoveRotation(Quaternion.Slerp(
                rb.rotation,
                targetRotation,
                followT
            ));
        }
    }

    public void OnLaunched()
    {
        CleanupFamily();

        DuckFamilyAbility ability = GetFamilyAbility();
        if (ability == null || familyDuckPrefab == null)
            return;

        AbilityController abilityController = GetComponent<AbilityController>();

        int totalFamilyCount =
            Mathf.Max(1, ability.startingFamilyCount + Mathf.RoundToInt(abilityController?.RadiusBoost ?? 0f));

        int followerCount = totalFamilyCount - 1;

        float mass =
            ability.familyDuckMass + (abilityController?.DamageBoost ?? 0f);

        _hasCrashed = false;
        _inFormation = true;

        for (int i = 0; i < followerCount; i++)
            SpawnFamilyDuck(i, ability, mass);
    }

    public void OnLeaderCrashed()
    {
        CrashFamily();
    }

    public void OnFamilyDuckCrashed()
    {
        if (_hasCrashed)
            return;

        GetComponent<DuckImpact>()?.Crash();
        CrashFamily();
    }

    public void OnReset()
    {
        CleanupFamily();
    }

    private DuckFamilyAbility GetFamilyAbility()
    {
        return GetComponent<DuckController>()?.Definition?.ability as DuckFamilyAbility;
    }

    private void SpawnFamilyDuck(int formationIndex, DuckFamilyAbility ability, float mass)
    {
        Vector3 position = GetFormationPosition(formationIndex, ability);

        GameObject duck = Instantiate(familyDuckPrefab, position, transform.rotation);
        duck.name = $"FamilyDuck-{formationIndex + 1}";

        Rigidbody rb = duck.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.mass = mass;
            rb.useGravity = false;
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        Collider familyCollider = duck.GetComponent<Collider>();
        if (_leaderCollider != null && familyCollider != null)
            Physics.IgnoreCollision(_leaderCollider, familyCollider);

        DuckFamilyMember member = duck.GetComponent<DuckFamilyMember>();
        if (member == null)
            member = duck.AddComponent<DuckFamilyMember>();

        member.Initialize(this);

        if (familyDuckFlightModel != null)
        {
            foreach (Renderer renderer in duck.GetComponentsInChildren<Renderer>())
                renderer.enabled = false;

            Instantiate(familyDuckFlightModel, duck.transform.position, duck.transform.rotation, duck.transform);
        }

        foreach (GameObject otherDuck in _familyDucks)
        {
            if (otherDuck == null || familyCollider == null)
                continue;

            Collider otherCollider = otherDuck.GetComponent<Collider>();
            if (otherCollider != null)
                Physics.IgnoreCollision(familyCollider, otherCollider);
        }

        _familyDucks.Add(duck);
    }

    private Vector3 GetFormationPosition(int formationIndex, DuckFamilyAbility ability)
    {
        int row = formationIndex / 2 + 1;
        int side = formationIndex % 2 == 0 ? -1 : 1;

        float angle = side * ability.vAngle;

        Vector3 direction =
            Quaternion.AngleAxis(angle, transform.up) * -transform.forward;

        return transform.position + direction.normalized * ability.spacing * row;
    }

    private void CrashFamily()
    {
        if (_hasCrashed)
            return;

        _hasCrashed = true;
        _inFormation = false;

        Vector3 leaderVelocity =
            _leaderRb != null ? _leaderRb.linearVelocity : transform.forward * 8f;

        for (int i = 0; i < _familyDucks.Count; i++)
        {
            GameObject duck = _familyDucks[i];
            if (duck == null)
                continue;

            if (familyDuckCrashedModel != null)
            {
                foreach (Transform child in duck.transform)
                    Destroy(child.gameObject);

                Instantiate(familyDuckCrashedModel, duck.transform.position, duck.transform.rotation, duck.transform);
            }

            Rigidbody rb = duck.GetComponent<Rigidbody>();
            if (rb == null)
                continue;

            rb.isKinematic = false;
            rb.useGravity = true;
            rb.linearVelocity = leaderVelocity + Random.insideUnitSphere * 2f;
            rb.angularVelocity = Random.insideUnitSphere * 4f;
        }
    }

    private void CleanupFamily()
    {
        foreach (GameObject duck in _familyDucks)
        {
            if (duck != null)
                Destroy(duck);
        }

        _familyDucks.Clear();
        _inFormation = false;
        _hasCrashed = false;
    }
}