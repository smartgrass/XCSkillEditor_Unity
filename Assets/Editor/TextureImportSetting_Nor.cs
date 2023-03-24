

using UnityEngine;
using System.Collections;
using UnityEditor;

/// <summary>
/// AssetPostprocessor： 贴图、模型、声音等资源导入时调用，可自动设置相应参数
/// 导入图片时自动设置图片的参数
/// </summary>
public class TextureImportSetting : AssetPostprocessor
{

    /// <summary>
    /// 图片导入之前调用，可设置图片的格式、Tag……
    /// </summary>
    void OnPreprocessTexture()
    {
        if (this.assetPath.Contains("Assets/Resources/Sprite"))
        {
            Debug.Log("OnPreProcessTexture=" + this.assetPath);

            TextureImporter importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite; // 设置为Sprite类型
            importer.mipmapEnabled = false; // 禁用mipmap
            importer.SaveAndReimport();
            //importer.spritePackingTag = "tag"; // 设置Sprite的打包Tag
        }


        //Debug.Log("OnPreprocessTexture");
    }
}