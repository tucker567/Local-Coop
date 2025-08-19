// 2025-08-18 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerCursorController : MonoBehaviour
{
    private Camera mainCamera;
    private InputAction clickAction;

    private MultiplayerInputActions inputActions;

    void Start()
    {
        // Initialize the input actions
        inputActions = new MultiplayerInputActions();
        clickAction = inputActions.UI.Click;
        clickAction.Enable();

        // Get the main camera
        UpdateCameraReference();
    }

    void OnEnable()
    {
        // Subscribe to the sceneLoaded event to update the camera reference
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        // Unsubscribe from the sceneLoaded event
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;

        // Disable the input actions to prevent memory leaks
        if (clickAction != null)
        {
            clickAction.Disable();
        }
    }

    void Update()
    {
        // Check for click input
        if (clickAction.WasPerformedThisFrame())
        {
            if (mainCamera == null)
            {
                Debug.LogWarning("Main camera is missing. Updating camera reference.");
                UpdateCameraReference();
            }

            // Simulate a button click
            PointerEventData pointerEventData = new PointerEventData(EventSystem.current)
            {
                position = mainCamera.WorldToScreenPoint(transform.position)
            };

            var results = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerEventData, results);

            foreach (var result in results)
            {
                if (result.gameObject.TryGetComponent<Button>(out Button button))
                {
                    button.onClick.Invoke();
                    Debug.Log($"Button {button.name} clicked by cursor!");
                }
            }
        }
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        // Update the camera reference when a new scene is loaded
        UpdateCameraReference();
    }

    private void UpdateCameraReference()
    {
        mainCamera = Camera.main;

        if (mainCamera == null)
        {
            Debug.LogError("No main camera found in the scene. Ensure a camera is tagged as 'MainCamera'.");
        }
    }

    private void OnDestroy()
    {
        // Cleanup the input actions to prevent memory leaks
        if (inputActions != null)
        {
            inputActions.Dispose();
        }
    }
}