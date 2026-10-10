
using UnityEngine;

public class NexelythMapPlayerMarker : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private NexelythMapPositionTester positionTester;

    [SerializeField]
    private Renderer mapRenderer;

    private static readonly int PlayerUV =
        Shader.PropertyToID("_PlayerUV");

    private MaterialPropertyBlock propertyBlock;

    private void Awake()
    {
        propertyBlock = new MaterialPropertyBlock();
    }

    private void LateUpdate()
    {
        if (positionTester == null || mapRenderer == null)
            return;

        if (!positionTester.IsInitialized)
            return;

        Vector2 uv = positionTester.CurrentUV;

        mapRenderer.GetPropertyBlock(propertyBlock);

        propertyBlock.SetVector(
            PlayerUV,
            new Vector4(uv.x, uv.y, 0f, 0f)
        );

        mapRenderer.SetPropertyBlock(propertyBlock);
    }
}
