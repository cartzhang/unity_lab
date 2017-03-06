using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

public class MassExportBitmaps : EditorWindow
{

    private int size;
    private SubstanceArchive[] substances;

    [MenuItem("Window/SubstanceMassExport")]
    static void Init()
    {
        EditorWindow.GetWindow(typeof(MassExportBitmaps));
    }

    void OnGUI()
    {
        if (size == 0)
        {
            size = 1;
        }

        size = EditorGUILayout.IntField("Substances", size);

        if (substances == null)
        {
            substances = new SubstanceArchive[size];
        }

        if (substances.Length != size)
        {
            SubstanceArchive[] cache = substances;

            substances = new SubstanceArchive[size];

            for (int i = 0; i < (cache.Length > size ? size : cache.Length); i++)
            {
                substances[i] = cache[i];
            }
        }

        for (int i = 0; i < size; i++)
        {
            substances[i] = EditorGUILayout.ObjectField("Substance " + (i + 1), substances[i], typeof(SubstanceArchive), false) as SubstanceArchive;
        }

        if (GUILayout.Button("Extract"))
        {
            for (int subs = 0; subs < size; subs++)
            {
                SubstanceArchive substance = substances[subs];

                if (substance == null)
                {
                    continue;
                }

                string substancePath = AssetDatabase.GetAssetPath(substance.GetInstanceID());
                SubstanceImporter substanceImporter = AssetImporter.GetAtPath(substancePath) as SubstanceImporter;
                int substanceMaterialCount = substanceImporter.GetMaterialCount();
                ProceduralMaterial[] substanceMaterials = substanceImporter.GetMaterials();

                if (substanceMaterialCount <= 0)
                {
                    continue;
                }

                string basePath = substancePath.Replace("/" + substance.name + ".sbsar", "");

                if (!Directory.Exists(basePath + "/" + substance.name))
                {
                    AssetDatabase.CreateFolder(basePath, substance.name);

                    AssetDatabase.ImportAsset(basePath + "/" + substance.name);
                }

                if (!Directory.Exists("EXPORT_HERE"))
                {
                    Directory.CreateDirectory("EXPORT_HERE");
                }

                System.Type substanceImporterType = typeof(SubstanceImporter);
                MethodInfo exportBitmaps = substanceImporterType.GetMethod("ExportBitmaps", BindingFlags.Instance | BindingFlags.Public);
                if (null == exportBitmaps)
                {
                    return;
                }

                foreach (ProceduralMaterial substanceMaterial in substanceMaterials)
                {
                    substanceMaterial.isReadable = true;//@zpj
                    bool generateAllOutputs = substanceImporter.GetGenerateAllOutputs(substanceMaterial);

                    if (!Directory.Exists(basePath + "/" + substance.name + "/" + substanceMaterial.name))
                    {
                        AssetDatabase.CreateFolder(basePath + "/" + substance.name, substanceMaterial.name);

                        AssetDatabase.ImportAsset(basePath + "/" + substance.name + "/" + substanceMaterial.name);
                    }

                    string materialPath = basePath + "/" + substance.name + "/" + substanceMaterial.name + "/";
                    Material newMaterial = new Material(substanceMaterial.shader);

                    newMaterial.CopyPropertiesFromMaterial(substanceMaterial);

                    AssetDatabase.CreateAsset(newMaterial, materialPath + substanceMaterial.name + ".mat");

                    AssetDatabase.ImportAsset(materialPath + substanceMaterial.name + ".mat");

                    substanceImporter.SetGenerateAllOutputs(substanceMaterial, true);

                    exportBitmaps.Invoke(substanceImporter, new object[] { substanceMaterial, @"EXPORT_HERE", false });

                    if (!generateAllOutputs)
                    {
                        substanceImporter.SetGenerateAllOutputs(substanceMaterial, false);
                    }

                    string[] exportedTextures = Directory.GetFiles("EXPORT_HERE");

                    if (exportedTextures.Length > 0)
                    {
                        string TmpfilePath = string.Empty;
                        foreach (string exportedTexture in exportedTextures)
                        {
                            TmpfilePath = materialPath + exportedTexture.Replace("EXPORT_HERE", "");
                            if (File.Exists(TmpfilePath))
                            {
                                File.Delete(TmpfilePath);
                                //Debug.Log(TmpfilePath);
                            }
                            File.Move(exportedTexture, TmpfilePath);
                        }
                    }

                    AssetDatabase.Refresh();

                    int propertyCount = ShaderUtil.GetPropertyCount(newMaterial.shader);
                    Texture[] materialTextures = substanceMaterial.GetGeneratedTextures();

                    if ((materialTextures.Length <= 0) || (propertyCount <= 0))
                    {
                        continue;
                    }

                    Texture newTmpTexture = new Texture();
                    foreach (ProceduralTexture materialTexture in materialTextures)
                    {
                        string newTexturePath = materialPath + materialTexture.name + ".tga";// (Clone)
                        string astmpe = Application.dataPath + newTexturePath.Substring(6);
                        if (!File.Exists(astmpe))
                        {
                            newTexturePath = materialPath + materialTexture.name + " (Clone).tga";
                            astmpe = Application.dataPath + newTexturePath.Substring(6);
                            if (!File.Exists(astmpe))
                            {
                                Debug.LogError(newTexturePath + "not exist");
                            }
                        }
                        Texture newTextureAsset = (Texture)AssetDatabase.LoadAssetAtPath(newTexturePath, typeof(Texture));
                        if (null != newTextureAsset)
                        {
                            try
                            {

                                for (int i = 0; i < propertyCount; i++)
                                {
                                    if (ShaderUtil.GetPropertyType(newMaterial.shader, i) == ShaderUtil.ShaderPropertyType.TexEnv)
                                    {
                                        string propertyName = ShaderUtil.GetPropertyName(newMaterial.shader, i);
                                        newTmpTexture = newMaterial.GetTexture(propertyName);
                                        //Debug.Log(newTmpTexture.name + " and  " + propertyName + " new assset " + newTextureAsset.name);
                                        if (null != newTmpTexture && (newTmpTexture.name == newTextureAsset.name || newTmpTexture.name + " (Clone)"  == newTextureAsset.name) )
                                        {
                                            newMaterial.SetTexture(propertyName, newTextureAsset);
                                        }
                                    }
                                }
                            }
                            catch (System.Exception ex)
                            {
                                Debug.Log(ex.Message);
                            }
                        }

                        ProceduralOutputType outType = materialTexture.GetProceduralOutputType();
                        if (materialTexture.GetProceduralOutputType() == ProceduralOutputType.Normal)
                        {
                            TextureImporter textureImporter = AssetImporter.GetAtPath(newTexturePath) as TextureImporter;
                            if (null != textureImporter)
                            {
                                textureImporter.textureType = TextureImporterType.Bump;
                            }
                            AssetDatabase.ImportAsset(newTexturePath);
                        }
                    }
                }

                if (Directory.Exists("EXPORT_HERE"))
                {
                    Directory.Delete("EXPORT_HERE");
                }
            }
        }
    }
}
