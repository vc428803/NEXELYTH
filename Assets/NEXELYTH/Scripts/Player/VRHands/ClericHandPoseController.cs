using UnityEngine;
using UnityEngine.InputSystem;

public class ClericHandPoseController : MonoBehaviour
{
    [SerializeField] private Animator handAnimator;
    [SerializeField] private InputActionReference castAction;

    private static readonly int AnimationIndex =
        Animator.StringToHash("animationIndex");

    private const int IdlePose = 8;
    private const int CastPose = 0;

    private void OnEnable()
    {
        castAction.action.Enable();
    }

    private void OnDisable()
    {
        castAction.action.Disable();
    }

    private void Update()
    {
        float value = castAction.action.ReadValue<float>();

        Debug.Log($"Cast Action = {value}");

        handAnimator.SetInteger(
            AnimationIndex,
            value > 0.5f ? CastPose : IdlePose
        );
    }
}