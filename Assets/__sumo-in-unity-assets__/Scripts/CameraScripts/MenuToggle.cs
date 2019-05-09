using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuToggle : MonoBehaviour
{
    private Canvas CanvasObject; // Assign in inspector
    public GameObject menu;
    private bool isMenuVisible;
    private CursorLockMode wantedCursorLockMode;

    // Apply requested cursor state
    private static void SetCursorState(CursorLockMode lockMode)
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.lockState = lockMode;
        // Hide cursor when locking
        Cursor.visible = (CursorLockMode.Locked != lockMode);
    }

    void Start()
    {
        isMenuVisible = false;
        menu.SetActive(isMenuVisible);
    }
}
