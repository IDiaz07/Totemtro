using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewsDatabase", menuName = "Game/News Database")]
public class NewsDatabase : ScriptableObject
{
    public List<NewsData> newsList;
}