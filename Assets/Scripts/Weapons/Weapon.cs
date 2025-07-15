using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "Scriptable Objects/Weapons/Weapon")]
public class Weapon : ScriptableObject
{
    public float minDamage;
    public float maxDamage;

    public float attackDelay;
    public float attackRange;
}
