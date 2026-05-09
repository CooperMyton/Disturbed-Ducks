using UnityEngine;

/// Lightweight projectile spawned by SplitAbility.
/// Physics and existing Destructible/TargetEnemy/Collectible scripts
/// handle all interactions automatically via Rigidbody.
/// Requires: Rigidbody, SphereCollider
public class MiniDuck : MonoBehaviour
{
    [SerializeField] private float lifetime = 12f;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }
}