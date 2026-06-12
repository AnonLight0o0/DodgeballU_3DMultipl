using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ConnectingText : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI textDisplay;

    [Header("Settings")]
    [Tooltip("In Seconds")]
    [SerializeField] private float changeInterval = 3.0f;
    [SerializeField] private List<string> textLines; // The list of phrases to cycle through

    private int currentIndex = 0;

    void Start()
    {
        StartCoroutine(TypeTextSequence());
    }

    IEnumerator TypeTextSequence()
    {
        while (true) // This creates an infinite loop so it keeps running
        {
            // Set the text to the current phrase
            textDisplay.text = textLines[currentIndex];

            // Wait for the specified interval before moving to the next line
            yield return new WaitForSeconds(changeInterval);

            // Move to the next index, and loop back to 0 if we hit the end
            currentIndex = (currentIndex + 1) % textLines.Count;
        }
    }
}