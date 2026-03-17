using UnityEngine;
using System.Collections;

public class MetaBootstrap : MonoBehaviour
{
    IEnumerator Start()
    {
        yield return InitializeSystems();
    }

    IEnumerator InitializeSystems()
    {
        // esperar ItemDatabase
        while (ItemDatabase.Instance == null)
            yield return null;

        // esperar currency
        while (MetaCurrencySystem.Instance == null)
            yield return null;

        // esperar hero progress
        while (HeroProgressSystem.Instance == null)
            yield return null;

        // esperar hero selection
        while (HeroSelectionManager.Instance == null)
            yield return null;

        // esperar inventario
        while (MetaInventory.Instance == null)
            yield return null;

        Debug.Log("META SYSTEMS INITIALIZED");
    }
}