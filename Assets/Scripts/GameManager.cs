using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public GameObject cursor;

    private PlayerInput _playerInput;
    
    private void Awake()
    {
        Cursor.visible = false;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        _playerInput = FindObjectOfType<PlayerInput>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_playerInput.currentControlScheme == "Gamepad")
        {
            cursor.SetActive(false);
        }
        else
        {
            cursor.SetActive(true);
        }

        Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        pos.z = 1f;
        cursor.transform.position = pos;
    }
}
