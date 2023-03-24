using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;

public struct BreakPower
{
    public int noBreakPower;//霸体
    public int maxBreakPower;

    internal void SetFull()
    {
        noBreakPower = maxBreakPower;
    }

    public void SetMaxValue(int max)
    {
        maxBreakPower = max;
        SetFull();
    }
}

[System.Serializable]
public struct PlayerData
{
    public string playerName;
    public int hp;
    public int mp;
    public int lv;
    public int exp;
    public int MaxHp;
    public float BaseAck => lv * 20 +20;

    public void SetFull()
    {
        hp = MaxHp;
    }
    public void MaxAndDull(int maxHp)
    {
        MaxHp = maxHp;
        SetFull();
    }

    public static PlayerData GetData()
    {
        return IOHelper.GetData<PlayerData>(DataPath.playerDataJson);
    }
    public static void SetData(PlayerData playerData)
    {
        IOHelper.SetData(DataPath.playerDataJson,playerData);
    }

    public override string ToString()
    {
        return string.Format("hp:{0} mp{1} lv{2} maxHp{3}", hp, mp, lv, MaxHp);
    }
}


public static class IOHelper
{
    public static bool IsFileExists(string fileName)
    {
        return File.Exists(fileName);
    }

    public static bool IsDirectoryExists(string fileName)
    {
        return Directory.Exists(fileName);
    }

    public static void CreateFile(string fileName, string content)

    {

        StreamWriter streamWriter = File.CreateText(fileName);

        streamWriter.Write(content);

        streamWriter.Close();

    }

    public static void CreateDirectory(string fileName)

    {

        //文件夹存在则返回

        if (IsDirectoryExists(fileName))

            return;

        Directory.CreateDirectory(fileName);

    }

    public static void SetData(string fileName, object pObject)
    {
        //将对象序列化为字符串
        string toSave = JsonUtility.ToJson(pObject);

        //对字符串进行加密,32位加密密钥

        //toSave = RijndaelEncrypt(toSave, "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx");

        StreamWriter streamWriter = File.CreateText(fileName);

        streamWriter.Write(toSave);

        streamWriter.Close();
    }

    public static T GetData<T>(string fileName) where T:new() 
    {
        if (!IsFileExists(fileName))
        {
            return new T();
        }

        StreamReader streamReader = File.OpenText(fileName);
        string data = streamReader.ReadToEnd();
        //对数据进行解密，32位解密密钥
        //data = RijndaelDecrypt(data, "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx");
        streamReader.Close();

        return JsonUtility.FromJson<T>(data);
    }


    #region 弃用
    private static string RijndaelEncrypt(string pString, string pKey)
    {

        //密钥

        byte[] keyArray = UTF8Encoding.UTF8.GetBytes(pKey);

        //待加密明文数组

        byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(pString);

        //Rijndael解密算法

        RijndaelManaged rDel = new RijndaelManaged();

        rDel.Key = keyArray;

        rDel.Mode = CipherMode.ECB;

        rDel.Padding = PaddingMode.PKCS7;

        ICryptoTransform cTransform = rDel.CreateEncryptor();

        //返回加密后的密文

        byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

        return Convert.ToBase64String(resultArray, 0, resultArray.Length);

    }

    private static String RijndaelDecrypt(string pString, string pKey)

    {

        //解密密钥

        byte[] keyArray = UTF8Encoding.UTF8.GetBytes(pKey);

        //待解密密文数组

        byte[] toEncryptArray = Convert.FromBase64String(pString);

        //Rijndael解密算法

        RijndaelManaged rDel = new RijndaelManaged();

        rDel.Key = keyArray;

        rDel.Mode = CipherMode.ECB;

        rDel.Padding = PaddingMode.PKCS7;

        ICryptoTransform cTransform = rDel.CreateDecryptor();

        //返回解密后的明文

        byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

        return UTF8Encoding.UTF8.GetString(resultArray);

    }
    #endregion


}

