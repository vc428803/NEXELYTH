
using UnityEngine;
using UnityEngine.UI;

public class NexelythSAOCategoryVisual : MonoBehaviour
{
    [Header("Circle Images")]
    [SerializeField] private Image outerCircle;
    [SerializeField] private Image whiteCircle;
    [SerializeField] private Image innerCircle;
    [SerializeField] private Image categoryIcon;

    [Header("Selected Colors")]
    [SerializeField]
    private Color selectedColor =
        new Color32(231, 164, 0, 255);

    [SerializeField] private Color selectedIconColor = Color.white;

    [Header("Unselected Colors")]
    [SerializeField]
    private Color unselectedColor =
        new Color32(215, 215, 215, 255);

    [SerializeField]
    private Color unselectedIconColor =
        new Color32(190, 190, 190, 255);

    public void SetSelected(bool selected)
    {
        Color circleColor =
            selected ? selectedColor : unselectedColor;

        if (outerCircle != null)
            outerCircle.color = circleColor;

        if (whiteCircle != null)
            whiteCircle.color = Color.white;

        if (innerCircle != null)
            innerCircle.color = circleColor;

        if (categoryIcon != null)
        {
            categoryIcon.color = selected
                ? selectedIconColor
                : unselectedIconColor;
        }
    }
}
