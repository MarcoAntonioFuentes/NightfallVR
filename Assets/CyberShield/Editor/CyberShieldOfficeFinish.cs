using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CyberShield.Editor
{
    public static class CyberShieldOfficeFinish
    {
        public static string Apply()
        {
            if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().path!=CyberShieldCorporateBuilder.Path)throw new System.Exception("Wrong scene");
            const string folder="Assets/CyberShield/Materials/Nexora/";
            foreach(string kind in new[]{"Roble","Moqueta"})
            {
                var mat=AssetDatabase.LoadAssetAtPath<Material>(folder+kind+".mat");
                string path=folder+kind+"_Detail.asset";
                var tex=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if(!tex)
                {
                    tex=new Texture2D(256,256,TextureFormat.RGB24,true);tex.name=kind+" detalle";tex.wrapMode=TextureWrapMode.Repeat;
                    var pixels=new Color[256*256];
                    for(int y=0;y<256;y++)for(int x=0;x<256;x++)
                    {
                        float noise=Mathf.PerlinNoise(x*0.09f,y*0.09f);
                        float grain=kind=="Roble"?0.78f+0.18f*Mathf.Sin(y*0.6f+Mathf.PerlinNoise(x*0.015f,y*0.03f)*8):0.8f+0.18f*noise;
                        if(kind=="Moqueta")grain+=((x+y)%2==0?0.025f:-0.025f);
                        pixels[y*256+x]=new Color(grain,grain,grain);
                    }
                    tex.SetPixels(pixels);tex.Apply();AssetDatabase.CreateAsset(tex,path);
                }
                mat.SetTexture("_BaseMap",tex);mat.SetTextureScale("_BaseMap",kind=="Roble"?new Vector2(2,1):new Vector2(14,14));EditorUtility.SetDirty(mat);
            }
            var root=GameObject.Find("NEXORA / Oficina corporativa");
            string lightingPath=folder+"LED blanco.mat";
            var light=AssetDatabase.LoadAssetAtPath<Material>(lightingPath);
            if(!light){light=new Material(Shader.Find("Universal Render Pipeline/Unlit"));light.SetColor("_BaseColor",new Color(0.94f,0.96f,1));AssetDatabase.CreateAsset(light,lightingPath);}
            foreach(var r in root.GetComponentsInChildren<Renderer>())if(r.name=="Panel LED")r.sharedMaterial=light;
            int reduced=0;
            if(!root.transform.Find("Decoración agrupada"))
            {
                var batched=new GameObject("Decoración agrupada");batched.transform.SetParent(root.transform);
                System.IO.Directory.CreateDirectory("Assets/CyberShield/Meshes/Nexora");
                var meshes=root.GetComponentsInChildren<MeshFilter>().Where(m=>m.sharedMesh && new[]{"Cube","Sphere","Cylinder"}.Contains(m.sharedMesh.name)).ToArray();
                int index=0;
                foreach(var group in meshes.GroupBy(m=>m.GetComponent<Renderer>().sharedMaterial))
                {
                    var mesh=new Mesh();mesh.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;
                    mesh.CombineMeshes(group.Select(m=>new CombineInstance{mesh=m.sharedMesh,transform=m.transform.localToWorldMatrix}).ToArray());
                    AssetDatabase.CreateAsset(mesh,"Assets/CyberShield/Meshes/Nexora/Group_"+index+++".asset");
                    var g=new GameObject("Material / "+group.Key.name,typeof(MeshFilter),typeof(MeshRenderer));g.transform.SetParent(batched.transform);g.GetComponent<MeshFilter>().sharedMesh=mesh;g.GetComponent<Renderer>().sharedMaterial=group.Key;
                    foreach(var original in group){original.GetComponent<Renderer>().enabled=false;reduced++;}
                }
            }
            EditorSceneManager.MarkSceneDirty(root.scene);EditorSceneManager.SaveScene(root.scene);AssetDatabase.SaveAssets();
            return "Texturas aplicadas; renderizadores decorativos agrupados: "+reduced;
        }
    }
}
