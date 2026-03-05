using UnityEngine;

public class CurrencyDebugCheats : MonoBehaviour
{
    public int goldAmount = 1000;
    public int gemsAmount = 100;

    void Update()
    {
        // G = dar oro
        if (Input.GetKeyDown(KeyCode.G))
        {
            MetaCurrencySystem.Instance.AddGold(goldAmount);
            Debug.Log("Added Gold: " + goldAmount);
        }

        // H = dar gemas
        if (Input.GetKeyDown(KeyCode.H))
        {
            MetaCurrencySystem.Instance.AddGems(gemsAmount);
            Debug.Log("Added Gems: " + gemsAmount);
        }

        // J = resetear monedas
        if (Input.GetKeyDown(KeyCode.J))
        {
            PlayerPrefs.DeleteKey("MetaGold");
            PlayerPrefs.DeleteKey("MetaGems");

            MetaCurrencySystem.Instance.AddGold(0);
            MetaCurrencySystem.Instance.AddGems(0);

            Debug.Log("Currency Reset");
        }
    }
}