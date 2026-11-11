using System;
using TMPro;
using UnityEngine;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace CyberShield.Editor
{
    public static class CyberShieldHandsOnBuilder
    {
        public const string Path="Assets/CyberShield/Scenes/CyberShieldVR_HandsOn.unity";
        [MenuItem("CyberShield/Crear versión interactiva")]
        public static string Build()
        {
            if(AssetDatabase.LoadAssetAtPath<SceneAsset>(Path)) throw new Exception("La versión interactiva ya existe.");
            var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if(scene.path!=CyberShieldOperationsBuilder.Path)throw new Exception("Abre Operations primero.");
            EditorSceneManager.SaveScene(scene);
            EditorSceneManager.SaveScene(scene,Path,true);
            scene=EditorSceneManager.OpenScene(Path);
            var game=UnityEngine.Object.FindFirstObjectByType<CyberShieldGame>();game.physicalMode=true;
            var h=game.gameObject.AddComponent<CyberShieldHandsOn>();h.game=game;
            var dark=AssetDatabase.LoadAssetAtPath<Material>("Assets/CyberShield/Materials/Operations/Titanio.mat");
            var cyan=AssetDatabase.LoadAssetAtPath<Material>("Assets/CyberShield/Materials/Operations/Circuito cian.mat");
            var amber=AssetDatabase.LoadAssetAtPath<Material>("Assets/CyberShield/Materials/Operations/Seguridad ámbar.mat");
            var baseObject=Cube("Módulo físico PC-02",new Vector3(0,0.82f,1.35f),new Vector3(1.5f,0.12f,0.6f),dark);
            h.isolationLabel=Label("PC-02 / AISLAMIENTO\nEthernet: CONECTADO\nWi-Fi: ACTIVO",new Vector3(0,1.15f,1.62f),new Vector2(1.4f,0.4f),2);
            var e=Cube("Desconectar Ethernet",new Vector3(-0.4f,0.96f,1.25f),new Vector3(0.3f,0.16f,0.22f),amber);
            h.ethernetSwitch=e.AddComponent<XRSimpleInteractable>();h.ethernetLamp=e.GetComponent<Renderer>();
            Label("ETHERNET",new Vector3(-0.4f,0.96f,1.12f),new Vector2(0.28f,0.12f),0.8f).transform.SetParent(e.transform,true);
            var w=Cube("Desactivar Wi-Fi",new Vector3(0.4f,0.96f,1.25f),new Vector3(0.3f,0.16f,0.22f),amber);
            h.wifiSwitch=w.AddComponent<XRSimpleInteractable>();h.wifiLamp=w.GetComponent<Renderer>();
            Label("WI-FI",new Vector3(0.4f,0.96f,1.12f),new Vector2(0.28f,0.12f),0.8f).transform.SetParent(w.transform,true);
            Cube("Mesa de respaldo",new Vector3(2.5f,0.76f,1.25f),new Vector3(2.1f,0.1f,0.8f),dark);
            var reader=Cube("Lector de respaldo",new Vector3(2.95f,0.86f,1.2f),new Vector3(0.5f,0.1f,0.5f),cyan);
            // Independent unscaled coordinate frame for the detection volume.
            h.reader=new GameObject("Zona de lectura").transform;h.reader.position=new Vector3(2.95f,0.88f,1.2f);
            h.readerLabel=Label("LECTOR OFFLINE\nColoca y suelta una tarjeta aquí",new Vector3(2.95f,1.15f,1.55f),new Vector2(1f,0.35f),1.7f);
            h.cards=new XRGrabInteractable[3];
            string[] names={"AYER 18:00\nOFFLINE\nVERIFICADO","HOY 10:15\nPOSTINCIDENTE\nSIN VERIFICAR","SINCRONIZADO\nRED AFECTADA\nSIN VERIFICAR"};
            for(int i=0;i<3;i++)
            {
                var card=Cube("Tarjeta / "+i,new Vector3(1.72f+i*0.3f,0.86f,1.15f),new Vector3(0.24f,0.065f,0.32f),i==0?cyan:amber);
                var rb=card.AddComponent<Rigidbody>();rb.mass=0.15f;rb.collisionDetectionMode=CollisionDetectionMode.ContinuousDynamic;
                var grab=card.AddComponent<XRGrabInteractable>();grab.throwOnDetach=false;h.cards[i]=grab;
                var label=Label(names[i],card.transform.position+new Vector3(0,0.04f,0),new Vector2(0.22f,0.3f),0.65f);
                label.transform.rotation=Quaternion.Euler(90,0,0);label.transform.SetParent(card.transform,true);
            }
            AddButton("SOLICITAR PISTA",new Vector3(-1.1f,1.15f,1.6f),game.ShowHint);
            AddButton("RECUPERAR TARJETAS",new Vector3(2.5f,0.5f,0.8f),h.ReturnCards);
            game.choices[5].GetComponentInChildren<TMP_Text>().text="C · Usa los dos controles de la mesa\nDesconecta Ethernet y Wi-Fi en el módulo PC-02.";
            for(int i=6;i<9;i++)game.choices[i].GetComponentInChildren<TMP_Text>().text+="\nToma la tarjeta y suéltala en el lector.";
            GameObject.Find("02 / AISLAR").transform.Find("Instrucción").GetComponent<TMP_Text>().text="PC-02 está comprometido. Usa los controles ETHERNET y WI-FI de la mesa para aislar ambas conexiones.";
            GameObject.Find("03 / RESTAURAR").transform.Find("Instrucción").GetComponent<TMP_Text>().text="Toma una tarjeta con el botón de agarre y suéltala sobre el lector cian. El incidente ocurrió hoy a las 10:00.";
            var a=game.GetComponent<CyberShieldAtmosphere>();
            foreach(var t in a.briefing.GetComponentsInChildren<TMP_Text>())if(t.text.StartsWith("Apunta"))t.text="Gatillo: seleccionar. Agarre: tomar tarjetas. Suelta sobre el lector.\nTeletransporte para acercarte. Pistas disponibles sin penalización.";
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            return Path;
        }
        static GameObject Cube(string name,Vector3 pos,Vector3 size,Material mat)
        {var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.position=pos;go.transform.localScale=size;go.GetComponent<Renderer>().sharedMaterial=mat;return go;}
        static TMP_Text Label(string text,Vector3 pos,Vector2 size,float font)
        {var go=new GameObject(text,typeof(TextMeshPro));go.transform.position=pos;var t=go.GetComponent<TextMeshPro>();t.text=text;t.font=TMP_Settings.defaultFontAsset;t.fontSize=font;t.enableAutoSizing=true;t.fontSizeMin=0.4f;t.fontSizeMax=font;t.rectTransform.sizeDelta=size;t.alignment=TextAlignmentOptions.Center;return t;}
        static void AddButton(string title,Vector3 pos,UnityEngine.Events.UnityAction action)
        {
            var go=new GameObject(title,typeof(RectTransform),typeof(Canvas),typeof(TrackedDeviceGraphicRaycaster),typeof(UnityEngine.UI.Image),typeof(UnityEngine.UI.Button));
            go.GetComponent<Canvas>().renderMode=RenderMode.WorldSpace;go.transform.position=pos;go.transform.localScale=Vector3.one*0.0015f;
            go.GetComponent<RectTransform>().sizeDelta=new Vector2(650,110);go.GetComponent<UnityEngine.UI.Image>().color=new Color(0.04f,0.3f,0.4f);
            var b=go.GetComponent<UnityEngine.UI.Button>();b.targetGraphic=go.GetComponent<UnityEngine.UI.Image>();UnityEventTools.AddPersistentListener(b.onClick,action);
            var label=new GameObject("Texto",typeof(RectTransform),typeof(TextMeshProUGUI));label.transform.SetParent(go.transform,false);
            var t=label.GetComponent<TextMeshProUGUI>();t.text=title;t.font=TMP_Settings.defaultFontAsset;t.fontSize=30;t.raycastTarget=false;t.alignment=TextAlignmentOptions.Center;
            t.rectTransform.anchorMin=Vector2.zero;t.rectTransform.anchorMax=Vector2.one;t.rectTransform.sizeDelta=Vector2.zero;
        }
    }
}
