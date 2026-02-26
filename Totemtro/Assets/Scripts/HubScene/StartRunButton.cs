using UnityEngine;

public class StartRunButton : MonoBehaviour
{
    public void OnClick()
    {
        HubUIManager.Instance.OpenConfirmation();
    }
}