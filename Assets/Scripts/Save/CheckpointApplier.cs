using UnityEngine;

public class CheckpointApplier : MonoBehaviour
{
    [SerializeField] private PlayerController player;
    [SerializeField] private CameraController2D cameraController;

    private void Awake()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.GetComponent<PlayerController>();
        }

        if (cameraController == null)
        {
            GameObject mainCamera = GameObject.Find("Main Camera");
            if (mainCamera != null)
                cameraController = mainCamera.GetComponent<CameraController2D>();
        }
    }

    private void Start()
    {
        if (player != null)
        {
            player.RestoreInventory(SaveManager.Inventory);
        }

        if (!SaveManager.HasSave) return;

        if (player != null)
        {
            player.transform.position = SaveManager.RespawnPosition;
        }

        if (cameraController != null && !string.IsNullOrEmpty(SaveManager.RespawnWallpaperId))
        {
            GameObject wallpaperObj = GameObject.Find(SaveManager.RespawnWallpaperId);
            if (wallpaperObj != null)
                cameraController.SetWallpaper(wallpaperObj.transform);
        }
    }
}
