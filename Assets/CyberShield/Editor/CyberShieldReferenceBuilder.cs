using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace CyberShield.Editor
{
    public static class CyberShieldReferenceBuilder
    {
        public const string Path="Assets/CyberShield/Scenes/CyberShieldVR_ShieldOps.unity";
        static readonly Color Ink=new Color(.018f,.06f,.12f,.97f), Cyan=new Color(.02f,.75f,1), Amber=new Color(1,.35f,.035f), Green=new Color(.08f,.9f,.5f);
        [MenuItem("CyberShield/Crear Shield Ops")]
        public static string Build() => Build(false);
        public static string Build(bool replaceIncomplete)
        {
            if(AssetDatabase.LoadAssetAtPath<SceneAsset>(Path)&&!replaceIncomplete)throw new Exception("Shield Ops ya existe");
            var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if(!scene.path.EndsWith("CyberShieldVR_NightBreach.unity"))throw new Exception("Abre Night Breach");
            EditorSceneManager.SaveScene(scene,Path,true);scene=EditorSceneManager.OpenScene(Path);
            var game=UnityEngine.Object.FindFirstObjectByType<CyberShieldGame>();var cinema=game.GetComponent<CyberShieldCinemaScreens>();
            var flow=game.gameObject.AddComponent<CyberShieldReferenceExperience>();flow.game=game;flow.hands=game.GetComponent<CyberShieldHandsOn>();
            var root=new GameObject("CYBERSHIELD / SHIELD OPS").transform;
            var mail=cinema.mailStatus.GetComponentInParent<Canvas>().transform;
            foreach(var image in mail.GetComponentsInChildren<Image>())
            {
                if(image.name=="Cabecera")image.color=new Color(.04f,.15f,.28f);
                if(image.name=="Bandeja")image.color=new Color(.055f,.17f,.3f);
            }
            string[] messages={"COMUNICADOS     10:24\nRecordatorio de reunión","URGENTE: verifica tu cuenta     09:17\nActualiza tus datos para evitar el bloqueo","EQUIPO TI     08:03\nNuevos recursos disponibles"};
            for(int i=0;i<3;i++)
            {
                var b=game.choices[i];b.GetComponent<Image>().color=new Color(.7f,.81f,.9f);
                var colors=b.colors;colors.normalColor=Color.white;colors.highlightedColor=Amber;colors.pressedColor=new Color(1,.65f,.25f);colors.selectedColor=Amber;colors.disabledColor=new Color(.45f,.58f,.68f);colors.colorMultiplier=1;b.colors=colors;
                var t=b.GetComponentInChildren<TMP_Text>();t.text=messages[i];t.color=new Color(.015f,.055f,.1f);t.fontSize=25;
            }
            // Existing animated network becomes a tilted, interactive holographic surface.
            var network=cinema.networkStatus.GetComponentInParent<Canvas>().gameObject;
            network.name="Holograma / AISLAR";network.transform.SetParent(root,true);network.transform.position=new Vector3(0,1.6f,1.7f);network.transform.rotation=Quaternion.Euler(16,0,0);network.transform.localScale=Vector3.one*.0017f;
            var background=network.GetComponent<Image>();background.color=Ink;
            flow.networkPanel=network;flow.computerFrames=new Image[cinema.nodes.Length];
            for(int i=0;i<cinema.nodes.Length;i++)
            {
                var node=cinema.nodes[i];var position=node.rectTransform.anchoredPosition;
                var frame=Rect(network.transform,position,new Vector2(180,105),new Color(.025f,.1f,.17f));frame.name="Seleccionar equipo "+i;frame.raycastTarget=true;flow.computerFrames[i]=frame;
                node.transform.SetParent(frame.transform,false);node.rectTransform.anchoredPosition=new Vector2(0,16);
                Text(frame.transform,i==2?"PC-02 / INFECTADO":"EQUIPO "+(i+1),new Vector2(0,-32),new Vector2(175,25),18,Color.white);
                var button=frame.gameObject.AddComponent<Button>();button.targetGraphic=frame;var navigation=button.navigation;navigation.mode=Navigation.Mode.None;button.navigation=navigation;
                UnityEventTools.AddIntPersistentListener(button.onClick,flow.SelectComputer,i);
            }
            // Keep confirmation separate from the computer selection.
            flow.isolateButton=Button(network.transform,"AISLAR",new Vector2(0,-288),new Vector2(550,80),Amber);
            flow.isolationAction=flow.isolateButton.GetComponentInChildren<TMP_Text>();UnityEventTools.AddPersistentListener(flow.isolateButton.onClick,flow.Isolate);
            foreach(var bar in cinema.telemetry)bar.gameObject.SetActive(false);
            var oldTelemetry=network.transform.Find("Telemetría");if(oldTelemetry)oldTelemetry.gameObject.SetActive(false);
            var machine=Panel(root,"Monitor / defensa",new Vector3(0,1.55f,2.7f),new Vector2(1000,740),.0022f);
            Shield(machine,new Vector2(0,80),1.1f,Cyan);
            Text(machine,"DEFENSA DE RED",new Vector2(0,285),new Vector2(800,60),38,Cyan);
            Text(machine,"SEÑALA  →  CONFIRMA  →  AISLA",new Vector2(0,-210),new Vector2(850,60),29,Color.white);
            // Physical cards remain valid; this panel offers an explicit restore confirmation.
            var recovery=Panel(root,"Holograma / RESTAURAR",new Vector3(2.55f,1.65f,1.65f),new Vector2(920,620),.00145f);flow.recoveryPanel=recovery.gameObject;
            Text(recovery,"RESTAURAR SISTEMA",new Vector2(0,245),new Vector2(830,65),40,Cyan);
            flow.backupFrames=new Image[3];
            string[] copies={"COPIA A\nAYER 18:00\nOFFLINE","COPIA B\nHOY 10:15\nPOSTINCIDENTE","COPIA C\nSINCRONIZADA\nRED AFECTADA"};
            for(int i=0;i<3;i++)
            {
                var button=Button(recovery,copies[i],new Vector2(-285+i*285,30),new Vector2(260,270),new Color(.03f,.15f,.25f));flow.backupFrames[i]=button.GetComponent<Image>();button.GetComponentInChildren<TMP_Text>().fontSize=29;
                UnityEventTools.AddIntPersistentListener(button.onClick,flow.SelectBackup,i);
                Rect(button.transform,new Vector2(0,95),new Vector2(50,5),Cyan);Rect(button.transform,new Vector2(0,85),new Vector2(50,5),Cyan);Rect(button.transform,new Vector2(0,75),new Vector2(50,5),Cyan);
            }
            flow.restoreButton=Button(recovery,"RESTAURAR",new Vector2(0,-205),new Vector2(750,90),Green);flow.restoreAction=flow.restoreButton.GetComponentInChildren<TMP_Text>();UnityEventTools.AddPersistentListener(flow.restoreButton.onClick,flow.Restore);
            var victory=Panel(root,"SISTEMA RECUPERADO",new Vector3(0,1.65f,1.25f),new Vector2(1050,920),.0017f);flow.victoryPanel=victory.gameObject;
            Shield(victory,new Vector2(0,160),1.1f,Green);
            Text(victory,"SISTEMA RECUPERADO",new Vector2(0,-25),new Vector2(960,85),55,Color.white);
            Text(victory,"AMENAZA IDENTIFICADA\n\nEQUIPO AISLADO\n\nRESPALDO RESTAURADO",new Vector2(0,-190),new Vector2(900,240),34,Green);
            for(int i=0;i<3;i++){float y=-115-i*75;Line(victory,new Vector2(-400,y),new Vector2(-385,y-14),Green,7);Line(victory,new Vector2(-385,y-14),new Vector2(-360,y+16),Green,7);}
            flow.resultScore=Text(victory,"",new Vector2(0,-355),new Vector2(800,50),34,Color.white);
            var restart=Button(victory,"VOLVER A JUGAR",new Vector2(0,-420),new Vector2(700,65),new Color(.03f,.3f,.25f));UnityEventTools.AddPersistentListener(restart.onClick,game.Restart);
            var atmosphere=game.GetComponent<CyberShieldAtmosphere>();flow.oldDebrief=atmosphere.debrief;flow.oldDebrief.SetActive(false);atmosphere.debrief=victory.gameObject;
            var breach=game.GetComponent<CyberShieldBreachAtmosphere>();breach.incidentDisplay.fontSize=.75f;
            // Warm practical lights among the cool workstations, matching the reference palette.
            foreach(var light in breach.accentLights){light.range=5;light.intensity=.9f;}
            var main=GameObject.Find("Luz principal").GetComponent<Light>();main.intensity=.55f;main.color=new Color(.7f,.83f,1);RenderSettings.ambientLight=new Color(.22f,.27f,.34f);
            network.SetActive(false);recovery.gameObject.SetActive(false);victory.gameObject.SetActive(false);
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            if(!EditorBuildSettings.scenes.Any(s=>s.path==Path))EditorBuildSettings.scenes=EditorBuildSettings.scenes.Concat(new[]{new EditorBuildSettingsScene(Path,true)}).ToArray();return Path;
        }
        static Transform Panel(Transform parent,string name,Vector3 p,Vector2 size,float scale)
        {
            var g=new GameObject(name,typeof(RectTransform),typeof(Canvas),typeof(GraphicRaycaster),typeof(TrackedDeviceGraphicRaycaster),typeof(Image));g.transform.SetParent(parent);g.transform.position=p;g.transform.localScale=Vector3.one*scale;g.GetComponent<Canvas>().renderMode=RenderMode.WorldSpace;g.GetComponent<Canvas>().worldCamera=Camera.main;g.GetComponent<RectTransform>().sizeDelta=size;g.GetComponent<Image>().color=Ink;g.GetComponent<Image>().raycastTarget=false;
            foreach(var edge in new[]{new Vector2(0,size.y*.5f),new Vector2(0,-size.y*.5f)})Rect(g.transform,edge,new Vector2(size.x,3),Cyan);
            foreach(var edge in new[]{new Vector2(size.x*.5f,0),new Vector2(-size.x*.5f,0)})Rect(g.transform,edge,new Vector2(3,size.y),Cyan);return g.transform;
        }
        static Image Rect(Transform parent,Vector2 pos,Vector2 size,Color color){var g=new GameObject("Trazo",typeof(RectTransform),typeof(Image));g.transform.SetParent(parent,false);var r=g.GetComponent<RectTransform>();r.anchoredPosition=pos;r.sizeDelta=size;var i=g.GetComponent<Image>();i.color=color;i.raycastTarget=false;return i;}
        static TMP_Text Text(Transform parent,string value,Vector2 pos,Vector2 size,float font,Color color){var g=new GameObject(value,typeof(RectTransform),typeof(TextMeshProUGUI));g.transform.SetParent(parent,false);var t=g.GetComponent<TextMeshProUGUI>();t.text=value;t.font=TMP_Settings.defaultFontAsset;t.fontSize=font;t.color=color;t.alignment=TextAlignmentOptions.Center;t.raycastTarget=false;t.rectTransform.anchoredPosition=pos;t.rectTransform.sizeDelta=size;return t;}
        static Button Button(Transform parent,string title,Vector2 pos,Vector2 size,Color color){var i=Rect(parent,pos,size,color);i.name=title;i.raycastTarget=true;var b=i.gameObject.AddComponent<Button>();b.targetGraphic=i;Text(i.transform,title,Vector2.zero,size-new Vector2(20,10),32,Color.white);return b;}
        static void Line(Transform parent,Vector2 a,Vector2 b,Color color,float width){var i=Rect(parent,(a+b)*.5f,new Vector2(Vector2.Distance(a,b),width),color);i.rectTransform.localRotation=Quaternion.Euler(0,0,Mathf.Atan2(b.y-a.y,b.x-a.x)*Mathf.Rad2Deg);}
        static void Shield(Transform parent,Vector2 p,float scale,Color color)
        {
            Vector2[] shape={new Vector2(-85,110),new Vector2(0,145),new Vector2(85,110),new Vector2(72,5),new Vector2(0,-55),new Vector2(-72,5),new Vector2(-85,110)};
            for(int i=0;i<shape.Length-1;i++)Line(parent,p+shape[i]*scale,p+shape[i+1]*scale,color,8);
            Line(parent,p+new Vector2(-40,45)*scale,p+new Vector2(-8,10)*scale,Color.white,12);Line(parent,p+new Vector2(-8,10)*scale,p+new Vector2(45,76)*scale,Color.white,12);
        }
    }
}
