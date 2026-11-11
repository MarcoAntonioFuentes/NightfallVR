using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace CyberShield.Editor
{
    public static class CyberShieldCorporateBuilder
    {
        public const string Path="Assets/CyberShield/Scenes/CyberShieldVR_Nexora.unity";
        static Transform office;
        static Material oak,metal,fabric,ivory,leaf,soil,glass,city,screen,paper;
        [MenuItem("CyberShield/Crear oficina Nexora")]
        public static string Build()
        {
            if(AssetDatabase.LoadAssetAtPath<SceneAsset>(Path))throw new Exception("Nexora ya existe.");
            var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if(scene.path!=CyberShieldHandsOnBuilder.Path)throw new Exception("Abre HandsOn primero.");
            EditorSceneManager.SaveScene(scene);EditorSceneManager.SaveScene(scene,Path,true);scene=EditorSceneManager.OpenScene(Path);
            System.IO.Directory.CreateDirectory("Assets/CyberShield/Materials/Nexora");
            office=new GameObject("NEXORA / Oficina corporativa").transform;
            oak=Mat("Roble",new Color(0.55f,0.34f,0.18f));metal=Mat("Aluminio",new Color(0.3f,0.34f,0.38f),0.6f);
            fabric=Mat("Tapizado",new Color(0.065f,0.12f,0.17f));ivory=Mat("Pintura",new Color(0.75f,0.78f,0.77f));
            leaf=Mat("Hojas",new Color(0.12f,0.3f,0.12f));soil=Mat("Tierra",new Color(0.07f,0.045f,0.025f));
            glass=Mat("Cielo ventanas",new Color(0.55f,0.76f,0.88f),0,true);city=Mat("Edificios",new Color(0.34f,0.43f,0.5f));
            screen=Mat("Pantalla",new Color(0.025f,0.07f,0.11f));paper=Mat("Papel",new Color(0.85f,0.85f,0.8f));
            var carpet=Mat("Moqueta",new Color(0.22f,0.25f,0.28f));
            var old=GameObject.Find("BLACKOUT · Arquitectura y señalética");if(old)old.SetActive(false);
            foreach(var r in GameObject.Find("Sala informática").GetComponentsInChildren<Renderer>())
            {
                if(r.name.StartsWith("Pared"))r.sharedMaterial=ivory;
                else if(r.name.StartsWith("Suelo"))r.sharedMaterial=carpet;
                else if(r.name.StartsWith("Mesa"))r.sharedMaterial=oak;
                else if(r.name.StartsWith("Soporte"))r.sharedMaterial=metal;
                else if(r.name.StartsWith("Franja"))r.gameObject.SetActive(false);
            }
            foreach(string name in new[]{"Mesa de respaldo","Módulo físico PC-02"})GameObject.Find(name).GetComponent<Renderer>().sharedMaterial=oak;
            var main=GameObject.Find("Luz principal").GetComponent<Light>();main.intensity=1.1f;main.color=new Color(1,0.94f,0.84f);
            main.transform.rotation=Quaternion.Euler(48,-35,0);main.shadows=LightShadows.Soft;main.shadowStrength=0.65f;
            RenderSettings.ambientLight=new Color(0.45f,0.49f,0.53f);
            Cube("Techo acústico",new Vector3(0,4.4f,0),new Vector3(10,0.12f,10),ivory);
            for(int x=-4;x<=4;x+=2)for(int z=-4;z<=4;z+=2)
            {
                Cube("Junta de techo",new Vector3(x,4.32f,z),new Vector3(1.97f,0.018f,1.97f),paper,false);
                if(z%4==0)Cube("Panel LED",new Vector3(x,4.29f,z),new Vector3(1.1f,0.025f,0.35f),glass,false);
            }
            // Carpet seams, skirting and wall acoustic panels establish office scale.
            for(int i=-4;i<=4;i++)
            {
                Cube("Unión moqueta",new Vector3(i,0.003f,0),new Vector3(0.008f,0.005f,9.7f),metal,false);
                Cube("Unión moqueta",new Vector3(0,0.003f,i),new Vector3(9.7f,0.005f,0.008f),metal,false);
            }
            for(int s=-1;s<=1;s+=2)
            {
                Cube("Zócalo",new Vector3(s*4.78f,0.1f,0),new Vector3(0.04f,0.2f,9.6f),metal,false);
                // Inset window panels with a layered skyline and mullions.
                for(int n=0;n<3;n++)
                {
                    float z=-2.8f+n*2.2f;
                    Cube("Ventana panorámica",new Vector3(s*4.77f,2.6f,z),new Vector3(0.035f,2f,1.8f),glass,false);
                    for(int b=0;b<5;b++)
                    {
                        float h=0.3f+((b+n*2)%4)*0.15f;
                        Cube("Perfil urbano",new Vector3(s*4.73f,1.63f+h*0.5f,z-0.72f+b*0.34f),new Vector3(0.02f,h,0.25f),city,false);
                    }
                    Cube("Marco vertical",new Vector3(s*4.68f,2.6f,z),new Vector3(0.08f,2.1f,0.055f),metal,false);
                    foreach(float y in new[]{1.58f,3.62f})Cube("Marco horizontal",new Vector3(s*4.68f,y,z),new Vector3(0.09f,0.065f,1.9f),metal,false);
                }
            }
            // Workstations behind the training zone leave the central aisle open.
            for(int i=0;i<2;i++)
            {
                float x=i==0?-2.8f:2.8f;
                Cube("Mesa de oficina",new Vector3(x,0.75f,-2.9f),new Vector3(2.4f,0.065f,0.95f),oak);
                foreach(float dx in new[]{-1f,1f})Cube("Pata metálica",new Vector3(x+dx,0.36f,-2.9f),new Vector3(0.055f,0.72f,0.7f),metal);
                Monitor(new Vector3(x,0.8f,-3.15f),i==0?"NEXORA / MESA DE AYUDA\n12 tickets resueltos\n3 solicitudes en espera":"NEXORA / CONTABILIDAD\nCierre del mes\nAcceso restringido");
                Cube("Teclado",new Vector3(x,0.8f,-2.7f),new Vector3(0.55f,0.025f,0.19f),fabric);
                Cube("Ratón",new Vector3(x+0.42f,0.81f,-2.68f),new Vector3(0.08f,0.04f,0.12f),metal);
                Chair(new Vector3(x,0,-1.9f));
                Cylinder("Taza",new Vector3(x-0.8f,0.88f,-2.7f),new Vector3(0.09f,0.08f,0.09f),ivory);
                Cylinder("Café",new Vector3(x-0.8f,0.961f,-2.7f),new Vector3(0.072f,0.003f,0.072f),soil);
                Cube("Cuaderno",new Vector3(x+0.83f,0.8f,-2.7f),new Vector3(0.22f,0.02f,0.3f),paper);
                Label(i==0?"MARTA / SOPORTE TI":"LUCÍA / CONTABILIDAD",new Vector3(x-0.7f,0.86f,-3.35f),new Vector2(0.7f,0.15f),0.85f,new Color(0.05f,0.12f,0.18f));
            }
            Plant(new Vector3(-4.1f,0,3.9f));Plant(new Vector3(4.1f,0,-4.1f));Plant(new Vector3(-4.1f,0,-4.1f));
            Cube("Armario de archivo",new Vector3(3.7f,0.9f,4.3f),new Vector3(1.2f,1.8f,0.55f),ivory);
            for(int i=0;i<3;i++){Cube("Separación archivador",new Vector3(3.7f,0.35f+i*0.55f,4.01f),new Vector3(1.1f,0.018f,0.02f),metal,false);Cube("Tirador",new Vector3(3.7f,0.55f+i*0.55f,3.98f),new Vector3(0.25f,0.025f,0.04f),metal,false);}
            Cube("Impresora",new Vector3(-3.8f,0.65f,3.9f),new Vector3(0.8f,1.3f,0.65f),ivory);
            Cube("Bandeja impresora",new Vector3(-3.8f,1.34f,3.9f),new Vector3(0.65f,0.1f,0.5f),fabric);
            Cube("Puerta",new Vector3(0,1.45f,-4.77f),new Vector3(1.7f,2.9f,0.08f),oak);
            Cube("Manilla",new Vector3(0.6f,1.1f,-4.68f),new Vector3(0.18f,0.04f,0.05f),metal,false);
            var exit=Label("SALIDA / RECEPCIÓN",new Vector3(0,3.1f,-4.65f),new Vector2(2.8f,0.3f),2,new Color(0.05f,0.3f,0.2f));exit.transform.rotation=Quaternion.Euler(0,180,0);
            Label("NEXORA",new Vector3(0,3.95f,4.72f),new Vector2(3f,0.45f),4,new Color(0.02f,0.19f,0.28f));
            Label("TECNOLOGÍA QUE CUIDA TU OPERACIÓN",new Vector3(0,3.6f,4.72f),new Vector2(6f,0.25f),1.6f,new Color(0.1f,0.25f,0.3f));
            // Business incident context and actual evidence inspection.
            var game=UnityEngine.Object.FindFirstObjectByType<CyberShieldGame>();var hands=game.GetComponent<CyberShieldHandsOn>();
            var scenario=game.gameObject.AddComponent<CyberShieldCorporateScenario>();scenario.game=game;scenario.hands=hands;game.corporateScenario=scenario;
            var evidence=Panel("Expediente de correo",new Vector3(-2.5f,1.1f,1.25f),new Vector2(1050,650),0.0015f);
            evidence.rotation=Quaternion.Euler(18,0,0);
            scenario.evidenceText=Text(evidence,"EXPEDIENTE CS-1042\nInspecciona al menos dos evidencias antes de reportar.",29,300);
            string[] clues={"INSPECCIONAR REMITENTE","INSPECCIONAR ENLACE","CONSULTAR CONTEXTO"};
            for(int i=0;i<3;i++)UnityEventTools.AddIntPersistentListener(Button(evidence,clues[i],80).onClick,scenario.InspectEvidence,i);
            // Scan before restore: a separate, non-overlapping physical zone.
            var table=GameObject.Find("Mesa de respaldo");table.transform.localScale=new Vector3(2.3f,0.1f,1.5f);table.transform.position=new Vector3(2.45f,0.76f,0.95f);
            var scanner=Cube("Bandeja de análisis",new Vector3(1.7f,0.85f,0.5f),new Vector3(0.5f,0.08f,0.46f),Mat("Ámbar escáner",new Color(0.7f,0.35f,0.04f)));
            scenario.scanZone=new GameObject("Zona de análisis").transform;scenario.scanZone.position=new Vector3(1.7f,0.86f,0.5f);
            var scanPanel=Panel("Diagnóstico respaldo",new Vector3(2.45f,1.4f,1.6f),new Vector2(1000,350),0.0013f);
            scenario.scannerText=Text(scanPanel,"ANALIZADOR OFFLINE\nSuelta la tarjeta en la bandeja ámbar antes de restaurar.",29,295);
            hands.readerLabel.transform.position=new Vector3(3.15f,1.06f,0.95f);hands.readerLabel.fontSizeMax=1.1f;hands.readerLabel.rectTransform.sizeDelta=new Vector2(0.7f,0.25f);
            GameObject.Find("RECUPERAR TARJETAS").transform.position=new Vector3(2.5f,0.55f,0.15f);
            GameObject.Find("SOLICITAR PISTA").transform.position=new Vector3(-1.12f,0.88f,1.1f);
            var business=Panel("Servicios corporativos",new Vector3(-3.25f,2.85f,3.2f),new Vector2(1100,500),0.0017f);
            scenario.serviceText=Text(business,"NEXORA / SERVICIOS",32,440);
            var chat=Panel("Comunicaciones del equipo",new Vector3(3.25f,2.85f,3.2f),new Vector2(1100,500),0.0017f);
            scenario.colleagueText=Text(chat,"MARTA / COORDINACIÓN TI",32,440);
            scenario.alertLamp=Cube("Estado del incidente",new Vector3(0,2.5f,2.8f),new Vector3(0.12f,0.05f,0.05f),screen,false).GetComponent<Renderer>();
            GameObject.Find("Telemetría de incidente").SetActive(false);GameObject.Find("Estado del respaldo").SetActive(false);
            // The old atmosphere still drives game feedback; its hidden objects remain referenced.
            var atmosphere=game.GetComponent<CyberShieldAtmosphere>();
            foreach(var t in atmosphere.briefing.GetComponentsInChildren<TMP_Text>())
            {
                if(t.text.Contains("PROTOCOLO"))t.text="NEXORA\nINCIDENTE 1042";
                if(t.text.StartsWith("10:00"))t.text="Lunes, 10:00. Contabilidad reporta un correo extraño y PC-02 presenta actividad sospechosa. Eres parte del equipo de TI.";
                if(t.text.StartsWith("01"))t.text="01  INVESTIGA las evidencias del correo\n02  CONTÉN el equipo comprometido\n03  ANALIZA y RESTAURA una copia limpia";
            }
            GameObject.Find("03 / RESTAURAR").transform.Find("Instrucción").GetComponent<TMP_Text>().text="Primero suelta la tarjeta sobre la bandeja ÁMBAR para analizarla. Después llévala al lector CIAN. El incidente fue hoy a las 10:00.";
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            var scenes=EditorBuildSettings.scenes.ToList();scenes.Add(new EditorBuildSettingsScene(Path,true));EditorBuildSettings.scenes=scenes.ToArray();
            return Path;
        }
        static Material Mat(string name,Color color,float metallic=0,bool unlit=false)
        {var m=new Material(Shader.Find(unlit?"Universal Render Pipeline/Unlit":"Universal Render Pipeline/Lit"));m.SetColor("_BaseColor",color);if(!unlit){m.SetFloat("_Metallic",metallic);m.SetFloat("_Smoothness",0.25f);}AssetDatabase.CreateAsset(m,"Assets/CyberShield/Materials/Nexora/"+name+".mat");return m;}
        static GameObject Cube(string name,Vector3 p,Vector3 s,Material m,bool collide=true)
        {var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(office);g.transform.position=p;g.transform.localScale=s;g.GetComponent<Renderer>().sharedMaterial=m;if(!collide)UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());return g;}
        static GameObject Cylinder(string name,Vector3 p,Vector3 s,Material m)
        {var g=GameObject.CreatePrimitive(PrimitiveType.Cylinder);g.name=name;g.transform.SetParent(office);g.transform.position=p;g.transform.localScale=s;g.GetComponent<Renderer>().sharedMaterial=m;UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());return g;}
        static void Chair(Vector3 p)
        {
            Cube("Asiento ergonómico",p+new Vector3(0,0.49f,0),new Vector3(0.55f,0.11f,0.5f),fabric);
            var back=Cube("Respaldo",p+new Vector3(0,0.83f,-0.23f),new Vector3(0.5f,0.65f,0.085f),fabric);back.transform.rotation=Quaternion.Euler(-8,0,0);
            Cylinder("Pistón silla",p+new Vector3(0,0.23f,0),new Vector3(0.065f,0.2f,0.065f),metal);
            for(int i=0;i<5;i++){float a=i*Mathf.PI*2/5;var foot=Cube("Base silla",p+new Vector3(Mathf.Sin(a)*0.15f,0.08f,Mathf.Cos(a)*0.15f),new Vector3(0.04f,0.04f,0.35f),metal,false);foot.transform.rotation=Quaternion.Euler(0,i*72,0);Cylinder("Rueda",p+new Vector3(Mathf.Sin(a)*0.3f,0.045f,Mathf.Cos(a)*0.3f),new Vector3(0.06f,0.035f,0.06f),fabric);}
            foreach(float x in new[]{-0.32f,0.32f}){Cube("Brazo silla",p+new Vector3(x,0.65f,0),new Vector3(0.06f,0.05f,0.35f),fabric,false);}
        }
        static void Monitor(Vector3 p,string text)
        {Cube("Base monitor",p,new Vector3(0.3f,0.035f,0.2f),metal,false);Cube("Soporte monitor",p+new Vector3(0,0.18f,0),new Vector3(0.05f,0.35f,0.05f),metal,false);Cube("Monitor de oficina",p+new Vector3(0,0.4f,0),new Vector3(0.8f,0.45f,0.045f),fabric);Cube("Display",p+new Vector3(0,0.4f,0.027f),new Vector3(0.75f,0.4f,0.008f),screen,false);var t=Label(text,p+new Vector3(0,0.4f,0.035f),new Vector2(0.68f,0.33f),0.85f,Color.white);t.transform.rotation=Quaternion.Euler(0,180,0);}
        static void Plant(Vector3 p)
        {Cylinder("Maceta",p+new Vector3(0,0.22f,0),new Vector3(0.4f,0.22f,0.4f),ivory);Cylinder("Tierra",p+new Vector3(0,0.445f,0),new Vector3(0.34f,0.01f,0.34f),soil);for(int i=0;i<7;i++){float a=i*Mathf.PI*2/7;var l=GameObject.CreatePrimitive(PrimitiveType.Sphere);l.name="Hoja ornamental";l.transform.SetParent(office);l.transform.position=p+new Vector3(Mathf.Sin(a)*0.18f,0.75f+(i%3)*0.15f,Mathf.Cos(a)*0.18f);l.transform.localScale=new Vector3(0.12f,0.65f,0.22f);l.transform.rotation=Quaternion.Euler(25,i*51,20);l.GetComponent<Renderer>().sharedMaterial=leaf;UnityEngine.Object.DestroyImmediate(l.GetComponent<Collider>());}}
        static TMP_Text Label(string value,Vector3 p,Vector2 size,float font,Color c)
        {var g=new GameObject(value,typeof(TextMeshPro));g.transform.SetParent(office);g.transform.position=p;var t=g.GetComponent<TextMeshPro>();t.text=value;t.font=TMP_Settings.defaultFontAsset;t.color=c;t.fontSize=font;t.enableAutoSizing=true;t.fontSizeMax=font;t.fontSizeMin=0.4f;t.rectTransform.sizeDelta=size;t.alignment=TextAlignmentOptions.Center;return t;}
        static Transform Panel(string name,Vector3 p,Vector2 size,float scale)
        {var g=new GameObject(name,typeof(RectTransform),typeof(Canvas),typeof(TrackedDeviceGraphicRaycaster),typeof(UnityEngine.UI.Image),typeof(UnityEngine.UI.VerticalLayoutGroup));g.transform.position=p;g.transform.localScale=Vector3.one*scale;g.GetComponent<RectTransform>().sizeDelta=size;g.GetComponent<Canvas>().renderMode=RenderMode.WorldSpace;g.GetComponent<UnityEngine.UI.Image>().color=new Color(0.035f,0.085f,0.12f);g.GetComponent<UnityEngine.UI.Image>().raycastTarget=false;var l=g.GetComponent<UnityEngine.UI.VerticalLayoutGroup>();l.padding=new RectOffset(24,24,18,18);l.spacing=10;l.childControlHeight=l.childControlWidth=true;l.childForceExpandHeight=false;l.childForceExpandWidth=true;return g.transform;}
        static TMP_Text Text(Transform p,string value,float font,float height)
        {var g=new GameObject("Texto",typeof(RectTransform),typeof(TextMeshProUGUI),typeof(UnityEngine.UI.LayoutElement));g.transform.SetParent(p,false);var t=g.GetComponent<TextMeshProUGUI>();t.text=value;t.font=TMP_Settings.defaultFontAsset;t.fontSize=font;t.raycastTarget=false;t.color=Color.white;t.alignment=TextAlignmentOptions.MidlineLeft;g.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight=height;return t;}
        static UnityEngine.UI.Button Button(Transform p,string title,float height)
        {var g=new GameObject(title,typeof(RectTransform),typeof(UnityEngine.UI.Image),typeof(UnityEngine.UI.Button),typeof(UnityEngine.UI.LayoutElement));g.transform.SetParent(p,false);g.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight=height;g.GetComponent<UnityEngine.UI.Image>().color=new Color(0.1f,0.35f,0.43f);var b=g.GetComponent<UnityEngine.UI.Button>();b.targetGraphic=g.GetComponent<UnityEngine.UI.Image>();var t=Text(g.transform,title,27,height);t.alignment=TextAlignmentOptions.Center;t.rectTransform.anchorMin=Vector2.zero;t.rectTransform.anchorMax=Vector2.one;t.rectTransform.sizeDelta=Vector2.zero;return b;}
    }
}
