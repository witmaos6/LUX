using UnityEngine;

public class EventToSaveRelay : MonoBehaviour
{
    [SerializeField] private GameEvent receiveEvent;
    [SerializeField] private Transform player;
    [SerializeField] private CameraController2D cameraController;

    private void Awake()
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
                player = playerObject.transform;
        }

        if (cameraController == null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
                cameraController = mainCamera.GetComponent<CameraController2D>();
        }
    }

    private void OnEnable()
    {
        if (receiveEvent != null)
            GameEventManager.Subscribe(receiveEvent, Save);
    }

    private void OnDisable()
    {
        if (receiveEvent != null)
            GameEventManager.Unsubscribe(receiveEvent, Save);
    }

    private void Save()
    {
        Vector3 position = player != null ? player.position : transform.position;
        string wallpaperId = cameraController != null && cameraController.CurrentWallpaper != null
            ? cameraController.CurrentWallpaper.name
            : null;

        SaveManager.SetRespawnPoint(position, wallpaperId);
    }
}
