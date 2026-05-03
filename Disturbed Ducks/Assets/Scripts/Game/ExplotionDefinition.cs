using UnityEngine;

[CreateAssetMenu(fileName = "NewExplosionDefinition", menuName = "Game/Explosion Definition")]
public class ExplosionDefinition : ScriptableObject
{
    [Header("Base Explosion — on crash, ability not yet purchased")]
    public float baseRadius = 2f;
    public float baseDamage = 30f;
    public float baseDelay  = 0.2f;

    [Header("Ability Explosion — on crash with ability purchased, or triggered by Shift")]
    public float abilityRadius = 5f;
    public float abilityDamage = 80f;
    public float abilityDelay  = 1.5f;

    [Header("Visual")]
    [Tooltip("Assign a URP transparent material for a nice look — leave null for solid sphere")]
    public Material explosionMaterial;
    public Color    explosionColor = new Color(1f, 0.4f, 0f, 1f);
    public float    visualDuration = 0.4f;
}