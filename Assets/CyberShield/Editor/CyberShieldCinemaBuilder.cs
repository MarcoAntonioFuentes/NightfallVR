using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace CyberShield.Editor
{
    public static class CyberShieldCinemaBuilder
    {
        public const string Path="Assets/CyberShield/Scenes/CyberShieldVR_Cinema.unity";
        static readonly Color Ink=new Color(.012f,.025f,.047f), Panel=new Color(.025f,.065f,.1f), Cyan=new Color(.04f,.78f,1), Muted=new Color(.35f,.56f,.65f), White=new Color(.85f,.95f,1);
        static Transform root;
        [MenuItem("CyberShield/Crear pantallas Cinema")]
        public static string Build()
        {
            if(AssetDatabase.LoadAssetAtPath<SceneAsset>(Path))throw new Exception("Cinema ya existe");
            var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if(scene.isDirty)throw new Exception("La escena tiene cambios sin guardar");
            if(!scene.path.EndsWith("CyberShieldVR_Dynamic.unity"))throw new Exception("Abre Dynamic");
            EditorSceneManager.SaveScene(scene,Path,true);scene=EditorSceneManager.OpenScene(Path);
            root=new GameObject("NEXORA / Interfaces cinematográficas").transform;
            var game=UnityEngine.Object.FindFirstObjectByType<CyberShieldGame>();
            var screens=game.gameObject.AddComponent<CyberShieldCinemaScreens>();screens.game=game;
            // Reuse the existing wired decision buttons instead of creating a second game flow.
            var left=Screen("01 / Correo",new Vector3(-2.5f,1.55f,2.73f));
            Header(left,"01","NEXORA MAIL","BANDEJA DE ENTRADA");
            Rect(left,"Bandeja",new Vector2(-350,-5),new Vector2(225,450),Panel);
            Text(left,"ENTRADA\n\nDESTACADOS\n\nENVIADOS",new Vector2(-350,70),new Vector2(190,230),22,Muted);
            Envelope(left,new Vector2(-350,-125),85,Cyan);
            Text(left,"SELECCIONA EL CORREO FALSO",new Vector2(110,225),new Vector2(650,45),24,White);
            string[] labels={"CALENDARIO     ·     09:20\nReunión de equipo  /  hoy 15:00","SOPORTE     ·     09:42\nVerifica tu contraseña AHORA","BIBLIOTECA     ·     09:47\nTu reserva está confirmada"};
            for(int i=0;i<3;i++)
            {
                var button=game.choices[i];button.transform.SetParent(left,false);var r=button.GetComponent<RectTransform>();
                r.anchorMin=r.anchorMax=r.pivot=new Vector2(.5f,.5f);r.anchoredPosition=new Vector2(110,125-i*125);r.sizeDelta=new Vector2(645,108);r.localScale=Vector3.one;button.gameObject.SetActive(true);
                var layout=button.GetComponent<LayoutElement>();if(layout)layout.ignoreLayout=true;
                var image=button.GetComponent<Image>();image.color=new Color(.035f,.12f,.18f);
                var text=button.GetComponentInChildren<TMP_Text>();text.text=labels[i];text.fontSize=26;text.enableAutoSizing=false;text.alignment=TextAlignmentOptions.MidlineLeft;
                text.rectTransform.anchorMin=Vector2.zero;text.rectTransform.anchorMax=Vector2.one;text.rectTransform.offsetMin=new Vector2(24,8);text.rectTransform.offsetMax=new Vector2(-15,-8);
            }
            screens.evidenceFlags=new Image[3];for(int i=0;i<3;i++)screens.evidenceFlags[i]=Rect(left,"Pista",new Vector2(-50+i*125,-292),new Vector2(108,7),Muted);
            screens.mailStatus=Text(left,"",new Vector2(80,-250),new Vector2(700,35),22,Cyan);
            var old=GameObject.Find("01 / DETECTAR");if(old)old.SetActive(false);

            var middle=Screen("02 / Red corporativa",new Vector3(0,1.55f,2.73f));Header(middle,"02","NETWORK DEFENSE","NEXORA / RED INTERNA");
            Vector2[] nodes={new Vector2(-310,120),new Vector2(290,120),new Vector2(0,-10),new Vector2(-280,-165),new Vector2(280,-165)};
            var routes=new List<Image>();
            for(int i=0;i<nodes.Length;i++)if(i!=2)routes.Add(Link(middle,nodes[2],nodes[i],new Color(.3f,.12f,.12f),4));
            screens.routes=routes.ToArray();screens.nodes=new Image[nodes.Length];
            string[] names={"CORREO","ERP","PC-02","ARCHIVOS","RESPALDO"};
            for(int i=0;i<nodes.Length;i++)
            {
                Rect(middle,"Tarjeta de equipo",nodes[i],new Vector2(i==2?180:150,105),Panel);
                screens.nodes[i]=Rect(middle,"Monitor",nodes[i]+new Vector2(0,8),new Vector2(60,36),Cyan);
                Rect(middle,"Pie",nodes[i]+new Vector2(0,-17),new Vector2(22,4),Muted);
                Text(middle,names[i],nodes[i]+new Vector2(0,-42),new Vector2(180,25),20,White);
            }
            screens.networkStatus=Text(middle,"",new Vector2(0,235),new Vector2(760,35),27,Cyan);
            Rect(middle,"Telemetría",new Vector2(0,-278),new Vector2(875,95),Panel);
            screens.telemetry=new Image[25];for(int i=0;i<25;i++)screens.telemetry[i]=Rect(middle,"Actividad",new Vector2(-400+i*33,-285),new Vector2(18,35),Cyan);
            screens.cursor=Rect(middle,"Barrido",new Vector2(0,0),new Vector2(2,360),new Color(.07f,.32f,.4f)).rectTransform;

            var right=Screen("03 / Recuperación",new Vector3(2.5f,1.55f,2.73f));Header(right,"03","RECOVERY VAULT","OFFLINE / COPIAS SEGURAS");
            screens.recoveryStatus=Text(right,"",new Vector2(0,235),new Vector2(870,38),25,Cyan);
            screens.gauge=new Image[32];
            for(int i=0;i<32;i++){float angle=i*Mathf.PI*2/32;var p=new Vector2(Mathf.Sin(angle)*135,Mathf.Cos(angle)*135+25);var segment=Rect(right,"Sector de integridad",p,new Vector2(10,23),Muted);segment.rectTransform.localRotation=Quaternion.Euler(0,0,-angle*Mathf.Rad2Deg);screens.gauge[i]=segment;}
            screens.gaugeLabel=Text(right,"00%",new Vector2(0,25),new Vector2(200,100),60,White);
            screens.files=new Image[24];for(int i=0;i<24;i++)
            {
                var p=new Vector2(-380+(i%12)*69,-195-(i/12)*65);
                screens.files[i]=Rect(right,"Archivo",p,new Vector2(49,46),Panel);
                Rect(screens.files[i].transform,"Detalle",new Vector2(0,4),new Vector2(26,3),Muted);
                Rect(screens.files[i].transform,"Detalle",new Vector2(0,-5),new Vector2(26,3),Muted);
            }
            Text(right,"ANALIZAR   →   VALIDAR   →   RESTAURAR",new Vector2(0,-310),new Vector2(900,30),21,Muted);

            // The on-screen network replaces floating debug-style bars and long headings.
            foreach(var t in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None))if(t.name=="Telemetría visual")t.gameObject.SetActive(false);
            var arcade=game.GetComponent<CyberShieldArcade>();arcade.threat.position=new Vector3(0,3.35f,2.9f);
            foreach(var ring in arcade.rings)ring.transform.position=arcade.threat.position;
            arcade.objective.transform.position=new Vector3(0,3.02f,2.65f);arcade.objective.fontSize=.8f;
            arcade.compactStatus.transform.position=new Vector3(0,3.7f,2.8f);arcade.compactStatus.fontSize=.9f;
            arcade.toast.transform.position=new Vector3(0,2.72f,2.6f);arcade.toast.fontSize=.65f;
            var light=GameObject.Find("Luz principal").GetComponent<Light>();light.intensity=.65f;light.color=new Color(.8f,.9f,1);light.shadowStrength=.5f;
            RenderSettings.ambientLight=new Color(.25f,.3f,.36f);
            var evidence=GameObject.Find("Expediente de correo");if(evidence)evidence.transform.localScale*=.8f;
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            if(!EditorBuildSettings.scenes.Any(s=>s.path==Path))EditorBuildSettings.scenes=EditorBuildSettings.scenes.Concat(new[]{new EditorBuildSettingsScene(Path,true)}).ToArray();return Path;
        }
        static Transform Screen(string name,Vector3 position)
        {
            var g=new GameObject(name,typeof(RectTransform),typeof(Canvas),typeof(GraphicRaycaster),typeof(TrackedDeviceGraphicRaycaster),typeof(Image));g.transform.SetParent(root);g.transform.position=position;g.transform.localScale=Vector3.one*.0022f;
            g.GetComponent<RectTransform>().sizeDelta=new Vector2(1000,740);g.GetComponent<Canvas>().renderMode=RenderMode.WorldSpace;g.GetComponent<Canvas>().worldCamera=Camera.main;g.GetComponent<Image>().color=Ink;g.GetComponent<Image>().raycastTarget=false;return g.transform;
        }
        static void Header(Transform parent,string index,string title,string subtitle)
        {
            Rect(parent,"Cabecera",new Vector2(0,305),new Vector2(1000,130),Panel);Rect(parent,"Acento",new Vector2(-475,305),new Vector2(5,95),Cyan);
            Text(parent,index,new Vector2(-418,320),new Vector2(85,65),45,Cyan);Text(parent,title,new Vector2(70,330),new Vector2(750,55),34,White);Text(parent,subtitle,new Vector2(70,282),new Vector2(750,30),20,Muted);
        }
        static Image Rect(Transform parent,string name,Vector2 pos,Vector2 size,Color color)
        {
            var g=new GameObject(name,typeof(RectTransform),typeof(Image));g.transform.SetParent(parent,false);var r=g.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=r.pivot=new Vector2(.5f,.5f);r.anchoredPosition=pos;r.sizeDelta=size;var image=g.GetComponent<Image>();image.color=color;image.raycastTarget=false;return image;
        }
        static TMP_Text Text(Transform parent,string content,Vector2 pos,Vector2 size,float font,Color color)
        {
            var g=new GameObject("Texto / "+content,typeof(RectTransform),typeof(TextMeshProUGUI));g.transform.SetParent(parent,false);var t=g.GetComponent<TextMeshProUGUI>();t.text=content;t.font=TMP_Settings.defaultFontAsset;t.fontSize=font;t.color=color;t.alignment=TextAlignmentOptions.Center;t.raycastTarget=false;t.rectTransform.anchoredPosition=pos;t.rectTransform.sizeDelta=size;return t;
        }
        static Image Link(Transform parent,Vector2 from,Vector2 to,Color color,float width)
        {var image=Rect(parent,"Conexión",(from+to)*.5f,new Vector2(Vector2.Distance(from,to),width),color);image.rectTransform.localRotation=Quaternion.Euler(0,0,Mathf.Atan2(to.y-from.y,to.x-from.x)*Mathf.Rad2Deg);return image;}
        static void Envelope(Transform parent,Vector2 pos,float size,Color color)
        {Rect(parent,"Sobre",pos,new Vector2(size,size*.63f),color);Link(parent,pos+new Vector2(-size*.5f,size*.32f),pos-new Vector2(0,size*.1f),Panel,5);Link(parent,pos+new Vector2(size*.5f,size*.32f),pos-new Vector2(0,size*.1f),Panel,5);}
    }
}
