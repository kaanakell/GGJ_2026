using UnityEngine;
using Unity.Cinemachine;

public class CameraImpulseManager : MonoBehaviour
{
    public static CameraImpulseManager Instance;
    public CinemachineImpulseSource impulseSource;

    void Awake()
    {
        Instance = this;
    }

    public void PlayImpulse(Vector2 direction, float strength)
    {
        impulseSource.DefaultVelocity =
            new Vector3(direction.x, direction.y, 0f) * strength;

        impulseSource.GenerateImpulse();
    }
}
