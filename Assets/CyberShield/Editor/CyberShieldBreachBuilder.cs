using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CyberShield.Editor
{
    public static class CyberShieldBreachBuilder
    {
        public const string Path="Assets/CyberShield/Scenes/CyberShieldVR_NightBreach.unity";
        const string Folder="Assets/CyberShield/Materials/NightBreach/";
        static Transform root,decor;
        static Material dark,steel,cyan,red,white;
        static readonly List<Renderer> strips=new List<Renderer>(),leds=new List<Renderer>();
        [MenuItem("CyberShield/Crear Night Breach")]
        public static string Build()
        {
            if(AssetDatabase.LoadAssetAtPath<SceneAsset>(Path))throw new Exception("Night Breach ya existe");
            var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if(!scene.path.EndsWith("CyberShieldVR_Cinema.unity"))throw new Exception("Abre Cinema primero");
            // Save a copy including any current edits, leaving the previous scene intact.
            EditorSceneManager.SaveScene(scene,Path,true);scene=EditorSceneManager.OpenScene(Path);
            System.IO.Directory.CreateDirectory(Folder);strips.Clear();leds.Clear();
            root=new GameObject("NEXORA / NIGHT BREACH").transform;decor=new GameObject("Decoración nueva").transform;decor.SetParent(root);
            dark=Mat("Grafito",new Color(.035f,.055f,.075f),false,.25f);steel=Mat("Metal",new Color(.16f,.22f,.28f),false,.7f);
            cyan=Mat("LED cian",new Color(.03f,.7f,1),true);red=Mat("LED alerta",new Color(1,.13f,.05f),true);white=Mat("Luz oficina",new Color(.55f,.8f,1),true);
            var night=NightSkyline();var remap=new Dictionary<Material,Material>();
            foreach(var renderer in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(renderer.transform.IsChildOf(root))continue;
                if(renderer.GetComponent<TMPro.TMP_SubMesh>() || renderer.GetComponent<TMP_Text>())continue;
                var originals=renderer.sharedMaterials;bool changed=false;
                for(int i=0;i<originals.Length;i++)
                {
                    var original=originals[i];if(!original)continue;
                    string name=original.name;
                    if(!(name.Contains("Cielo ventanas")||name=="Pintura"||name=="Papel"||name=="Edificios"||name=="LED blanco"))continue;
                    if(!remap.TryGetValue(original,out var replacement))
                    {
                        replacement=new Material(original);replacement.name="Noche / "+name;
                        if(name.Contains("Cielo ventanas")){replacement.SetColor("_BaseColor",Color.white);replacement.SetTexture("_BaseMap",night);}
                        else replacement.SetColor("_BaseColor",name=="LED blanco"?new Color(.3f,.5f,.65f):name=="Edificios"?new Color(.025f,.045f,.08f):new Color(.18f,.23f,.29f));
                        AssetDatabase.CreateAsset(replacement,Folder+name.Replace("/","_")+".mat");remap.Add(original,replacement);
                    }
                    originals[i]=replacement;changed=true;
                }
                if(changed)renderer.sharedMaterials=originals;
            }
            for(int side=-1;side<=1;side+=2)
            {
                float x=side*4.08f;
                Cube("Rack / bastidor",new Vector3(x,1.23f,2.8f),new Vector3(.74f,2.46f,.8f),dark);
                for(int i=0;i<9;i++)
                {
                    float y=.25f+i*.245f;
                    Cube("Servidor",new Vector3(x,y,2.36f),new Vector3(.64f,.19f,.09f),steel);
                    for(int v=0;v<5;v++)Cube("Ventilación",new Vector3(x-.18f+v*.065f,y,2.305f),new Vector3(.032f,.1f,.008f),dark);
                    for(int n=0;n<2;n++)leds.Add(Cube("Estado servidor",new Vector3(x+.2f,y-.04f+n*.07f,2.3f),new Vector3(.035f,.018f,.012f),cyan,true).GetComponent<Renderer>());
                }
                Label("BACKUP / "+(side<0?"A":"B"),new Vector3(x,2.6f,2.33f),new Vector2(.8f,.16f),.65f,new Color(.4f,.8f,1));
                var strip=Cube("Iluminación de alerta",new Vector3(side*2.6f,3.95f,.1f),new Vector3(.06f,.035f,4.3f),red,true);strips.Add(strip.GetComponent<Renderer>());
                Cube("Canaleta técnica",new Vector3(side*3.1f,.05f,-1),new Vector3(.16f,.07f,6),dark);
            }
            // Detailed input hardware and dressed workstations, leaving the central aisle clear.
            foreach(float x in new[]{-2.8f,2.8f})
            {
                Cube("Alfombrilla de trabajo",new Vector3(x,.802f,-2.75f),new Vector3(1,.008f,.4f),dark);
                for(int row=0;row<4;row++)for(int key=0;key<13;key++)Cube("Tecla",new Vector3(x-.25f+key*.038f,.834f,-2.74f+row*.038f),new Vector3(.031f,.012f,.029f),steel);
                Cable(new[]{new Vector3(x,.82f,-2.8f),new Vector3(x+.4f,.82f,-3.1f),new Vector3(x+.5f,.4f,-3.3f),new Vector3(x+.5f,.1f,-3.3f)},.012f);
                var screen=Cube("Pantalla secundaria en alerta",new Vector3(x+.65f,1.17f,-3.18f),new Vector3(.62f,.39f,.04f),dark);
                screen.transform.rotation=Quaternion.Euler(0,180,0);
                var alert=Label("!\nACCESO BLOQUEADO",new Vector3(x+.65f,1.17f,-3.145f),new Vector2(.56f,.32f),.8f,new Color(1,.22f,.08f));alert.transform.rotation=Quaternion.Euler(0,180,0);
                for(int note=0;note<3;note++)Cube("Documento de trabajo",new Vector3(x-.85f,.813f+note*.004f,-2.8f+note*.018f),new Vector3(.2f,.003f,.27f),steel);
            }
            // Real monitor housings and stands around the gameplay screens.
            for(int i=0;i<3;i++)
            {
                float x=(i-1)*2.5f;
                foreach(float dx in new[]{-1.15f,1.15f})Cube("Marco monitor",new Vector3(x+dx,1.55f,2.7f),new Vector3(.045f,1.72f,.1f),dark);
                foreach(float y in new[]{.69f,2.41f})Cube("Marco monitor",new Vector3(x,y,2.7f),new Vector3(2.34f,.045f,.1f),dark);
                Cube("Pie de monitor",new Vector3(x,.82f,2.75f),new Vector3(.2f,.22f,.18f),steel);
                Cable(new[]{new Vector3(x,.8f,2.9f),new Vector3(x+.5f,.75f,2.95f),new Vector3(x+.5f,.2f,3.1f)},.014f);
            }
            var game=UnityEngine.Object.FindFirstObjectByType<CyberShieldGame>();var effect=game.gameObject.AddComponent<CyberShieldBreachAtmosphere>();effect.game=game;
            effect.warningStrips=strips.ToArray();effect.serverLeds=leds.ToArray();
            effect.incidentDisplay=Label("INTRUSIÓN DETECTADA",new Vector3(0,3.52f,3.25f),new Vector2(4,.5f),1.2f,Color.white);
            effect.compromisedScreens=UnityEngine.Object.FindObjectsByType<TMP_Text>(FindObjectsSortMode.None).Where(t=>t.text.Contains("ACCESO BLOQUEADO")).Select(t=>t.gameObject).ToArray();
            var lights=new List<Light>();foreach(float x in new[]{-3f,3f}){var g=new GameObject("Rebote de alerta",typeof(Light));g.transform.SetParent(root);g.transform.position=new Vector3(x,2.4f,.5f);var l=g.GetComponent<Light>();l.type=LightType.Point;l.range=4;l.intensity=.7f;l.shadows=LightShadows.None;lights.Add(l);}effect.accentLights=lights.ToArray();
            var audio=new GameObject("Señal de incidente",typeof(AudioSource));audio.transform.SetParent(root);audio.transform.position=new Vector3(-4,1.7f,2.6f);effect.alarm=audio.GetComponent<AudioSource>();effect.alarm.spatialBlend=1;effect.alarm.minDistance=1;effect.alarm.maxDistance=12;effect.alarm.rolloffMode=AudioRolloffMode.Linear;
            var main=GameObject.Find("Luz principal").GetComponent<Light>();main.intensity=.38f;main.color=new Color(.5f,.68f,1);RenderSettings.ambientLight=new Color(.16f,.21f,.3f);
            var arcade=game.GetComponent<CyberShieldArcade>();arcade.compactStatus.transform.position=new Vector3(0,3.12f,2.8f);arcade.objective.transform.position=new Vector3(0,2.85f,2.7f);arcade.threat.gameObject.SetActive(false);foreach(var ring in arcade.rings)ring.gameObject.SetActive(false);
            BatchDecor();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            if(!EditorBuildSettings.scenes.Any(s=>s.path==Path))EditorBuildSettings.scenes=EditorBuildSettings.scenes.Concat(new[]{new EditorBuildSettingsScene(Path,true)}).ToArray();return Path;
        }
        static Material Mat(string name,Color color,bool unlit,float metallic=0){var m=new Material(Shader.Find(unlit?"Universal Render Pipeline/Unlit":"Universal Render Pipeline/Lit"));m.SetColor("_BaseColor",color);if(!unlit){m.SetFloat("_Metallic",metallic);m.SetFloat("_Smoothness",.45f);}AssetDatabase.CreateAsset(m,Folder+name+".mat");return m;}
        static GameObject Cube(string name,Vector3 p,Vector3 scale,Material m,bool reactive=false){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(reactive?root:decor);g.transform.position=p;g.transform.localScale=scale;g.GetComponent<Renderer>().sharedMaterial=m;UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());return g;}
        static TMP_Text Label(string value,Vector3 p,Vector2 size,float font,Color color){var g=new GameObject("Señal / "+value,typeof(TextMeshPro));g.transform.SetParent(root);g.transform.position=p;var t=g.GetComponent<TextMeshPro>();t.font=TMP_Settings.defaultFontAsset;t.text=value;t.fontSize=font;t.color=color;t.alignment=TextAlignmentOptions.Center;t.rectTransform.sizeDelta=size;return t;}
        static void Cable(Vector3[] points,float width){var g=new GameObject("Cableado");g.transform.SetParent(root);var l=g.AddComponent<LineRenderer>();l.sharedMaterial=dark;l.positionCount=points.Length;l.widthMultiplier=width;l.SetPositions(points);l.numCornerVertices=3;}
        static Texture2D NightSkyline()
        {
            var t=new Texture2D(512,256,TextureFormat.RGB24,false);t.name="Ciudad nocturna";var pixels=new Color[512*256];var random=new System.Random(1042);
            for(int y=0;y<256;y++)for(int x=0;x<512;x++)pixels[y*512+x]=Color.Lerp(new Color(.015f,.025f,.06f),new Color(.04f,.09f,.16f),y/256f);
            for(int x=0;x<512;x+=25){int height=random.Next(60,200);for(int y=0;y<height;y++)for(int dx=0;dx<22&&x+dx<512;dx++){bool window=dx%7<3&&y%12<5&&random.NextDouble()>.25;pixels[y*512+x+dx]=window?new Color(.22f,.4f,.6f):new Color(.015f,.025f,.04f);}}
            t.SetPixels(pixels);t.Apply();AssetDatabase.CreateAsset(t,Folder+"Ciudad nocturna.asset");return t;
        }
        static void BatchDecor()
        {
            int index=0;foreach(var group in decor.GetComponentsInChildren<MeshFilter>().GroupBy(m=>m.GetComponent<Renderer>().sharedMaterial))
            {
                var mesh=new Mesh{indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};mesh.CombineMeshes(group.Select(m=>new CombineInstance{mesh=m.sharedMesh,transform=m.transform.localToWorldMatrix}).ToArray());AssetDatabase.CreateAsset(mesh,Folder+"Decor_"+index+++".asset");
                var g=new GameObject("Decoración combinada",typeof(MeshFilter),typeof(MeshRenderer));g.transform.SetParent(root);g.GetComponent<MeshFilter>().sharedMesh=mesh;g.GetComponent<Renderer>().sharedMaterial=group.Key;foreach(var original in group)original.GetComponent<Renderer>().enabled=false;
            }
        }
        public static string InstallFurniture()
        {
            if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().path!=Path)throw new Exception("Abre Night Breach");
            root=GameObject.Find("NEXORA / NIGHT BREACH").transform;
            if(root.Find("Mobiliario Kenney"))return "Mobiliario ya instalado";
            var props=new GameObject("Mobiliario Kenney").transform;props.SetParent(root);
            var converted=new Dictionary<Material,Material>();
            foreach(float x in new[]{-3.15f,3.15f})
            {
                var table=Prop("desk",new Vector3(x,0,-.7f),.75f,props,converted);
                Prop("chairDesk",new Vector3(x,0,-1.6f),1.05f,props,converted);
                Prop("computerScreen",new Vector3(x,.77f,-.95f),.46f,props,converted);
                Prop("computerKeyboard",new Vector3(x,.77f,-.5f),.025f,props,converted);
                Prop("computerMouse",new Vector3(x+.35f,.77f,-.5f),.04f,props,converted);
            }
            EditorSceneManager.MarkSceneDirty(root.gameObject.scene);EditorSceneManager.SaveScene(root.gameObject.scene);AssetDatabase.SaveAssets();return "2 estaciones Kenney instaladas";
        }
        static GameObject Prop(string name,Vector3 position,float height,Transform parent,Dictionary<Material,Material> converted)
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/CyberShield/ThirdParty/KenneyFurniture/"+name+".fbx");if(!prefab)throw new Exception("No importado: "+name);
            var g=(GameObject)PrefabUtility.InstantiatePrefab(prefab,parent);g.transform.position=Vector3.zero;
            var renderers=g.GetComponentsInChildren<Renderer>();var bounds=renderers[0].bounds;foreach(var renderer in renderers)bounds.Encapsulate(renderer.bounds);
            g.transform.localScale*=height/Mathf.Max(.001f,bounds.size.y);
            bounds=renderers[0].bounds;foreach(var renderer in renderers)bounds.Encapsulate(renderer.bounds);
            g.transform.position=position-new Vector3(bounds.center.x,bounds.min.y,bounds.center.z);
            foreach(var renderer in renderers)
            {
                var materials=renderer.sharedMaterials;
                for(int i=0;i<materials.Length;i++)
                {
                    var original=materials[i];if(!original)continue;
                    if(!converted.TryGetValue(original,out var replacement))
                    {
                        replacement=new Material(Shader.Find("Universal Render Pipeline/Lit"));replacement.name="Kenney URP / "+original.name;
                        replacement.SetColor("_BaseColor",original.HasProperty("_Color")?original.GetColor("_Color"):Color.white);
                        if(original.HasProperty("_MainTex"))replacement.SetTexture("_BaseMap",original.GetTexture("_MainTex"));replacement.SetFloat("_Smoothness",.32f);
                        string path=AssetDatabase.GenerateUniqueAssetPath(Folder+"Kenney_"+original.name.Replace("/","_")+".mat");AssetDatabase.CreateAsset(replacement,path);converted.Add(original,replacement);
                    }
                    materials[i]=replacement;
                }
                renderer.sharedMaterials=materials;
            }
            return g;
        }
    }
}
