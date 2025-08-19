// 2025-08-18 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

// 2025-08-18 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using UnityEngine;
using UnityEngine.SceneManagement; // For scene transitions
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class LocalMultiplayerManager : MonoBehaviour
{
    [Header("Player Settings")]
    public GameObject playerPrefab;
    public Transform lobbyArea;
    public Transform gameArea;
    public int maxPlayers = 3; // Set to 3 for this system

    private int currentPlayerCount = 0;


    [Header("Cursor Settings")]
    public float cursorSpeed = 5000f;

    [Header("Cursor Colors")]
    public Color[] cursorColors;

    [Header("Voting System")]
    public Button startGameButton; // Reference to the "Start Game" button
    private int votes = 0; // Tracks the number of votes

    [Header("Voting Visualization")]
    public TextMeshProUGUI voteCountText; // Reference to the TextMeshProUGUI element for votes

    [Header("Persistent Canvas")]
    public Canvas persistentCanvas; // Reference to the PersistentCanvas

    private MultiplayerInputActions inputActions;
    private static LocalMultiplayerManager instance;

    private class Player
    {
        public GameObject cursorObject;
        public InputDevice device;
        public Vector2 cursorPosition;
        public Vector2 currentInput; // Store the current input value
        public bool hasVoted; // Tracks if the player has voted
    }

    private readonly Player[] players = new Player[3];


    private void Awake()
    {
        // Ensure there's only one instance of MultiplayerManager
        if (instance != null && instance != this)
        {
            Destroy(gameObject); // Destroy duplicate instances
            return;
        }

        instance = this;

        // Make the MultiplayerManager persistent
        DontDestroyOnLoad(gameObject);

        inputActions = new MultiplayerInputActions();

        // Subscribe to input events
        inputActions.UI.Join.performed += OnJoin;
        inputActions.Gameplay.MoveCursor.performed += OnMoveCursor;
        inputActions.Gameplay.MoveCursor.canceled += OnMoveCursor;

        // Make the PersistentCanvas persistent
        if (persistentCanvas != null)
        {
            DontDestroyOnLoad(persistentCanvas.gameObject);
        }
    }


    private void OnEnable()
    {
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    // 2025-08-18 AI-Tag
    // This was created with the help of Assistant, a Unity Artificial Intelligence product.

    // 2025-08-18 AI-Tag
    // This was created with the help of Assistant, a Unity Artificial Intelligence product.

    // 2025-08-18 AI-Tag
    // This was created with the help of Assistant, a Unity Artificial Intelligence product.

// 2025-08-18 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

private void OnJoin(InputAction.CallbackContext context)
{
    if (!context.performed || currentPlayerCount >= maxPlayers)
        return;

    var device = context.control.device;

    // Check if the device is already assigned
    foreach (var player in players)
    {
        if (player != null && player.device == device)
            return;
    }

    // Assign the device to a new player
    var playerIndex = currentPlayerCount;
    var cursorObject = Instantiate(playerPrefab);

    if (cursorObject == null)
    {
        Debug.LogError("Failed to instantiate PlayerCursor prefab!");
        return;
    }

    // Parent the cursor to the PersistentCanvas
    if (persistentCanvas != null)
    {
        cursorObject.transform.SetParent(persistentCanvas.transform, false);
    }

    // Set the initial position of the cursor
    Vector2 initialPosition = new Vector2(10, -10);
    RectTransform cursorRectTransform = cursorObject.GetComponent<RectTransform>();
    if (cursorRectTransform != null)
    {
        cursorRectTransform.anchoredPosition = initialPosition;
    }
    else
    {
        cursorObject.transform.position = initialPosition;
    }

    // Assign a unique color to the cursor
    Color cursorColor = cursorColors[playerIndex % cursorColors.Length];
    SpriteRenderer spriteRenderer = cursorObject.GetComponent<SpriteRenderer>();
    if (spriteRenderer != null)
    {
        spriteRenderer.color = cursorColor;
    }
    else
    {
        UnityEngine.UI.Image image = cursorObject.GetComponent<UnityEngine.UI.Image>();
        if (image != null)
        {
            image.color = cursorColor;
        }
    }

    players[playerIndex] = new Player
    {
        device = device,
        cursorObject = cursorObject,
        cursorPosition = initialPosition,
        hasVoted = false
    };

    Debug.Log($"Player {playerIndex + 1} joined with device: {device.displayName}");
    Debug.Log($"Cursor instantiated at position: {initialPosition} with color: {cursorColor}");

    currentPlayerCount++;

    // Enable the "Start Game" button when all players are connected
    if (currentPlayerCount == maxPlayers)
    {
        Debug.Log("All players connected. Enabling voting system.");
        EnableVoting();
    }
}

    private void OnMoveCursor(InputAction.CallbackContext context)
    {
        var device = context.control.device;

        // Find the player associated with this device
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] != null && players[i].device == device)
            {
                if (context.performed)
                {
                    players[i].currentInput = context.ReadValue<Vector2>();
                }
                else if (context.canceled)
                {
                    players[i].currentInput = Vector2.zero; // Stop movement when input is canceled
                }
                break;
            }
        }
    }

    private void Update()
    {
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] != null && players[i].currentInput != Vector2.zero)
            {
                // Update the cursor position
                players[i].cursorPosition += players[i].currentInput * cursorSpeed * Time.deltaTime;

                // Clamp the position to stay within the canvas bounds
                RectTransform canvasRect = lobbyArea.GetComponentInParent<Canvas>().GetComponent<RectTransform>();
                Vector2 clampedPosition = ClampToCanvas(players[i].cursorPosition, canvasRect);

                players[i].cursorPosition = clampedPosition;
                players[i].cursorObject.GetComponent<RectTransform>().anchoredPosition = clampedPosition;
            }
        }
    }

    private void EnableVoting()
    {
        Debug.Log("All players connected. Voting enabled.");
        startGameButton.gameObject.SetActive(true); // Show the "Start Game" button
        startGameButton.onClick.AddListener(OnVote); // Add listener for voting

        if (voteCountText != null)
        {
            voteCountText.text = $"Votes: {votes}/{maxPlayers}";
        }

    }

    private void OnVote()
    {
        Debug.Log("Vote received!");
        // Find the player who clicked the button
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] != null && !players[i].hasVoted)
            {
                players[i].hasVoted = true;
                votes++;
                Debug.Log($"Player {i + 1} voted! Total votes: {votes}");
                break;
            }
        }

        // Update the voting text
        if (voteCountText != null)
        {
            voteCountText.text = $"Votes: {votes}/{maxPlayers}";
        }

        // Check if all players have voted
        if (votes >= maxPlayers)
        {
            Debug.Log("All players voted. Starting the game...");
            TransitionToNextScene();
        }
    }


    private void TransitionToNextScene()
    {
        Debug.Log("Transitioning to the next scene...");
        SceneManager.sceneLoaded += OnSceneLoaded; // Subscribe to the sceneLoaded event
        SceneManager.LoadScene("GameScene");
    }

    // 2025-08-18 AI-Tag
    // This was created with the help of Assistant, a Unity Artificial Intelligence product.

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Reposition cursors in the new scene
        foreach (var player in players)
        {
            if (player != null && player.cursorObject != null)
            {
                // Find the new Canvas in the scene
                Transform newParent = GameObject.Find("GameCanvas")?.transform; // Replace "GameCanvas" with the name of your new canvas
                if (newParent != null)
                {
                    player.cursorObject.transform.SetParent(newParent, false);
                }

                // Reset or adjust the cursor position if needed
                RectTransform cursorRectTransform = player.cursorObject.GetComponent<RectTransform>();
                if (cursorRectTransform != null)
                {
                    cursorRectTransform.anchoredPosition = new Vector2(10, -10); // Adjust as needed
                }
            }
        }

        SceneManager.sceneLoaded -= OnSceneLoaded; // Unsubscribe from the event
        Debug.Log("Scene loaded and cursors repositioned.");
    }

    private Vector2 ClampToCanvas(Vector2 position, RectTransform canvasRect)
    {
        // Get the canvas size in local space
        Vector2 canvasSize = canvasRect.sizeDelta;

        // Clamp the position to the canvas bounds
        float clampedX = Mathf.Clamp(position.x, -canvasSize.x / 2, canvasSize.x / 2);
        float clampedY = Mathf.Clamp(position.y, -canvasSize.y / 2, canvasSize.y / 2);

        return new Vector2(clampedX, clampedY);
    }
    
}