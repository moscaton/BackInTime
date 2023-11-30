using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;



public class DinoBossDefeat : MonoBehaviour
{

    public GameObject canvas;
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            canvas.SetActive(true);
        }
        if (other.gameObject.CompareTag("Player") && Input.GetKey(KeyCode.Q))
        {
            Debug.Log("does this collide");
            SceneManager.LoadScene("TimeGodScene");
        }


    }

    private void OnTriggerExit2D(Collider2D other)
    {
        canvas.SetActive(false);
    }
}
