using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class ShootTrigger : MonoBehaviour
{
    private Coroutine showModeCoroutine;
    public TextMeshProUGUI modeText;
    public ARController arController;
    public Button shootButton;
    private bool shootMode = false;

    // Provide a public method to get the value of shootMode
    public bool IsShootModeActive()
    {
        return shootMode;
    }

    public void SetShootModeActive(bool active)
    {
        shootMode = active;
    }
    
    void Start()
    {
        // // Get the ARPlacement component
        // arController = FindObjectOfType<ARController>();

        // // Get the Button component
        // shootButton = GetComponent<Button>();

        // Add a listener to the button's onClick event
        shootButton.onClick.AddListener(ToggleShootMode);
    }

    void ToggleShootMode()
    {
        // Toggle the shoot mode
        shootMode = !shootMode;

        // Update the button text
        if (shootMode)
        {
            modeText.text = "Shoot Mode Active";
        }
        else
        {
            modeText.text = "Shoot Mode Inactive";
        }
        if (showModeCoroutine != null)
    {
        StopCoroutine(showModeCoroutine);
    }

    // Start a new ShowModeText coroutine and store it in showModeCoroutine
    showModeCoroutine = StartCoroutine(ShowModeText());
    }

    IEnumerator ShowModeText()
    {
        yield return new WaitForSeconds(2);
        modeText.text = "";
    }

}
