using UnityEngine;
using Unity.Cinemachine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;

    [SerializeField] CinemachineImpulseSource impulseSource;

    void Awake()
    {
        Instance = this;
    }

    public void Shake(float magnitude)
    {
        impulseSource.GenerateImpulse(magnitude);
    }
}