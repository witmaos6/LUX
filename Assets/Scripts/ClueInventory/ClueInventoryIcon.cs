using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClueInventoryIcon : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private TMP_Text displayName;
    [SerializeField] private Color existColor;

    public void SetExist(string inDisplayName)
    {        
        image.color = existColor;
        displayName.text = inDisplayName;
    }

    public bool IsExist()
    {
        return image.color == existColor;
    }
}
