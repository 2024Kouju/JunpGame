
using UnityEngine;
using TMPro;

public class InputPlaceholderController : MonoBehaviour
{
    [Header("“ü—Í—“")]
    public TMP_InputField inputField;

    [Header("Placeholder")]
    public GameObject placeholder;

    void Update()
    {
        if (inputField == null || placeholder == null)
            return;

        // “ü—Í—“‚ð‘I‘ð‚µ‚Ä‚¢‚éŠÔ‚Í”ñ•\Ž¦
        placeholder.SetActive(!inputField.isFocused);
    }
}
