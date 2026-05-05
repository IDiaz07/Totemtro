using UnityEngine;

[CreateAssetMenu(fileName = "NewsData", menuName = "Game/News Item")]
public class NewsData : ScriptableObject
{
    public string id;
    public string title;
    public string date;
    [TextArea(5, 20)]
    public string content;

    public bool isImportant;
    public Sprite image;
}