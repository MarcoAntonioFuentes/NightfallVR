using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace CyberShield.Editor
{
    public static class CyberShieldBuilder
    {
        public const string ScenePath = "Assets/CyberShield/Scenes/CyberShieldVR.unity";
        static readonly Color Ink = new Color(0.025f, 0.055f, 0.09f);
        static readonly Color Cyan = new Color(0.05f, 0.85f, 0.95f);

        [MenuItem("CyberShield/Crear sala VR")]
        public static void Build()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath))
                throw new InvalidOperationException("La escena CyberShield ya existe; no se sobrescribirá.");
            System.IO.Directory.CreateDirectory("Assets/CyberShield/Scenes");
            System.IO.Directory.CreateDirectory("Assets/CyberShield/Materials");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            var shell = new GameObject("Sala informática").transform;
            var dark = Material("Grafito", new Color(0.08f, 0.12f, 0.17f));
            var wall = Material("Pared", new Color(0.2f, 0.28f, 0.34f));
            var accent = Material("Cian", Cyan);
            var floor = Box("Suelo · teletransporte", new Vector3(0,-0.1f,0), new Vector3(10,0.2f,10), dark, shell);
            floor.AddComponent<TeleportationArea>().interactionLayers = 1 << 31;
            Box("Pared posterior", new Vector3(0,1.8f,4.9f), new Vector3(10,3.6f,0.2f),wall,shell);
            Box("Pared izquierda", new Vector3(-4.9f,1.8f,0), new Vector3(0.2f,3.6f,10),wall,shell);
            Box("Pared derecha", new Vector3(4.9f,1.8f,0), new Vector3(0.2f,3.6f,10),wall,shell);
            Box("Pared entrada", new Vector3(0,1.8f,-4.9f), new Vector3(10,3.6f,0.2f),wall,shell);
            Box("Franja de seguridad", new Vector3(0,0.3f,4.77f), new Vector3(9.6f,0.05f,0.025f),accent,shell);
            var light = new GameObject("Luz principal").AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            light.transform.rotation = Quaternion.Euler(45,-25,0);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.5f,0.58f,0.65f);

            var rigAsset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/VRTemplateAssets/Prefabs/Setup/Complete XR Origin Set Up Variant.prefab");
            if (!rigAsset) throw new InvalidOperationException("No se encontró el rig VR de la plantilla.");
            var rig = (GameObject)PrefabUtility.InstantiatePrefab(rigAsset, scene);
            rig.transform.position = new Vector3(0,0,-0.8f);
            rig.transform.rotation = Quaternion.identity;
            if (!UnityEngine.Object.FindObjectsByType<XRInteractionManager>(FindObjectsSortMode.None).Any(x=>x.gameObject.scene==scene))
                new GameObject("XR Interaction Manager",typeof(XRInteractionManager));
            if (!UnityEngine.Object.FindObjectsByType<UnityEngine.EventSystems.EventSystem>(FindObjectsSortMode.None).Any(x=>x.gameObject.scene==scene))
                new GameObject("EventSystem",typeof(UnityEngine.EventSystems.EventSystem),typeof(XRUIInputModule));

            var game = new GameObject("CyberShield · Control de misión").AddComponent<CyberShieldGame>();
            game.speaker = game.gameObject.AddComponent<AudioSource>();
            game.speaker.playOnAwake = false;
            game.speaker.spatialBlend = 0;
            game.stationStatus = new TMP_Text[3];
            game.choices = new UnityEngine.UI.Button[9];
            string[] titles = {"01 / DETECTAR", "02 / AISLAR", "03 / RESTAURAR"};
            string[] descriptions = {
                "Selecciona el correo sospechoso para reportarlo. No abras enlaces ni adjuntos.",
                "ALERTA · PC-02 ejecutó un adjunto malicioso. PC-01 y PC-03 siguen sanos. Contén el incidente sin destruir evidencias.",
                "Incidente: hoy, 10:00. PC-02 está aislado. Selecciona una copia limpia para restaurarla en el entorno aislado."};
            string[][] options = {
                new[]{"A · Calendario\nreuniones@empresa.example\nReunión del equipo a las 15:00.","B · Tu cuenta será bloqueada\nsoporte@empres4.example\nConfirma tu contraseña en 10 minutos.","C · Mantenimiento\nTI@empresa.example\nAviso programado. No solicita datos."},
                new[]{"A · Apagar todos los equipos\nInterrumpir toda la sala.","B · Formatear PC-02\nBorrar el disco de inmediato.","C · Aislar la red de PC-02\nDesconectar Ethernet y Wi-Fi; conservar evidencias."},
                new[]{"A · Copia de ayer · 18:00\nOffline · integridad verificada · análisis limpio.","B · Copia de hoy · 10:15\nPosterior al incidente · no verificada.","C · Carpeta sincronizada\nEn la red afectada · integridad desconocida."}
            };
            for (int i=0;i<3;i++)
            {
                float x=(i-1)*2.5f;
                Box("Mesa PC-0"+(i+1),new Vector3(x,0.72f,2.7f),new Vector3(2.25f,0.12f,1f),dark,shell);
                Box("Soporte PC-0"+(i+1),new Vector3(x,0.34f,2.9f),new Vector3(0.5f,0.68f,0.5f),wall,shell);
                Box("Monitor PC-0"+(i+1),new Vector3(x,1.55f,2.85f),new Vector3(2.24f,1.66f,0.12f),dark,shell);
                var pc=Box("Torre PC-0"+(i+1),new Vector3(x+0.8f,0.35f,2.3f),new Vector3(0.25f,0.65f,0.5f),dark,shell);
                var led=Box("Estado PC-0"+(i+1),new Vector3(x+0.8f,0.55f,2.04f),new Vector3(0.12f,0.04f,0.025f),accent,pc.transform);
                if(i==1) game.compromisedIndicator=led.GetComponent<Renderer>();
                var panel=Panel(titles[i],new Vector3(x,1.55f,2.77f),new Vector2(1000,740),0.0022f);
                Text(panel,"Título",titles[i],36,70,Cyan);
                game.stationStatus[i]=Text(panel,"Estado",i==0?"RETO ACTIVO":"BLOQUEADO · completa el reto anterior",23,38,Cyan);
                Text(panel,"Instrucción",descriptions[i],28,115,Color.white);
                for(int j=0;j<3;j++)
                {
                    var button=Button(panel,options[i][j],112);
                    game.choices[i*3+j]=button;
                    button.interactable=i==0;
                    UnityEventTools.AddIntPersistentListener(button.onClick,game.Choose,i*3+j);
                }
            }
            var board=Panel("Estado de misión",new Vector3(0,3.25f,3.8f),new Vector2(1600,340),0.0025f);
            game.scoreText=Text(board,"Puntuación","CYBERSHIELD VR   |   00/100 puntos   |   Errores: 0",45,75,Cyan);
            game.feedbackText=Text(board,"Feedback","Revisa los mensajes y reporta el intento de phishing.",32,100,Color.white);
            Text(board,"Controles","Apunta con el mando y pulsa el gatillo. Usa el teletransporte para acercarte.",25,45,Color.white);
            var reset=Button(board,"REINICIAR ENTRENAMIENTO",55);
            UnityEventTools.AddPersistentListener(reset.onClick,game.Restart);
            EditorSceneManager.SaveScene(scene,ScenePath);
            var scenes=EditorBuildSettings.scenes.ToList();
            if(!scenes.Any(s=>s.path==ScenePath)) scenes.Add(new EditorBuildSettingsScene(ScenePath,true));
            EditorBuildSettings.scenes=scenes.ToArray();
            AssetDatabase.SaveAssets();
            // Close only clean prior scenes, keeping unsaved work untouched.
            for(int i=SceneManager.sceneCount-1;i>=0;i--)
            {
                var old=SceneManager.GetSceneAt(i);
                if(old!=scene && !old.isDirty) EditorSceneManager.CloseScene(old,true);
            }
            Selection.activeGameObject=game.gameObject;
        }

        static Material Material(string name,Color color)
        {
            var mat=new Material(Shader.Find("Universal Render Pipeline/Lit")){name=name,color=color};
            AssetDatabase.CreateAsset(mat,"Assets/CyberShield/Materials/"+name+".mat");
            return mat;
        }
        static GameObject Box(string name,Vector3 position,Vector3 scale,Material mat,Transform parent)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name=name;
            go.transform.position=position;
            go.transform.localScale=scale;
            go.transform.SetParent(parent,true);
            go.GetComponent<Renderer>().sharedMaterial=mat;
            return go;
        }
        static Transform Panel(string name,Vector3 position,Vector2 size,float scale)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Canvas),typeof(UnityEngine.UI.CanvasScaler),typeof(TrackedDeviceGraphicRaycaster),typeof(UnityEngine.UI.Image),typeof(UnityEngine.UI.VerticalLayoutGroup));
            go.GetComponent<Canvas>().renderMode=RenderMode.WorldSpace;
            var rt=go.GetComponent<RectTransform>();
            rt.sizeDelta=size;
            rt.position=position;
            rt.localScale=Vector3.one*scale;
            go.GetComponent<UnityEngine.UI.Image>().color=Ink;
            go.GetComponent<UnityEngine.UI.Image>().raycastTarget=false;
            var layout=go.GetComponent<UnityEngine.UI.VerticalLayoutGroup>();
            layout.padding=new RectOffset(24,24,18,18);
            layout.spacing=10;
            layout.childControlHeight=true;
            layout.childControlWidth=true;
            layout.childForceExpandHeight=false;
            layout.childForceExpandWidth=true;
            return go.transform;
        }
        static TMP_Text Text(Transform parent,string name,string content,int size,float height,Color color)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(TextMeshProUGUI),typeof(UnityEngine.UI.LayoutElement));
            go.transform.SetParent(parent,false);
            var t=go.GetComponent<TextMeshProUGUI>();
            t.text=content;t.fontSize=size;t.color=color;t.raycastTarget=false;
            t.font=TMP_Settings.defaultFontAsset;
            t.alignment=TextAlignmentOptions.MidlineLeft;
            go.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight=height;
            return t;
        }
        static UnityEngine.UI.Button Button(Transform parent,string content,float height)
        {
            var go=new GameObject("Opción",typeof(RectTransform),typeof(UnityEngine.UI.Image),typeof(UnityEngine.UI.Button),typeof(UnityEngine.UI.LayoutElement));
            go.transform.SetParent(parent,false);
            var img=go.GetComponent<UnityEngine.UI.Image>();img.color=new Color(0.13f,0.28f,0.38f);
            var b=go.GetComponent<UnityEngine.UI.Button>();b.targetGraphic=img;
            var colors=b.colors;colors.highlightedColor=new Color(0.4f,0.95f,1);colors.disabledColor=new Color(0.45f,0.5f,0.55f);b.colors=colors;
            go.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight=height;
            var label=Text(go.transform,"Texto",content,27,height,Color.white);
            var rt=label.rectTransform;rt.anchorMin=Vector2.zero;rt.anchorMax=Vector2.one;rt.offsetMin=new Vector2(20,6);rt.offsetMax=new Vector2(-20,-6);
            return b;
        }
    }
}
