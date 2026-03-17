using System;

public interface ISaveSystem
{
    void Save(string key, string data);
    string Load(string key);
}