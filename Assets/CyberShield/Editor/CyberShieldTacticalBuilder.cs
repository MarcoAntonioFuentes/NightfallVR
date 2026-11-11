using System;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace CyberShield.Editor
{
    public static class CyberShieldTacticalBuilder
    {
        internal static Transform SharedPanel(Transform parent,string name,Vector3 position,Vector2 size,float scale)=>Panel(parent,name,position,size,scale);
        internal static TMP_Text SharedText(Transform parent,string value,Vector2 position,Vector2 size,float font,Color color)=>Text(parent,value,position,size,font,color);
        internal static Button SharedButton(Transform parent,string title,Vector2 position,Vector2 size)=>MakeButton(parent,title,position,size);
        static Material ink, metal, cyan;
        static Color blue=new Color(.02f,.75f,1);
        [MenuItem("CyberShield/Mejorar operaciones tácticas")]
        public static string Install()
        {
            var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if(!scene.path.EndsWith("CyberShieldVR_ShieldOps.unity"))throw new Exception("Abre Shield Ops primero.");
            if(UnityEngine.Object.FindFirstObjectByType<CyberShieldTacticalNetwork>())throw new Exception("La mejora táctica ya está instalada.");
            var flow=UnityEngine.Object.FindFirstObjectByType<CyberShieldReferenceExperience>();
            var root=new GameObject("CYBERSHIELD / TACTICAL OPERATIONS");
            var tactical=root.AddComponent<CyberShieldTacticalNetwork>();tactical.flow=flow;
            ink=Mat("Tactical grafito",new Color(.025f,.04f,.065f),false);metal=Mat("Tactical metal",new Color(.13f,.2f,.25f),false);cyan=Mat("Tactical luz",blue,true);
            var map=new GameObject("Mesa / red volumétrica").transform;map.SetParent(root.transform);tactical.map=map.gameObject;
            Cube(map,"Superficie",new Vector3(0,1.01f,1.65f),new Vector3(2.65f,.035f,1.7f),ink);
            for(int i=0;i<14;i++){float x=-1.3f+i*.2f;Cube(map,"Retícula",new Vector3(x,1.035f,1.65f),new Vector3(.004f,.004f,1.65f),cyan);}
            for(int i=0;i<9;i++)Cube(map,"Retícula",new Vector3(0,1.035f,.85f+i*.2f),new Vector3(2.6f,.004f,.004f),cyan);
            var server=Cube(map,"Servidor central",new Vector3(0,1.28f,2.24f),new Vector3(.22f,.45f,.22f),metal).transform;tactical.server=server;
            for(int i=0;i<6;i++)Cube(map,"Servidor / LED",new Vector3(0,1.12f+i*.06f,2.12f),new Vector3(.16f,.014f,.006f),cyan);
            tactical.screens=new Renderer[8];tactical.pedestals=new Renderer[8];tactical.computers=new XRSimpleInteractable[8];tactical.connections=new LineRenderer[8];tactical.packets=new Renderer[8];tactical.selectionFrames=new Image[8];
            string[] ids={"PC-01","PC-03","PC-02","PC-04","PC-05","PC-06","PC-07","PC-08"};
            for(int i=0;i<8;i++)
            {
                int slot=i==2?4:i==4?2:i;
                float x=-.96f+(slot%4)*.64f,z=slot<4?1.75f:1.12f;
                var pc=new GameObject(ids[i]).transform;pc.SetParent(map);pc.position=new Vector3(x,1.08f+(slot<4?.22f:0),z);
                tactical.pedestals[i]=Cube(pc,"Base holográfica",pc.position,new Vector3(.38f,.025f,.26f),cyan).GetComponent<Renderer>();
                Cube(pc,"Pantalla / marco",pc.position+new Vector3(0,.21f,0),new Vector3(.29f,.2f,.04f),metal);
                tactical.screens[i]=Cube(pc,"Pantalla / estado",pc.position+new Vector3(0,.21f,-.025f),new Vector3(.25f,.16f,.012f),cyan).GetComponent<Renderer>();
                Cube(pc,"Soporte",pc.position+new Vector3(0,.09f,0),new Vector3(.035f,.12f,.04f),metal);
                Cube(pc,"Teclado",pc.position+new Vector3(0,.035f,-.065f),new Vector3(.24f,.016f,.09f),metal);
                var collider=pc.gameObject.AddComponent<BoxCollider>();collider.center=new Vector3(0,.17f,0);collider.size=new Vector3(.38f,.34f,.25f);
                tactical.computers[i]=pc.gameObject.AddComponent<XRSimpleInteractable>();
                var label=Panel(map,ids[i]+" / seleccionar",pc.position+new Vector3(0,-.04f,-.16f),new Vector2(190,65),.0018f);
                var b=MakeButton(label,ids[i],Vector2.zero,new Vector2(190,65));UnityEventTools.AddIntPersistentListener(b.onClick,tactical.Select,i);tactical.selectionFrames[i]=b.GetComponent<Image>();
                var route=new GameObject("Conexión / "+ids[i],typeof(LineRenderer)).GetComponent<LineRenderer>();route.transform.SetParent(map);route.sharedMaterial=cyan;route.positionCount=3;route.startWidth=route.endWidth=.008f;route.SetPositions(new[]{server.position-new Vector3(0,.22f,0),new Vector3(x,1.05f,2.08f),pc.position});tactical.connections[i]=route;
                tactical.packets[i]=Cube(map,"Tráfico",pc.position,Vector3.one*.025f,cyan).GetComponent<Renderer>();
            }
            flow.volumetricNetwork=true;flow.computerFrames=tactical.selectionFrames;
            var detail=Panel(root.transform,"Actividad / equipo",new Vector3(1.98f,1.66f,1.75f),new Vector2(660,650),.0016f);tactical.details=detail.gameObject;
            tactical.heading=Text(detail,"INVESTIGA LA RED",new Vector2(0,250),new Vector2(620,65),35,blue);
            tactical.activity=Text(detail,"",new Vector2(0,65),new Vector2(600,295),27,Color.white);tactical.activity.alignment=TextAlignmentOptions.Left;
            flow.isolateButton=MakeButton(detail,"AISLAR",new Vector2(0,-150),new Vector2(590,85));flow.isolateButton.GetComponent<Image>().color=new Color(.7f,.12f,.025f);flow.isolationAction=flow.isolateButton.GetComponentInChildren<TMP_Text>();UnityEventTools.AddPersistentListener(flow.isolateButton.onClick,flow.Isolate);
            tactical.response=Text(detail,"",new Vector2(0,-255),new Vector2(610,100),24,new Color(1,.45f,.2f));
            var banner=Panel(root.transform,"Alerta / misión",new Vector3(0,2.65f,2.7f),new Vector2(1700,160),.0017f);
            tactical.banner=Text(banner,"CYBERSHIELD VR",Vector2.zero,new Vector2(1650,130),46,blue);
            flow.game.GetComponent<CyberShieldBreachAtmosphere>().incidentDisplay.gameObject.SetActive(false);
            flow.game.GetComponent<CyberShieldArcade>().compactStatus.gameObject.SetActive(false);
            flow.game.stationStatus[1].gameObject.SetActive(false);
            Decorate(root.transform);
            tactical.map.SetActive(false);tactical.details.SetActive(false);
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();return "Red 3D, ocho equipos investigables, alertas y decoración instaladas.";
        }
        static void Decorate(Transform root)
        {
            var pot=Mat("Tactical cerámica",new Color(.16f,.19f,.22f),false);var leaf=Mat("Tactical hojas",new Color(.07f,.3f,.16f),false);
            for(int side=-1;side<=1;side+=2)
            {
                for(int row=0;row<2;row++)
                {
                    var p=new Vector3(side*4.15f,.22f,-1.4f-row*2.5f);var g=GameObject.CreatePrimitive(PrimitiveType.Cylinder);g.name="Jardinera corporativa";g.transform.SetParent(root);g.transform.position=p;g.transform.localScale=new Vector3(.45f,.22f,.45f);g.GetComponent<Renderer>().sharedMaterial=pot;UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());
                    for(int l=0;l<9;l++){float a=l*2.399f;var v=GameObject.CreatePrimitive(PrimitiveType.Sphere);v.name="Follaje";v.transform.SetParent(root);v.transform.position=p+new Vector3(Mathf.Cos(a)*.19f,.35f+(l%3)*.17f,Mathf.Sin(a)*.19f);v.transform.localScale=new Vector3(.13f,.65f,.17f);v.transform.rotation=Quaternion.Euler(Mathf.Cos(a)*28,0,Mathf.Sin(a)*28);v.GetComponent<Renderer>().sharedMaterial=leaf;UnityEngine.Object.DestroyImmediate(v.GetComponent<Collider>());}
                }
                for(int z=0;z<4;z++)Cube(root,"Luminaria lineal",new Vector3(side*2.25f,3.9f,-3.6f+z*2),new Vector3(.055f,.03f,1.3f),cyan);
                var sign=Panel(root,"Cultura de seguridad",new Vector3(side*3.2f,2.1f,4.5f),new Vector2(800,1000),.0014f);
                Text(sign,"CYBERSHIELD",new Vector2(0,335),new Vector2(750,100),65,blue);
                Text(sign,side<0?"TU MENTE\nTAMBIÉN ES\nUN ESCUDO":"OBSERVA\nANALIZA\nPROTEGE",Vector2.zero,new Vector2(750,500),65,Color.white);
                Text(sign,"NEXORA  /  SECURITY OPERATIONS",new Vector2(0,-350),new Vector2(750,100),27,blue);
            }
        }
        public static string AddWorkstations()
        {
            var root=GameObject.Find("CYBERSHIELD / TACTICAL OPERATIONS").transform;
            if(root.Find("Puestos / ampliación"))return "Los puestos ya están instalados";
            var parent=new GameObject("Puestos / ampliación").transform;parent.SetParent(root);
            var originals=GameObject.Find("Mobiliario Kenney").transform;
            for(int i=0;i<originals.childCount;i++)
            {
                var source=originals.GetChild(i);var clone=UnityEngine.Object.Instantiate(source.gameObject,parent);clone.name=source.name+" / operaciones";clone.transform.position=source.position+new Vector3(0,0,-2.5f);
            }
            var porcelain=Mat("Tactical taza",new Color(.65f,.74f,.8f),false);
            var paper=Mat("Tactical documentos",new Color(.75f,.8f,.76f),false);
            var inkColor=Mat("Tactical tinta",new Color(.05f,.1f,.15f),false);
            for(int side=-1;side<=1;side+=2)
            {
                var p=new Vector3(side*3.05f,.78f,-3.2f);
                var mug=GameObject.CreatePrimitive(PrimitiveType.Cylinder);mug.name="Taza / personal";mug.transform.SetParent(parent);mug.transform.position=p+Vector3.up*.065f;mug.transform.localScale=new Vector3(.09f,.065f,.09f);mug.GetComponent<Renderer>().sharedMaterial=porcelain;UnityEngine.Object.DestroyImmediate(mug.GetComponent<Collider>());
                Cube(parent,"Agenda",p+new Vector3(.25f,.006f,0),new Vector3(.18f,.012f,.25f),paper);
                for(int line=0;line<6;line++)Cube(parent,"Notas de trabajo",p+new Vector3(.25f,.014f,-.08f+line*.027f),new Vector3(.13f,.002f,.003f),inkColor);
            }
            var flow=UnityEngine.Object.FindFirstObjectByType<CyberShieldReferenceExperience>();flow.game.stationStatus[1].gameObject.SetActive(false);
            EditorSceneManager.MarkSceneDirty(root.gameObject.scene);EditorSceneManager.SaveOpenScenes();AssetDatabase.SaveAssets();return "Dos puestos adicionales con agendas y tazas.";
        }
        static Material Mat(string name,Color color,bool unlit){string path="Assets/CyberShield/Materials/ShieldOps/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(m)return m;m=new Material(Shader.Find(unlit?"Universal Render Pipeline/Unlit":"Universal Render Pipeline/Lit"));m.color=color;AssetDatabase.CreateAsset(m,path);return m;}
        static GameObject Cube(Transform parent,string name,Vector3 p,Vector3 size,Material m){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(parent);g.transform.position=p;g.transform.localScale=size;g.GetComponent<Renderer>().sharedMaterial=m;UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());return g;}
        static Transform Panel(Transform parent,string name,Vector3 p,Vector2 size,float scale){var g=new GameObject(name,typeof(RectTransform),typeof(Canvas),typeof(GraphicRaycaster),typeof(TrackedDeviceGraphicRaycaster),typeof(Image));g.transform.SetParent(parent);g.transform.position=p;g.transform.localScale=Vector3.one*scale;g.GetComponent<Canvas>().renderMode=RenderMode.WorldSpace;g.GetComponent<Canvas>().worldCamera=Camera.main;g.GetComponent<RectTransform>().sizeDelta=size;g.GetComponent<Image>().color=new Color(.015f,.045f,.08f);g.GetComponent<Image>().raycastTarget=false;foreach(float y in new[]{-size.y*.5f,size.y*.5f})Image(g.transform,new Vector2(0,y),new Vector2(size.x,3),blue);return g.transform;}
        static Image Image(Transform parent,Vector2 p,Vector2 size,Color color){var g=new GameObject("Marco",typeof(RectTransform),typeof(Image));g.transform.SetParent(parent,false);var r=g.GetComponent<RectTransform>();r.anchoredPosition=p;r.sizeDelta=size;var i=g.GetComponent<Image>();i.color=color;i.raycastTarget=false;return i;}
        static TMP_Text Text(Transform parent,string value,Vector2 p,Vector2 size,float font,Color color){var g=new GameObject("Texto",typeof(RectTransform),typeof(TextMeshProUGUI));g.transform.SetParent(parent,false);var t=g.GetComponent<TextMeshProUGUI>();t.text=value;t.font=TMP_Settings.defaultFontAsset;t.fontSize=font;t.color=color;t.alignment=TextAlignmentOptions.Center;t.raycastTarget=false;t.rectTransform.anchoredPosition=p;t.rectTransform.sizeDelta=size;return t;}
        static Button MakeButton(Transform parent,string title,Vector2 p,Vector2 size){var i=Image(parent,p,size,new Color(.025f,.15f,.23f));i.raycastTarget=true;var b=i.gameObject.AddComponent<Button>();b.targetGraphic=i;var n=b.navigation;n.mode=Navigation.Mode.None;b.navigation=n;Text(i.transform,title,Vector2.zero,size-new Vector2(10,5),30,Color.white);return b;}
    }
}
