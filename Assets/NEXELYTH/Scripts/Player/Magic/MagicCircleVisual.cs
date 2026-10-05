using UnityEngine;

public class MagicCircleVisual : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform outerRing;
    [SerializeField] private Transform innerRune;

    [Header("Rotation")]
    [SerializeField] private float outerRotationSpeed = 20f;
    [SerializeField] private float innerRotationSpeed = -35f;

    private void Update()
    {
        if (outerRing != null)
        {
            outerRing.Rotate(
                Vector3.forward,
                outerRotationSpeed * Time.deltaTime,
                Space.Self
            );
        }

        if (innerRune != null)
        {
            innerRune.Rotate(
                Vector3.forward,
                innerRotationSpeed * Time.deltaTime,
                Space.Self
            );
        }
    }
}