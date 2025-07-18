using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "Scriptable Objects/Weapons/Weapon")]
public class Weapon : ScriptableObject
{
    public string weaponName = "Default Weapon";

    public int minDamage = 1;
    public int maxDamage = 10;

    public int maxEnemiesHit = 1;

    public WeaponSpeed weaponSpeed = WeaponSpeed.Fast;
    public float AttackWindup => GetAttackWindup();
    public float AttackDelay=> GetAttackDelay();
    public float attackRange = 1;
    public float attackHeightFactor = 1;

    public float knockbackForce = 5f;

    private float GetAttackWindup()
    {
        return weaponSpeed switch
        {
            WeaponSpeed.Slow => 0.4f,
            WeaponSpeed.Fast => 0.2f,
            WeaponSpeed.VeryFast => 0.1f,
            _ => 0.25f,
        };
    }
    
    
    private float GetAttackDelay()
    {
        return weaponSpeed switch
        {
            WeaponSpeed.Slow => 0.75f,
            WeaponSpeed.Fast => 0.5f,
            WeaponSpeed.VeryFast => 0.25f,
            _ => 0.5f,
        };
    }
}
