using UnityEngine;

public class HeroOpenDetail : MonoBehaviour
{
    void OnMouseDown()
    {
        var hero =
            HeroSelectionManager.Instance.SelectedHero;

        ChampDetailPanelUI.Instance?.Open(hero);
    }
}