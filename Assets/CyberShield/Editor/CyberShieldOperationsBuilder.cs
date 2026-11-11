using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace CyberShield.Editor
{
    public static class CyberShieldOperationsBuilder
    {
        public const string Path="Assets/CyberShield/Scenes/CyberShieldVR_Operations.unity";
        static readonly Color Cyan=new Color(0.12f,0.88f,1f);
        static readonly Color Amber=new Color(1f,0.64f,0.2f);
        static Transform root;
        static Material graphite,steel,cyan,amber,black,white;
        static int meshId;

        [MenuItem("CyberShield/Crear Protocolo Blackout")]
        public static string Build()
        {
            var scene=SceneManager.GetActiveScene();
            if(scene.path!=CyberShieldBuilder.ScenePath || scene.isDirty)
                throw new InvalidOperationException("Abre la escena base guardada antes de crear Operations.");
            if(AssetDatabase.LoadAssetAtPath<SceneAsset>(Path)) throw new InvalidOperationException("Operations ya existe; no se sobrescribirá.");
            EditorSceneManager.SaveScene(scene,Path,true);
            scene=EditorSceneManager.OpenScene(Path,OpenSceneMode.Single);
            System.IO.Directory.CreateDirectory("Assets/CyberShield/Materials/Operations");
            System.IO.Directory.CreateDirectory("Assets/CyberShield/Meshes");
            root=new GameObject("BLACKOUT · Arquitectura y señalética").transform;
            graphite=Mat("Titanio",new Color(0.025f,0.045f,0.065f));
            steel=Mat("Acero",new Color(0.09f,0.14f,0.18f));
            black=Mat("Paneles",new Color(0.009f,0.018f,0.03f));
            cyan=Mat("Circuito cian",Cyan,true);
            amber=Mat("Seguridad ámbar",Amber,true);
            white=Mat("Luminarias",new Color(0.6f,0.85f,1f),true);
            var game=UnityEngine.Object.FindFirstObjectByType<CyberShieldGame>();
            game.requireBriefing=true;
            var atmosphere=game.gameObject.AddComponent<CyberShieldAtmosphere>();
            atmosphere.game=game;
            atmosphere.ambience=game.gameObject.AddComponent<AudioSource>();
            atmosphere.ambience.playOnAwake=false;
            foreach(var r in GameObject.Find("Sala informática").GetComponentsInChildren<Renderer>())
                r.sharedMaterial=r.name.StartsWith("Estado")?cyan:r.name.StartsWith("Pared")?steel:graphite;
            foreach(var t in GameObject.Find("Sala informática").GetComponentsInChildren<Transform>().Where(t=>t.name.StartsWith("Pared")))
            {
                var p=t.position;p.y=2.2f;t.position=p;
                var s=t.localScale;s.y=4.4f;t.localScale=s;
            }
            RenderSettings.ambientLight=new Color(0.16f,0.23f,0.32f);
            var sun=GameObject.Find("Luz principal").GetComponent<Light>();
            sun.intensity=0.8f;sun.color=new Color(0.72f,0.86f,1f);sun.shadows=LightShadows.None;
            Camera.main.clearFlags=CameraClearFlags.SolidColor;
            Camera.main.backgroundColor=new Color(0.008f,0.015f,0.025f);
            // Closed architectural shell and a low-cost emissive lighting design.
            Box("Techo",new Vector3(0,4.42f,0),new Vector3(10,0.15f,10),graphite,true);
            for(int i=-4;i<=4;i++)
            {
                Box("Junta suelo X",new Vector3(i,0.006f,0),new Vector3(0.012f,0.008f,9.6f),steel);
                Box("Junta suelo Z",new Vector3(0,0.006f,i),new Vector3(9.6f,0.008f,0.012f),steel);
            }
            foreach(float x in new[]{-4.7f,4.7f})
            {
                Box("Línea perimetral",new Vector3(x,0.02f,0),new Vector3(0.025f,0.02f,9.4f),cyan);
                Box("Cornisa",new Vector3(x,3.9f,0),new Vector3(0.05f,0.035f,9.4f),cyan);
                for(int z=-4;z<=4;z+=2)
                    Box("Costilla estructural",new Vector3(x,2.1f,z),new Vector3(0.16f,4.2f,0.18f),graphite);
            }
            for(int z=-3;z<=3;z+=3)
            {
                Box("Viga superior",new Vector3(0,4.15f,z),new Vector3(9.6f,0.22f,0.25f),steel);
                Box("Luz de techo",new Vector3(0,4.02f,z),new Vector3(5.8f,0.025f,0.09f),white);
            }
            // Two banks of server cabinets; no expensive realtime point lights.
            for(int side=-1;side<=1;side+=2)
            for(int n=0;n<3;n++)
            {
                float x=side*4.15f,z=-2.8f+n*1.65f;
                Box("Rack "+side+" / "+n,new Vector3(x,1.25f,z),new Vector3(0.9f,2.5f,1.15f),graphite,true);
                for(int bay=0;bay<7;bay++)
                {
                    float y=0.35f+bay*0.29f;
                    Box("Bandeja servidor",new Vector3(x,y,z-0.59f),new Vector3(0.72f,0.23f,0.04f),black);
                    Box("LED servidor",new Vector3(x-0.27f,y,z-0.62f),new Vector3(0.025f,0.025f,0.02f),bay%3==0?amber:cyan);
                    Box("Ventilación",new Vector3(x+0.08f,y,z-0.62f),new Vector3(0.38f,0.015f,0.02f),steel);
                }
                Label("RACK / "+(side<0?"A":"B")+(n+1),new Vector3(x,2.32f,z-0.63f),new Vector2(0.7f,0.2f),1.4f,Cyan);
            }
            Box("Puerta de acceso",new Vector3(0,1.45f,-4.74f),new Vector3(1.7f,2.9f,0.12f),black,true);
            Box("Marco puerta izquierdo",new Vector3(-0.9f,1.5f,-4.62f),new Vector3(0.04f,3f,0.05f),cyan);
            Box("Marco puerta derecho",new Vector3(0.9f,1.5f,-4.62f),new Vector3(0.04f,3f,0.05f),cyan);
            var exit=Label("SECTOR 03 / ACCESO SEGURO",new Vector3(0,3.2f,-4.55f),new Vector2(3,0.35f),2.5f,Cyan);
            exit.transform.rotation=Quaternion.Euler(0,180,0);
            Label("C Y B E R S H I E L D",new Vector3(0,4.05f,4.72f),new Vector2(6,0.4f),4.5f,Color.white);
            Label("S E C U R I T Y   O P E R A T I O N S   /   N O D O   0 3",new Vector3(0,3.7f,4.72f),new Vector2(7,0.3f),1.8f,Cyan);
            for(int i=0;i<3;i++)
            {
                float x=(i-1)*2.5f;
                Box("Borde consola",new Vector3(x,0.8f,2.2f),new Vector3(2.2f,0.035f,0.03f),i==1?amber:cyan);
                Box("Teclado",new Vector3(x,0.81f,2.28f),new Vector3(0.68f,0.035f,0.25f),black);
                for(int k=0;k<5;k++) Box("Fila teclas",new Vector3(x,0.834f,2.19f+k*0.04f),new Vector3(0.58f,0.008f,0.012f),steel);
                Label("0"+(i+1),new Vector3(x-0.85f,0.5f,2.17f),new Vector2(0.6f,0.4f),2.5f,Cyan);
                for(int k=0;k<3;k++) Box("Guía estación",new Vector3(x,0.012f,0.55f+k*0.35f),new Vector3(0.18f,0.014f,0.12f),i==1?amber:cyan);
                var panel=GameObject.Find(new[]{"01 / DETECTAR","02 / AISLAR","03 / RESTAURAR"}[i]);
                panel.GetComponent<UnityEngine.UI.Image>().color=new Color(0.012f,0.027f,0.045f,0.98f);
                panel.transform.Find("Título").GetComponent<TMP_Text>().text=new[]{"01  /  ANÁLISIS DE CORREO","02  /  CONTENCIÓN DE RED","03  /  BÓVEDA DE RESPALDOS"}[i];
                panel.transform.Find("Título").GetComponent<TMP_Text>().fontSize=32;
                for(int j=0;j<3;j++)
                {
                    var b=game.choices[i*3+j];
                    b.GetComponent<UnityEngine.UI.Image>().color=new Color(0.07f,0.18f,0.24f);
                    var colors=b.colors;colors.disabledColor=new Color(0.33f,0.4f,0.46f);colors.pressedColor=new Color(0.2f,1f,0.85f);b.colors=colors;
                    b.GetComponentInChildren<TMP_Text>().fontSize=29;
                    var stripe=new GameObject("Acento",typeof(RectTransform),typeof(UnityEngine.UI.Image));
                    stripe.transform.SetParent(b.transform,false);
                    var rt=stripe.GetComponent<RectTransform>();rt.anchorMin=new Vector2(0,0);rt.anchorMax=new Vector2(0,1);rt.pivot=new Vector2(0,0.5f);rt.sizeDelta=new Vector2(5,0);rt.anchoredPosition=Vector2.zero;
                    stripe.GetComponent<UnityEngine.UI.Image>().color=i==1?Amber:Cyan;stripe.GetComponent<UnityEngine.UI.Image>().raycastTarget=false;
                }
            }
            var board=GameObject.Find("Estado de misión");
            board.GetComponent<UnityEngine.UI.Image>().color=new Color(0.012f,0.027f,0.045f);
            // Side telemetry panels enrich the incident while the central board stays readable.
            var network=Panel("Telemetría de incidente",new Vector3(-2.65f,2.85f,3.7f),new Vector2(850,310),0.002f);
            atmosphere.networkStatus=Text(network,"PC-02 / COMPROMETIDO\nTRÁFICO SOSPECHOSO DETECTADO",32,100,Amber);
            Text(network,"10:00:04  ADJUNTO EJECUTADO\n10:00:07  CONEXIÓN EXTERNA\n10:00:12  ALERTA DE INTEGRIDAD",25,130,new Color(0.58f,0.75f,0.82f));
            var vault=Panel("Estado del respaldo",new Vector3(2.65f,2.85f,3.7f),new Vector2(850,310),0.002f);
            atmosphere.restoreStatus=Text(vault,"REPOSITORIO OFFLINE · EN ESPERA",29,90,Cyan);
            Text(vault,"ÚLTIMA COPIA SANA / AYER 18:00\nHASH VERIFICADO · ALMACENAMIENTO AISLADO",25,90,new Color(0.58f,0.75f,0.82f));
            var bar=new GameObject("Progreso",typeof(RectTransform),typeof(UnityEngine.UI.Image),typeof(UnityEngine.UI.LayoutElement));
            bar.transform.SetParent(vault,false);bar.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight=18;bar.GetComponent<UnityEngine.UI.Image>().color=new Color(0.07f,0.15f,0.2f);
            var fill=new GameObject("Restauración",typeof(RectTransform),typeof(UnityEngine.UI.Image));fill.transform.SetParent(bar.transform,false);
            var fr=fill.GetComponent<RectTransform>();fr.anchorMin=Vector2.zero;fr.anchorMax=Vector2.one;fr.sizeDelta=Vector2.zero;
            var fillImage=fill.GetComponent<UnityEngine.UI.Image>();fillImage.color=Cyan;fillImage.raycastTarget=false;
            fillImage.sprite=AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");fillImage.type=UnityEngine.UI.Image.Type.Filled;fillImage.fillMethod=UnityEngine.UI.Image.FillMethod.Horizontal;fillImage.fillAmount=0;atmosphere.progressFill=fillImage;
            var status=Panel("Cronómetro de operación",new Vector3(0,0.45f,2.1f),new Vector2(1500,180),0.0019f);
            atmosphere.telemetry=Text(status,"OPERACIÓN BLACKOUT / ESPERANDO OPERADOR",32,130,Cyan);
            // Holographic topology at the back, above the workstations.
            var holo=new GameObject("Núcleo holográfico").transform;holo.SetParent(root);holo.position=new Vector3(0,2.95f,4.15f);
            atmosphere.hologram=holo;
            for(int ring=0;ring<3;ring++)
            {
                var go=new GameObject("Órbita "+ring);go.transform.SetParent(holo,false);
                go.transform.localRotation=Quaternion.Euler(ring==0?0:70,0,ring*60);
                var line=go.AddComponent<LineRenderer>();line.sharedMaterial=cyan;line.useWorldSpace=false;line.loop=true;line.widthMultiplier=0.008f;line.positionCount=64;
                for(int j=0;j<64;j++){float a=j*Mathf.PI*2/64;line.SetPosition(j,new Vector3(Mathf.Cos(a),Mathf.Sin(a),0)*0.35f);}
            }
            atmosphere.nodes=new Renderer[3];atmosphere.links=new LineRenderer[3];
            for(int i=0;i<3;i++)
            {
                var node=GameObject.CreatePrimitive(PrimitiveType.Sphere);node.name="Nodo de red PC-0"+(i+1);node.transform.SetParent(root);node.transform.position=new Vector3((i-1)*2.5f,2.48f,2.85f);node.transform.localScale=Vector3.one*0.12f;
                UnityEngine.Object.DestroyImmediate(node.GetComponent<Collider>());node.GetComponent<Renderer>().sharedMaterial=cyan;atmosphere.nodes[i]=node.GetComponent<Renderer>();
                var line=new GameObject("Enlace de datos "+i).AddComponent<LineRenderer>();line.transform.SetParent(root);line.sharedMaterial=Mat("Enlace "+i,Color.white,true);line.positionCount=3;line.widthMultiplier=0.012f;
                line.SetPositions(new[]{node.transform.position,new Vector3((i-1)*2.5f,2.65f,4.2f),holo.position});atmosphere.links[i]=line;
            }
            var briefing=Panel("Briefing de misión",new Vector3(0,1.65f,1.05f),new Vector2(1150,900),0.0017f);
            Text(briefing,"C Y B E R S H I E L D   /   V R",34,70,Cyan);
            Text(briefing,"PROTOCOLO\nBLACKOUT",78,200,Color.white);
            Text(briefing,"10:00 AM. Un adjunto malicioso acaba de comprometer PC-02. Eres el operador de respuesta del Nodo 03.",33,145,new Color(0.75f,0.86f,0.92f));
            Text(briefing,"01  DETECTA la suplantación\n02  AÍSLA el equipo afectado\n03  RECUPERA una copia limpia",32,170,Cyan);
            Text(briefing,"Apunta y pulsa el gatillo. Teletranspórtate para acercarte.\nSin límite de tiempo. Cada error descuenta 10 puntos.",25,95,Color.white);
            var start=Button(briefing,"INICIAR OPERACIÓN  →",90);
            UnityEventTools.AddPersistentListener(start.onClick,game.BeginMission);
            atmosphere.briefing=briefing.gameObject;
            briefing.GetComponent<Canvas>().overrideSorting=true;briefing.GetComponent<Canvas>().sortingOrder=20;
            var report=Panel("Informe de misión",new Vector3(0,1.55f,1.05f),new Vector2(1200,1020),0.0015f);
            Text(report,"MISIÓN COMPLETADA / BLACKOUT",42,100,Cyan);
            atmosphere.result=Text(report,"EVALUACIÓN DEL OPERADOR",33,650,Color.white);
            var retry=Button(report,"VOLVER A ENTRENAR",90);UnityEventTools.AddPersistentListener(retry.onClick,game.Restart);
            atmosphere.debrief=report.gameObject;report.gameObject.SetActive(false);
            report.GetComponent<Canvas>().overrideSorting=true;report.GetComponent<Canvas>().sortingOrder=21;
            var audio=Panel("Control de audio",new Vector3(-1.3f,0.5f,1.8f),new Vector2(600,140),0.0015f);
            atmosphere.muteButton=Button(audio,"AUDIO: ACTIVADO",100);
            atmosphere.muteLabel=atmosphere.muteButton.GetComponentInChildren<TMP_Text>();
            // Batch fixed decorative cubes by material; keep interactive objects independent.
            CombineDecorativeMeshes();
            board.transform.position=new Vector3(0,2.95f,2.7f);
            board.transform.localScale=Vector3.one*0.002f;
            network.position=new Vector3(-2.65f,3.05f,2.77f);
            vault.position=new Vector3(2.65f,3.05f,2.77f);
            audio.position=new Vector3(-2.5f,0.48f,2.05f);
            foreach(var t in root.GetComponentsInChildren<TextMeshPro>())
            {
                t.enableAutoSizing=true;t.fontSizeMin=0.5f;t.fontSizeMax=t.fontSize;
                if(t.text=="C Y B E R S H I E L D")
                {t.transform.position=new Vector3(-4.75f,3.25f,-0.4f);t.transform.rotation=Quaternion.Euler(0,-90,0);t.rectTransform.sizeDelta=new Vector2(5f,0.6f);}
                if(t.text.StartsWith("S E C U R I T Y"))
                {t.transform.position=new Vector3(4.75f,3.25f,-0.4f);t.transform.rotation=Quaternion.Euler(0,90,0);t.rectTransform.sizeDelta=new Vector2(6f,0.5f);}
            }
            foreach(var canvas in UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include,FindObjectsSortMode.None))
            {
                var img=canvas.GetComponent<UnityEngine.UI.Image>();
                if(img && canvas.gameObject!=atmosphere.briefing && canvas.gameObject!=atmosphere.debrief) img.raycastTarget=false;
            }
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            var scenes=EditorBuildSettings.scenes.ToList();scenes.Add(new EditorBuildSettingsScene(Path,true));EditorBuildSettings.scenes=scenes.ToArray();
            AssetDatabase.SaveAssets();
            return "Operations creada: "+Path;
        }

        static Material Mat(string name,Color color,bool unlit=false)
        {
            var mat=new Material(Shader.Find(unlit?"Universal Render Pipeline/Unlit":"Universal Render Pipeline/Lit"));
            mat.name=name;mat.SetColor("_BaseColor",color);
            if(!unlit){mat.SetFloat("_Smoothness",0.32f);mat.SetFloat("_Metallic",0.35f);}
            AssetDatabase.CreateAsset(mat,"Assets/CyberShield/Materials/Operations/"+name+".mat");return mat;
        }
        static GameObject Box(string name,Vector3 pos,Vector3 scale,Material mat,bool collider=false)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(root);go.transform.position=pos;go.transform.localScale=scale;go.GetComponent<Renderer>().sharedMaterial=mat;
            if(!collider) UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());
            return go;
        }
        static TMP_Text Label(string value,Vector3 pos,Vector2 size,float fontSize,Color color)
        {
            var go=new GameObject("Señal / "+value,typeof(TextMeshPro));go.transform.SetParent(root);go.transform.position=pos;
            var t=go.GetComponent<TextMeshPro>();t.text=value;t.font=TMP_Settings.defaultFontAsset;t.fontSize=fontSize;t.color=color;t.alignment=TextAlignmentOptions.Center;t.rectTransform.sizeDelta=size;return t;
        }
        static Transform Panel(string name,Vector3 pos,Vector2 size,float scale)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Canvas),typeof(UnityEngine.UI.CanvasScaler),typeof(TrackedDeviceGraphicRaycaster),typeof(UnityEngine.UI.Image),typeof(UnityEngine.UI.VerticalLayoutGroup));
            go.GetComponent<Canvas>().renderMode=RenderMode.WorldSpace;go.GetComponent<Canvas>().worldCamera=Camera.main;
            var rt=go.GetComponent<RectTransform>();rt.sizeDelta=size;rt.position=pos;rt.localScale=Vector3.one*scale;
            go.GetComponent<UnityEngine.UI.Image>().color=new Color(0.012f,0.025f,0.04f,1f);go.GetComponent<UnityEngine.UI.Image>().raycastTarget=true;
            var layout=go.GetComponent<UnityEngine.UI.VerticalLayoutGroup>();layout.padding=new RectOffset(32,32,22,22);layout.spacing=12;layout.childControlHeight=true;layout.childControlWidth=true;layout.childForceExpandHeight=false;layout.childForceExpandWidth=true;
            return go.transform;
        }
        static TMP_Text Text(Transform parent,string value,int size,float height,Color color)
        {
            var go=new GameObject("Texto",typeof(RectTransform),typeof(TextMeshProUGUI),typeof(UnityEngine.UI.LayoutElement));go.transform.SetParent(parent,false);
            var t=go.GetComponent<TextMeshProUGUI>();t.text=value;t.font=TMP_Settings.defaultFontAsset;t.fontSize=size;t.color=color;t.raycastTarget=false;t.alignment=TextAlignmentOptions.MidlineLeft;
            go.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight=height;return t;
        }
        static UnityEngine.UI.Button Button(Transform parent,string value,float height)
        {
            var go=new GameObject(value,typeof(RectTransform),typeof(UnityEngine.UI.Image),typeof(UnityEngine.UI.Button),typeof(UnityEngine.UI.LayoutElement));go.transform.SetParent(parent,false);
            var img=go.GetComponent<UnityEngine.UI.Image>();img.color=new Color(0.05f,0.38f,0.46f);
            var b=go.GetComponent<UnityEngine.UI.Button>();b.targetGraphic=img;go.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight=height;
            var t=Text(go.transform,value,32,height,Color.white);t.alignment=TextAlignmentOptions.Center;
            t.rectTransform.anchorMin=Vector2.zero;t.rectTransform.anchorMax=Vector2.one;t.rectTransform.offsetMin=new Vector2(12,5);t.rectTransform.offsetMax=new Vector2(-12,-5);return b;
        }
        static void CombineDecorativeMeshes()
        {
            // Only generated non-colliding cubes, never text, lines, spheres or existing content.
            var cubes=root.GetComponentsInChildren<MeshFilter>().Where(m=>m.sharedMesh.name=="Cube" && !m.GetComponent<Collider>()).ToArray();
            foreach(var group in cubes.GroupBy(m=>m.GetComponent<Renderer>().sharedMaterial))
            {
                var mesh=new Mesh();mesh.name="Decoración "+group.Key.name;
                mesh.CombineMeshes(group.Select(m=>new CombineInstance{mesh=m.sharedMesh,transform=m.transform.localToWorldMatrix}).ToArray());
                AssetDatabase.CreateAsset(mesh,"Assets/CyberShield/Meshes/Operations_"+(meshId++)+".asset");
                var go=new GameObject(mesh.name,typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(root);go.GetComponent<MeshFilter>().sharedMesh=mesh;go.GetComponent<MeshRenderer>().sharedMaterial=group.Key;
                foreach(var original in group) original.gameObject.SetActive(false);
            }
        }
    }
}
