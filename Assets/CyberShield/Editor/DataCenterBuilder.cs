using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CyberShield.DataCenter;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CyberShield.Editor
{
    [InitializeOnLoad]
    public static class DataCenterBuilder
    {
        static DataCenterBuilder()
        {
            if (!AssetDatabase.IsAssetImportWorkerProcess()) EditorApplication.update += MonitorValidation;
        }
        static int validationFrames;
        static void MonitorValidation()
        {
            if (!SessionState.GetBool("DataCenter.Validate",false) || !EditorApplication.isPlaying || EditorApplication.isCompiling) return;
            if (++validationFrames < 10) return;
            SessionState.SetBool("DataCenter.Validate",false);
            RunChecks();
        }
        public const string ScenePath = "Assets/CyberShield/Scenes/DataCenter_VR.unity";
        const string Folder = "Assets/CyberShield/DataCenter";
        static Material metal, wall, mint, cyan, ink, white;
        static DataCenterSimulation sim;
        static List<Renderer> lights;
        static List<DataCenterAction> actions;
        static readonly Color Green = new Color(.16f,1,.62f);

        [MenuItem("CyberShield/Centro de datos/Crear escena DataCenter VR")]
        public static void Build()
        {
            if (File.Exists(ScenePath))
            {
                var loaded=SceneManager.GetSceneByPath(ScenePath);
                if(loaded.IsValid()&&loaded.isLoaded) { SceneManager.SetActiveScene(loaded); return; }
                var prior=SceneManager.GetActiveScene();
                var opened=EditorSceneManager.OpenScene(ScenePath,string.IsNullOrEmpty(prior.path)&&!prior.isDirty ? OpenSceneMode.Single : OpenSceneMode.Additive);
                SceneManager.SetActiveScene(opened);
                for(int i=SceneManager.sceneCount-1;i>=0;i--){var old=SceneManager.GetSceneAt(i);if(old!=opened&&!old.isDirty)EditorSceneManager.CloseScene(old,true);}
                return;
            }
            Directory.CreateDirectory(Folder + "/Materials");
            AssetDatabase.Refresh();
            var current = SceneManager.GetActiveScene();
            var mode = string.IsNullOrEmpty(current.path) && (!current.isDirty || Application.isBatchMode) ? NewSceneMode.Single : NewSceneMode.Additive;
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, mode);
            SceneManager.SetActiveScene(scene);
            metal = Mat("Grafito", new Color(.035f,.055f,.07f));
            wall = Mat("Arquitectura", new Color(.07f,.115f,.13f));
            ink = Mat("Pantallas", new Color(.008f,.025f,.03f), true);
            mint = Mat("Neón jade", Green, true); cyan = Mat("Neón hielo", new Color(.05f,.7f,1), true);
            white = Mat("Cerámica", new Color(.65f,.75f,.72f));
            lights = new List<Renderer>(); actions = new List<DataCenterAction>();
            sim = new GameObject("DATA CENTER / director de simulación").AddComponent<DataCenterSimulation>();
            sim.narrator = Source("Asistente / narración", .9f); sim.narrator.pitch = .86f;
            sim.ambience = Source("Ambiente / ventilación", .6f); sim.alarm = Source("Alarma / acceso", .32f);
            sim.narration = Enumerable.Range(0,9).Select(i => AssetDatabase.LoadAssetAtPath<AudioClip>(Folder + "/Audio/Asistente_" + i + ".wav")).ToArray();
            sim.player = new GameObject("Operador / teletransporte").transform;
            sim.player.position = new Vector3(0,0,-6.3f);
            var camera = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener)); camera.tag = "MainCamera";
            camera.transform.SetParent(sim.player); camera.transform.localPosition = new Vector3(0,1.65f,0);
            sim.view = camera.GetComponent<Camera>(); sim.view.nearClipPlane = .05f; sim.view.farClipPlane = 60; sim.view.fieldOfView = 78;
            sim.view.clearFlags = CameraClearFlags.SolidColor; sim.view.backgroundColor = new Color(.008f,.02f,.027f);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat; RenderSettings.ambientLight = new Color(.25f,.34f,.37f);
            RenderSettings.fog = true; RenderSettings.fogColor = sim.view.backgroundColor; RenderSettings.fogMode = FogMode.Exponential; RenderSettings.fogDensity = .016f;
            var key = new GameObject("Luz / principal").AddComponent<Light>(); key.type = LightType.Directional; key.intensity = 1.1f; key.color = new Color(.57f,.79f,.85f); key.transform.rotation = Quaternion.Euler(50,-30,0);
            sim.alarmLight = new GameObject("Alarma / luz roja").AddComponent<Light>(); sim.alarmLight.type = LightType.Point; sim.alarmLight.color = Color.red; sim.alarmLight.intensity = 7; sim.alarmLight.range = 12; sim.alarmLight.transform.position = new Vector3(0,3,1); sim.alarmLight.enabled = false;

            Box("Suelo elevado",new Vector3(0,-.12f,0),new Vector3(13,.24f,15),metal);
            Box("Pared fondo",new Vector3(0,2.1f,6),new Vector3(13,4.2f,.2f),wall);
            Box("Pared izquierda",new Vector3(-6.5f,2,0),new Vector3(.2f,4,12),wall);
            Box("Pared derecha",new Vector3(6.5f,2,0),new Vector3(.2f,4,12),wall);
            for(int i=-6;i<=6;i++) Box("Junta del suelo",new Vector3(i,.005f,0),new Vector3(.012f,.008f,14),wall,false);
            for(int i=-7;i<=6;i++) Box("Junta transversal",new Vector3(0,.006f,i),new Vector3(13,.008f,.012f),wall,false);
            foreach(float x in new[]{-5.9f,5.9f}) { Box("Carril luminoso",new Vector3(x,.02f,0),new Vector3(.035f,.02f,13),mint,false); Box("Iluminación superior",new Vector3(x,3.8f,0),new Vector3(.08f,.08f,12),cyan,false); }
            for(int i=0;i<4;i++) Rack(i, new Vector3(i<2?-4.8f:4.8f,0,(i%2)*2.6f-.2f));
            for(int i=0;i<5;i++) Box("Viga de techo",new Vector3(0,4,i*2.6f-5),new Vector3(13,.13f,.18f),metal);

            // Front-facing wall displays use a shared readable canvas scale.
            Text("CENTRO DE DATOS",new Vector3(-2.7f,3.3f,5.82f),new Vector2(1050,150),74,Green);
            Text("03 / OPERACIÓN SWIFT     •     RECONSTRUCCIÓN EDUCATIVA",new Vector3(-2.7f,2.97f,5.81f),new Vector2(1050,90),22,Color.white);
            sim.clock = Text("20:36",new Vector3(3.1f,3.25f,5.8f),new Vector2(600,170),104,Green);
            sim.dashboard = Text("ATAQUE 2016 / EN ESPERA\n00 ÓRDENES / USD 0 M\n04 FEB 2016",new Vector3(2.85f,2.55f,5.8f),new Vector2(760,240),30,Color.white);
            sim.objective = Text("MISIÓN 03 / CENTRO DE DATOS\nInvestiga una sesión nocturna anómala.",new Vector3(0,2.95f,-.7f),new Vector2(1480,270),44,Color.white);
            // Main terminal / left station.
            Box("Mesa SWIFT",new Vector3(-2.7f,.85f,2),new Vector3(3.2f,.13f,1.3f),metal);
            Box("Pedestal SWIFT",new Vector3(-2.7f,.4f,2.1f),new Vector3(.65f,.8f,.65f),wall);
            Box("Terminal_SWIFT",new Vector3(-2.7f,1.85f,2.4f),new Vector3(3.25f,1.7f,.12f),ink);
            sim.logs = Text("SWIFT / AUDITORÍA\n20:36 / LOGIN\n████ HUECO / REGISTRO BORRADO ████",new Vector3(-2.7f,1.95f,2.32f),new Vector2(1470,580),28,Green);
            Button("PRESERVAR LOG / EVIDENCIA",new Vector3(-2.7f,1.17f,2.28f),new Vector2(3,.23f),3);
            // Defense station / right.
            Box("Panel_Controles",new Vector3(2.7f,1.65f,2.4f),new Vector3(3.2f,2.1f,.15f),ink);
            Text("DEFENSA / ZERO TRUST",new Vector3(2.7f,2.52f,2.3f),new Vector2(1400,140),48,Green);
            sim.controls = Text("MFA [INACTIVO]\nHORARIO [SIN ALERTA]\nSIEM [LOCAL]",new Vector3(2.7f,2.03f,2.3f),new Vector2(1350,330),35,Color.white);
            Button("01 / MFA / ACTIVAR",new Vector3(2.7f,1.51f,2.27f),new Vector2(2.85f,.26f),4);
            Button("02 / HORARIO / ACTIVAR",new Vector3(2.7f,1.16f,2.27f),new Vector2(2.85f,.26f),5);
            Button("03 / SIEM / ACTIVAR",new Vector3(2.7f,.81f,2.27f),new Vector2(2.85f,.26f),6);

            Box("Impresora",new Vector3(-3.2f,.65f,4.7f),new Vector3(1.1f,1.3f,.8f),white);
            Box("Salida de papel",new Vector3(-3.2f,.99f,4.28f),new Vector3(.8f,.1f,.03f),metal);
            sim.printerStatus = Text("IMPRESORA / APAGADA\nSin confirmaciones impresas",new Vector3(-3.1f,1.55f,4.22f),new Vector2(1000,210),31,Color.white);
            Button("INSPECCIONAR IMPRESORA",new Vector3(-3.2f,.65f,4.2f),new Vector2(1.7f,.32f),7);
            Box("Switch_Barato",new Vector3(3.3f,.8f,4.6f),new Vector3(1.5f,.38f,.6f),metal);
            for(int i=0;i<8;i++) Box("Puerto de red",new Vector3(2.72f+i*.16f,.82f,4.28f),new Vector3(.09f,.07f,.025f),mint,false);
            Text("SWITCH / RED ACTIVA",new Vector3(3.3f,1.35f,4.2f),new Vector2(1000,120),34,Green);
            Button("INSPECCIONAR SWITCH",new Vector3(3.3f,.45f,4.18f),new Vector2(1.8f,.3f),8);

            // Central holographic assistant: stacked emissive rings and a rotating core.
            var hologram = new GameObject("Asistente / núcleo holográfico").transform; hologram.position = new Vector3(0,1.75f,0); sim.core = hologram;
            for(int r=0;r<5;r++) Ring(hologram, .25f+r*.11f, (r-2)*.12f, r%2==0?mint:cyan);
            var orb = GameObject.CreatePrimitive(PrimitiveType.Sphere); orb.name = "Núcleo"; orb.transform.SetParent(hologram,false); orb.transform.localScale=Vector3.one*.42f; orb.GetComponent<Renderer>().sharedMaterial=cyan; UnityEngine.Object.DestroyImmediate(orb.GetComponent<Collider>());
            var floorRing=new GameObject("Nodo de seguridad / suelo").transform;Ring(floorRing,1.35f,.025f,mint);
            Text("A S I S T E N T E",new Vector3(0,2.35f,-.25f),new Vector2(1000,110),45,Green);
            sim.result = Text("INFORME / PENDIENTE\nCompleta una sesión para comparar resultados.",new Vector3(0,1.1f,5.65f),new Vector2(1400,730),30,Color.white);

            // Entry console: available immediately from the initial position.
            Box("Portal / consola",new Vector3(0,.85f,-2.5f),new Vector3(3.6f,.15f,.5f),metal);
            Button("ATAQUE 2016",new Vector3(-.95f,1.18f,-2.9f),new Vector2(1.7f,.3f),0);
            Button("MODO DEFENSA",new Vector3(.95f,1.18f,-2.9f),new Vector2(1.7f,.3f),1);
            Button("INICIAR SESIÓN",new Vector3(-.95f,.77f,-2.91f),new Vector2(1.7f,.3f),2);
            Button("VER DESENLACE",new Vector3(.95f,.77f,-2.91f),new Vector2(1.7f,.3f),9);
            Text("ARCHIVO 03   /   LA NOCHE DEL ATAQUE",new Vector3(0,3.4f,-2.8f),new Vector2(2200,160),85,Green);
            Text("36 ÓRDENES    ·    USD 951 MILLONES    ·    UNA SESIÓN SIN ALERTA",new Vector3(0,3.1f,-2.8f),new Vector2(2000,100),35,Color.white);
            // Duplicate start and outcome actions near investigation stations.
            Button("INICIAR SESIÓN",new Vector3(-.85f,.5f,1.65f),new Vector2(1.5f,.27f),2);
            Button("VER DESENLACE",new Vector3(.85f,.5f,1.65f),new Vector2(1.5f,.27f),9);
            for(int i=0;i<4;i++) Button(new[]{"01 / PORTAL","02 / TERMINAL","03 / DEFENSA","04 / EVIDENCIAS"}[i],new Vector3(-2.7f+i*1.8f,.12f,-1.45f),new Vector2(1.65f,.2f),10+i);
            // A compact head-mounted instruction/subtitle strip stays legible at every station.
            var hud = new GameObject("Visor / subtítulos",typeof(RectTransform),typeof(Canvas),typeof(UnityEngine.UI.Image));
            hud.GetComponent<Canvas>().renderMode=RenderMode.WorldSpace; hud.transform.SetParent(camera.transform,false);
            hud.transform.localPosition=new Vector3(0,-.82f,1.65f); hud.transform.localScale=Vector3.one*.001f;
            hud.GetComponent<RectTransform>().sizeDelta=new Vector2(1850,290);
            hud.GetComponent<UnityEngine.UI.Image>().color=new Color(.005f,.02f,.025f,.92f);
            hud.GetComponent<UnityEngine.UI.Image>().raycastTarget=false;
            sim.subtitle = HudText(hud.transform,"ASISTENTE / EN LÍNEA",new Vector2(0,40),new Vector2(1780,175),42,Green);
            HudText(hud.transform,"Clic / E: interactuar  ·  Botón derecho: mirar  ·  1–4: teletransporte  ·  R: reiniciar  ·  M: audio\nVR: gatillo o mirada fija 1,8 s sin mando. Usa los destinos del suelo.",new Vector2(0,-100),new Vector2(1780,85),27,Color.white);
            sim.leds=lights.ToArray(); sim.actions=actions.ToArray();
            EditorSceneManager.SaveScene(scene,ScenePath);
            for(int i=SceneManager.sceneCount-1;i>=0;i--)
            {
                var old=SceneManager.GetSceneAt(i);
                if(old!=scene && !old.isDirty) EditorSceneManager.CloseScene(old,true);
            }
            // Existing scenes remain available. New experience is the startup scene for builds.
            EditorBuildSettings.scenes = new[]{new EditorBuildSettingsScene(ScenePath,true)}.Concat(EditorBuildSettings.scenes.Where(s=>s.path!=ScenePath)).ToArray();
            AssetDatabase.SaveAssets();
            Debug.Log("DATACENTER_BUILD_OK " + ScenePath);
        }

        static AudioSource Source(string name,float volume) { var a = new GameObject(name).AddComponent<AudioSource>(); a.playOnAwake=false; a.volume=volume; return a; }
        static Material Mat(string name,Color color,bool emissive=false)
        {
            string path=Folder+"/Materials/"+name+".mat"; var existing=AssetDatabase.LoadAssetAtPath<Material>(path); if(existing)return existing;
            var m=new Material(Shader.Find(emissive?"Universal Render Pipeline/Unlit":"Universal Render Pipeline/Lit")); m.color=color;
            AssetDatabase.CreateAsset(m,path); return m;
        }
        static GameObject Box(string name,Vector3 pos,Vector3 scale,Material mat,bool collider=true)
        {
            var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.position=pos;g.transform.localScale=scale;g.GetComponent<Renderer>().sharedMaterial=mat;
            if(!collider)UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());return g;
        }
        static TMP_Text Text(string content,Vector3 pos,Vector2 size,int font,Color color)
        {
            var panel=new GameObject(content.Split('\n')[0],typeof(RectTransform),typeof(Canvas));panel.GetComponent<Canvas>().renderMode=RenderMode.WorldSpace;
            panel.transform.position=pos; panel.transform.localScale=Vector3.one*.002f;panel.GetComponent<RectTransform>().sizeDelta=size;
            return HudText(panel.transform,content,Vector2.zero,size,font,color);
        }
        static TMP_Text HudText(Transform parent,string content,Vector2 pos,Vector2 size,int font,Color color)
        {
            var g=new GameObject("Texto",typeof(RectTransform),typeof(TextMeshProUGUI));g.transform.SetParent(parent,false);
            var t=g.GetComponent<TextMeshProUGUI>();t.rectTransform.anchoredPosition=pos;t.rectTransform.sizeDelta=size;t.font=TMP_Settings.defaultFontAsset;
            t.text=content;t.fontSize=font;t.color=color;t.alignment=TextAlignmentOptions.Midline;t.raycastTarget=false;
            return t;
        }
        static void Button(string name,Vector3 pos,Vector2 size,int id)
        {
            var g=Box(name,pos,new Vector3(size.x,size.y,.07f),ink);var a=g.AddComponent<DataCenterAction>();a.id=id;a.simulation=sim;
            a.label=Text(name,pos+new Vector3(0,0,-.045f),new Vector2(size.x/.002f-25,size.y/.002f-8),id>=10?34:id==0||id==1||id==2||id==9?48:32,Green);
            actions.Add(a);
        }
        static void Rack(int index,Vector3 pos)
        {
            Box("Rack_0"+(index+1),pos+new Vector3(0,1.4f,0),new Vector3(1.25f,2.8f,1.25f),metal);
            Box("Rack / marco",pos+new Vector3(0,1.4f,-.635f),new Vector3(1.12f,2.65f,.025f),wall);
            for(int u=0;u<9;u++)
            {
                Box("Servidor",pos+new Vector3(0,.25f+u*.28f,-.67f),new Vector3(1.02f,.22f,.05f),metal);
                for(int l=0;l<3;l++) lights.Add(Box("LED",pos+new Vector3(-.42f+l*.095f,.25f+u*.28f,-.705f),new Vector3(.033f,.04f,.015f),mint,false).GetComponent<Renderer>());
                for(int v=0;v<6;v++) Box("Ventilación",pos+new Vector3(.02f+v*.07f,.25f+u*.28f,-.705f),new Vector3(.025f,.12f,.015f),ink,false);
            }
            Text("RACK / 0"+(index+1),pos+new Vector3(0,2.95f,-.65f),new Vector2(600,100),30,Green);
        }
        static void Ring(Transform parent,float radius,float y,Material mat)
        {
            var g=new GameObject("Anillo holográfico");g.transform.SetParent(parent,false);var line=g.AddComponent<LineRenderer>();line.useWorldSpace=false;line.loop=true;line.positionCount=64;line.startWidth=line.endWidth=.012f;line.sharedMaterial=mat;
            for(int i=0;i<64;i++){float a=i*Mathf.PI*2/64;line.SetPosition(i,new Vector3(Mathf.Cos(a)*radius,y,Mathf.Sin(a)*radius));}
        }

        // Batch validation runs the actual scene in Play mode, including audio and interaction code.
        static double deadline;
        [MenuItem("CyberShield/Centro de datos/Validar simulación")]
        public static void Validate()
        {
            if (EditorApplication.isPlaying) return;
            Build();
            if(SceneManager.sceneCount!=1)throw new Exception("Hay otra escena con cambios sin guardar. La escena DataCenter ya está creada; guarda esos cambios antes de validar.");
            SessionState.SetBool("DataCenter.Validate",true);
            deadline=EditorApplication.timeSinceStartup+90;
            EditorApplication.update+=Watchdog; EditorApplication.isPlaying=true;
        }
        static void Watchdog(){if(EditorApplication.timeSinceStartup>deadline){EditorApplication.update-=Watchdog;SessionState.SetBool("DataCenter.Validate",false);Debug.LogError("DATACENTER_TIMEOUT");EndValidation(2);}}
        static void Check(bool condition,string label){if(!condition)throw new Exception(label);Debug.Log("PASS / "+label);}
        static void RunChecks()
        {
            try
            {
                var s=UnityEngine.Object.FindFirstObjectByType<DataCenterSimulation>();
                Check(s&&s.actions.Length>=16&&s.leds.Length==108,"escena, acciones y 108 LEDs");
                Check(s.narration.Length==9&&s.narration.All(c=>c!=null),"nueve narraciones locales");
                var start=s.actions.First(a=>a.id==2);
                var ray=new Ray(s.view.transform.position,start.transform.position-s.view.transform.position);
                Check(Physics.Raycast(ray,out RaycastHit hit,12)&&hit.collider.gameObject==start.gameObject,"botón de inicio accesible por rayo sin obstáculos");
                s.Reset(false);start.Activate();s.Advance(DataCenterSimulation.Duration*45/443);
                Check(s.Orders==18,"primeros 45 minutos: 18 órdenes");
                s.Advance(DataCenterSimulation.Duration*45/443+.001f);Check(s.Orders==36,"90 minutos: 36 órdenes");
                s.Advance(DataCenterSimulation.Duration);Check(s.Complete&&s.clock.text=="03:59","reloj cruza medianoche y termina a 03:59");
                s.Reset(false);s.Act(3);s.Act(7);s.Act(8);s.Act(2);s.Finish();
                Check(s.Orders==36&&s.Evidence&&s.PrinterEvidence&&s.SwitchEvidence,"ataque: 36 órdenes y tres evidencias");
                s.Reset(true);s.Act(4);s.Act(5);s.Act(6);s.Act(2);s.Finish();
                Check(s.Blocked&&s.Orders==0&&s.Siem,"defensa: cero órdenes, MFA, horario y SIEM");
                Check(s.AttackSeen&&s.DefenseSeen,"comparación de ambas sesiones");
                s.Reset(true);s.Act(6);s.Act(2);s.Finish();Check(s.Orders==36&&!s.Blocked,"SIEM solo no bloquea transferencias");
                s.Reset(true);s.Act(4);s.Act(2);s.Act(5);Check(!s.Schedule,"controles bloqueados durante sesión");
                s.Reset(false);s.Act(4);Check(!s.Mfa,"modo ataque no altera controles");
                s.Reset(false);s.Refresh();Canvas.ForceUpdateCanvases();
                Directory.CreateDirectory("Assets/CyberShield/DataCenter/Preview");
                var rt=new RenderTexture(1600,1000,24);s.view.targetTexture=rt;s.view.Render();RenderTexture.active=rt;
                var image=new Texture2D(1600,1000,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1600,1000),0,0);image.Apply();
                File.WriteAllBytes("Assets/CyberShield/DataCenter/Preview/DataCenter.png",image.EncodeToPNG());s.view.targetTexture=null;RenderTexture.active=null;UnityEngine.Object.DestroyImmediate(image);UnityEngine.Object.DestroyImmediate(rt);
                File.WriteAllText("Logs/DataCenter-results.txt","PASS: compilación y ejecución en Unity.\nPASS: 108 LEDs y acciones.\nPASS: nueve narraciones locales.\nPASS: ataque = 36 órdenes y tres evidencias.\nPASS: defensa = 0 órdenes.\nPASS: SIEM solo no bloquea.\nPASS: controles protegidos durante la sesión.\nPASS: comparación entre modos.\n");
                Debug.Log("DATACENTER_VALIDATION_OK");EndValidation(0);
            }
            catch(Exception e){Debug.LogException(e);File.WriteAllText("Logs/DataCenter-results.txt",e.ToString());EndValidation(1);}
        }
        static void EndValidation(int code) { if(Application.isBatchMode)EditorApplication.Exit(code);else EditorApplication.isPlaying=false; }
    }
}
