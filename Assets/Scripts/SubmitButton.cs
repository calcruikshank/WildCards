using UnityEngine;
using UnityEngine.UI;

public class SubmitButton : MonoBehaviour
{
    private Image buttonImage;
    private Color originalColor;
    private Color darkenedColor;

    private void Start()
    {
        // Get the Image component attached to this object
        buttonImage = GetComponent<Image>();

        // Store the original color of the image
        if (buttonImage != null)
        {
            originalColor = buttonImage.color;

            // Create a slightly darkened color by reducing only the RGB values, keeping alpha the same
            darkenedColor = new Color(originalColor.r * 0.8f, originalColor.g * 0.8f, originalColor.b * 0.8f, originalColor.a);
        }
    }

    private void OnMouseOver()
    {
        // Set the color to the darkened version when the mouse is over the button
        if (buttonImage != null)
        {
            buttonImage.color = darkenedColor;
        }

        GameManager.singleton.playerInScene.hoveringOverSubmit = true;
    }

    private void OnMouseExit()
    {
        // Reset the color to its original state when the mouse exits
        if (buttonImage != null)
        {
            buttonImage.color = originalColor;
        }

        GameManager.singleton.playerInScene.hoveringOverSubmit = false;
    }
}
