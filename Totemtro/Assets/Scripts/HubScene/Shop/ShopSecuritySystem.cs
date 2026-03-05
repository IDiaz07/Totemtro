using UnityEngine;
using System.Security.Cryptography;
using System.Text;

public static class ShopSecuritySystem
{
    static string secretKey = "TotemTro_InternalKey";

    public static string GenerateHash(string data)
    {
        string combined = data + secretKey;

        using (SHA256 sha = SHA256.Create())
        {
            byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(combined));
            return System.Convert.ToBase64String(bytes);
        }
    }
}