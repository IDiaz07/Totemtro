using UnityEngine;

public class ExtractionPoint : MonoBehaviour
{
    bool playerInside = false;

    void Update()
    {
        if (!playerInside) return;

        if (Input.GetKeyDown(KeyCode.R))
        {
            Extract();
        }
    }

    void Extract()
    {
        Debug.Log("Extraction triggered");

        GameManager.Instance.ExtractRun();
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
            playerInside = true;
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
            playerInside = false;
    }
}