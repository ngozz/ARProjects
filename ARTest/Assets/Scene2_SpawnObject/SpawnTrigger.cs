using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class SpawnTrigger : MonoBehaviour
{
    private Coroutine showModeCoroutine;
    public TextMeshProUGUI modeText;
    public ARController arController;
    public Button spawnButton;
    private bool spawnMode = false;

    // Provide a public method to get the value of spawnMode
    public bool IsSpawnModeActive()
    {
        return spawnMode;
    }

    public void SetSpawnModeActive(bool active)
    {
        spawnMode = active;
    }
    
    void Start()
    {
        // // Get the ARPlacement component
        // arController = FindObjectOfType<ARController>();

        // // Get the Button component
        // spawnButton = GetComponent<Button>();

        // Add a listener to the button's onClick event
        spawnButton.onClick.AddListener(ToggleSpawnMode);
    }

    void ToggleSpawnMode()
    {
        // Toggle the spawn mode
        spawnMode = !spawnMode;

        // Update the button text
        if (spawnMode)
        {
            modeText.text = "Spawn Mode Active";
        }
        else
        {
            modeText.text = "Spawn Mode Inactive";
        }
        // Stop the currently running ShowModeText coroutine
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
