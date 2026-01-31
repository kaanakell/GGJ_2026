using UnityEngine;

public class ComboManager : MonoBehaviour
{
    [Header("Combo Settings")]
    public float comboWindow = 2.0f;
    public float switchMultiplier = 1.5f;

    [Header("Debug")]
    [SerializeField] private DamageType lastAttackType;
    [SerializeField] private float lastAttackTime;

    private void Start()
    {
        lastAttackType = DamageType.Claw;
        lastAttackTime = -100f;
    }

    public float GetDamageMultiplier(DamageType incomingType)
    {
        if (Time.time - lastAttackTime > comboWindow)
        {
            return 1f;
        }

        if (incomingType != lastAttackType)
        {
            return switchMultiplier;
        }

        return 1f;
    }

    public void RegisterHit(DamageType type)
    {
        lastAttackType = type;
        lastAttackTime = Time.time;
    }
}