using System;
using UnityEngine;
using UnityEngine.UI;

public class MaskManager : MonoBehaviour
{
    public GameObject tentacleMaskObject;
    public GameObject clawMaskObject;

    public ClawMechanic clawMechanic;
    public TentacleMechanic tentacleMechanic;

    public enum MaskType { Tentacle, Claw }
    public MaskType currentMask;
    public UIManager uiManager;

    public MaskType CurrentMask => currentMask;

    void Start()
    {
        EquipMask(MaskType.Tentacle);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ToggleMask();
        }
    }

    private void ToggleMask()
    {
        if (currentMask == MaskType.Tentacle)
        {
            EquipMask(MaskType.Claw);
        }
        else
        {
            EquipMask(MaskType.Tentacle);
        }
    }

    private void EquipMask(MaskType mask)
    {
        currentMask = mask;

        if (uiManager != null) uiManager.UpdateMaskIcon(mask);

        if (mask == MaskType.Tentacle)
        {
            tentacleMaskObject.SetActive(true);
            clawMaskObject.SetActive(false);

            if (clawMechanic != null)
            {
                clawMechanic.ResetClawState();
                clawMechanic.enabled = false;
            }

            if (tentacleMechanic != null) tentacleMechanic.enabled = true;

            // Optional: Sound effect / animation for tentacle equip
        }
        else
        {
            tentacleMaskObject.SetActive(false);
            clawMaskObject.SetActive(true);

            if (tentacleMechanic != null)
            {
                tentacleMechanic.ResetGrapple();
                tentacleMechanic.enabled = false;
            }

            if (clawMechanic != null) clawMechanic.enabled = true;

            // Optional: Sound effect / animation for claw equip
        }
    }
}