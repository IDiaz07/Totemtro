using UnityEngine;

public class ShopTabButton : MonoBehaviour
{
    public int sectionIndex; // 0=Offers, 1=Heroes, etc
    public ShopSnapController snapController;

    public void OnClick()
    {
        snapController.SnapTo(sectionIndex);
    }
}