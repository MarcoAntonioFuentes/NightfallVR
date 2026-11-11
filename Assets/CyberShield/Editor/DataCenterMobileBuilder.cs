using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CyberShield.DataCenter;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Management;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;

namespace CyberShield.Editor
{
    public static class DataCenterMobileBuilder
    {
        public const string Path="Assets/CyberShield/Scenes/DataCenter_Nightfall_Android.unity";
        public const string Folder="Assets/CyberShield/DataCenterMobile";
        static DataCenterMobileMission mission;
        static Material dark,cyan,orange,white;
        static List<DataCenterAction> actions;
        static readonly Color Ice=new Color(.12f,.85f,1);
        static string Asset(string name)=>Folder+"/"+name;

        [MenuItem("CyberShield/Nightfall Android/Crear experiencia móvil")]
        public static void Build()
        {
            if(File.Exists(Path))
            {
                var loaded=SceneManager.GetSceneByPath(Path);
                if(loaded.IsValid()&&loaded.isLoaded){SceneManager.SetActiveScene(loaded);return;}
                var previous=SceneManager.GetActiveScene();
                var opened=EditorSceneManager.OpenScene(Path,string.IsNullOrEmpty(previous.path)&&!previous.isDirty?OpenSceneMode.Single:OpenSceneMode.Additive);
                SceneManager.SetActiveScene(opened);
                for(int i=SceneManager.sceneCount-1;i>=0;i--){var old=SceneManager.GetSceneAt(i);if(old!=opened&&!old.isDirty)EditorSceneManager.CloseScene(old,true);}
                return;
            }
            Directory.CreateDirectory(Folder+"/Materials");Directory.CreateDirectory(Folder+"/Audio");AssetDatabase.Refresh();
            var current=SceneManager.GetActiveScene();
            var scene=EditorSceneManager.OpenScene(DataCenterBuilder.ScenePath,string.IsNullOrEmpty(current.path)&&!current.isDirty?OpenSceneMode.Single:OpenSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            var s=UnityEngine.Object.FindObjectsByType<DataCenterSimulation>(FindObjectsSortMode.None).First(x=>x.gameObject.scene==scene);
            s.enabled=false;mission=s.gameObject.AddComponent<DataCenterMobileMission>();mission.sim=s;
            dark=Mat("Obsidiana",new Color(.012f,.035f,.06f));cyan=Mat("Ion",new Color(.04f,.9f,1));orange=Mat("Intrusión",new Color(1,.16f,.035f));white=Mat("Marfil",new Color(.7f,.9f,1));
            mission.safeMaterial=cyan;mission.dangerMaterial=orange;
            actions=s.actions.ToList();
            foreach(var a in actions.Where(a=>a.id>=10&&a.id<=13).ToArray()){a.gameObject.SetActive(false);a.label.transform.parent.gameObject.SetActive(false);actions.Remove(a);}
            // Retain the original scene and its references. This copy becomes the mobile campaign.
            var oldTexts=UnityEngine.Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(x=>x.gameObject.scene==scene).ToArray();
            var keep=new[]{s.logs,s.printerStatus,s.controls};
            foreach(var t in oldTexts)if(!keep.Contains(t)&&!actions.Any(a=>a.label==t))t.gameObject.SetActive(false);
            s.objective.transform.parent.gameObject.SetActive(false);s.subtitle.transform.parent.gameObject.SetActive(false);
            foreach(var a in actions.Where(a=>a.id==4||a.id==6)){a.gameObject.SetActive(false);a.label.transform.parent.gameObject.SetActive(false);}
            foreach(var a in actions)if(a.label){a.label.fontSize=Mathf.Max(42,a.label.fontSize);a.label.enableAutoSizing=true;a.label.fontSizeMin=28;a.label.fontSizeMax=a.label.fontSize;}
            s.view.fieldOfView=70;s.view.allowHDR=false;s.view.allowMSAA=true;
            foreach(var light in UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None).Where(l=>l.gameObject.scene==scene))light.shadows=LightShadows.None;
            RenderSettings.ambientLight=new Color(.14f,.23f,.31f);RenderSettings.fogDensity=.012f;
            Label("N I G H T F A L L",new Vector3(0,3.42f,-2.8f),new Vector2(2200,190),125,Ice);
            for(int i=0;i<6;i++)
            {
                foreach(float side in new[]{-1f,1f})
                {
                    Box("Pilar estructural",new Vector3(side*6.25f,1.9f,-5+i*2),new Vector3(.22f,3.8f,.24f),dark);
                    Box("Luz vertical",new Vector3(side*6.1f,1.85f,-5+i*2),new Vector3(.025f,2.9f,.04f),i%2==0?cyan:orange,false);
                }
            }
            Box("Cielorraso",new Vector3(0,4.15f,0),new Vector3(13,.12f,15),dark);
            for(int i=0;i<7;i++)
            {
                Box("Aleta superior",new Vector3(0,3.98f,-6+i*1.7f),new Vector3(10,.08f,.3f),dark);
                foreach(float x in new[]{-2.1f,2.1f})Box("Banda superior",new Vector3(x,3.9f,-6+i*1.7f),new Vector3(2,.025f,.05f),cyan,false);
            }
            // Cyan chevrons mark the route and survive low mobile render quality.
            for(int i=0;i<10;i++)foreach(float side in new[]{-1f,1f})
            {
                var g=Box("Ruta de evacuación",new Vector3(side*.35f,.025f,-5.5f+i*.9f),new Vector3(.045f,.012f,.38f),cyan,false);g.transform.rotation=Quaternion.Euler(0,side*35,0);
            }
            // Physical network cable, paper evidence and a recognizable printer control.
            var cableRoot=new GameObject("Conector de contención").transform;cableRoot.position=new Vector3(3.3f,.8f,4.2f);
            mission.cablePlug=new GameObject("Conector extraíble").transform;mission.cablePlug.SetParent(cableRoot,false);
            var plug=Box("Ethernet rojo",cableRoot.position,new Vector3(.28f,.18f,.45f),orange);plug.transform.SetParent(mission.cablePlug,true);
            var plugAction=plug.AddComponent<DataCenterAction>();plugAction.id=8;plugAction.simulation=s;actions.Add(plugAction);
            var cable=Line("Enlace comprometido",orange,new[]{new Vector3(3.3f,.8f,4.2f),new Vector3(3.3f,.2f,3.4f),new Vector3(4.2f,.2f,3.4f)},.035f);cable.transform.SetParent(mission.cablePlug,true);
            mission.paper=Box("Evidencia / confirmación",new Vector3(-3.2f,1.08f,4.25f),new Vector3(.5f,.012f,.4f),white,false).transform;mission.paper.gameObject.SetActive(false);
            for(int i=0;i<5;i++)Box("Tecla SWIFT",new Vector3(-3.3f+i*.28f,.94f,1.65f),new Vector3(.22f,.035f,.16f),i==4?cyan:dark,false);
            // Three large pads serve the memory and network connection tasks.
            mission.memoryPads=new Transform[3];mission.padFaces=new Renderer[3];
            for(int i=0;i<3;i++)
            {
                var pad=Button((i+1).ToString(),new Vector3(1.8f+i*.9f,1.42f,2.12f),new Vector2(.7f,.7f),20+i);
                actions.Last().label.fontSize=220;actions.Last().label.fontSizeMax=220;
                mission.memoryPads[i]=pad.transform;mission.padFaces[i]=pad.GetComponent<Renderer>();
                var ring=Circle("Halo de nodo",cyan,.43f);ring.transform.position=pad.transform.position+Vector3.back*.07f;ring.transform.rotation=Quaternion.Euler(90,0,0);
            }
            mission.combo=Label("",new Vector3(2.7f,2.15f,2.08f),new Vector2(1450,180),60,Ice);
            var timing=new GameObject("Reto / ventana de bloqueo").transform;timing.position=new Vector3(2.7f,1.55f,2.04f);
            var track=Box("Carril",timing.position,new Vector3(2.4f,.13f,.04f),dark,false);track.transform.SetParent(timing,true);
            var target=Box("Ventana segura",timing.position,new Vector3(.96f,.16f,.05f),cyan,false);target.transform.SetParent(timing,true);
            var cursor=Box("Pulso móvil",timing.position+Vector3.back*.06f,new Vector3(.08f,.3f,.06f),orange,false);cursor.transform.SetParent(timing,true);mission.sweep=cursor.transform;
            var blockButton=actions.First(a=>a.id==5);blockButton.transform.position=new Vector3(2.7f,1,2.05f);blockButton.label.transform.parent.position=blockButton.transform.position+Vector3.back*.05f;
            // A real 3D shield around the assistant appears after the player's defenses are active.
            mission.shield=new GameObject("Escudo de defensa").transform;mission.shield.position=new Vector3(0,1.75f,0);
            for(int i=0;i<3;i++){var ring=Circle("Anillo de escudo",cyan,1.12f);ring.transform.SetParent(mission.shield,false);ring.transform.localRotation=Quaternion.Euler(i*60,i*35,0);}
            mission.shield.gameObject.SetActive(false);
            mission.network=Line("Transferencias SWIFT",orange,new[]{new Vector3(-2.7f,2.15f,2.2f),new Vector3(0,3.4f,3.7f),new Vector3(3.2f,3.1f,5.6f)},.012f);
            mission.packets=new Transform[24];for(int i=0;i<24;i++){var p=Box("Paquete de datos",Vector3.zero,Vector3.one*.075f,orange,false);mission.packets[i]=p.transform;}
            var dust=new GameObject("Atmósfera / motas de luz").AddComponent<ParticleSystem>();dust.transform.position=new Vector3(0,1.9f,0);
            var main=dust.main;main.maxParticles=90;main.startLifetime=7;main.startSpeed=.025f;main.startSize=.018f;main.startColor=new Color(.08f,.7f,1,.3f);
            var emission=dust.emission;emission.rateOverTime=10;var shape=dust.shape;shape.shapeType=ParticleSystemShapeType.Box;shape.scale=new Vector3(11,3,12);
            var dustMat=new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));dustMat.SetFloat("_Surface",1);dustMat.SetFloat("_SrcBlend",(float)BlendMode.SrcAlpha);dustMat.SetFloat("_DstBlend",(float)BlendMode.One);dustMat.SetFloat("_ZWrite",0);dustMat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");dustMat.renderQueue=3000;
            var existingDust=AssetDatabase.LoadAssetAtPath<Material>(Asset("Materials/Atmósfera.mat"));
            if(existingDust){UnityEngine.Object.DestroyImmediate(dustMat);dustMat=existingDust;}else AssetDatabase.CreateAsset(dustMat,Asset("Materials/Atmósfera.mat"));
            dust.GetComponent<ParticleSystemRenderer>().sharedMaterial=dustMat;
            // HUD is world-space, so it works in stereo rather than being drawn in only one eye.
            mission.headline=Hud("OPERACIÓN NIGHTFALL",s.view,new Vector3(0,.46f,1.45f),new Vector2(1350,120),58,Ice);
            mission.stats=Hud("0000 PTS",s.view,new Vector3(0,.34f,1.45f),new Vector2(1100,80),35,Color.white);
            mission.hint=Hud("Mira y toca para comenzar",s.view,new Vector3(0,-.35f,1.45f),new Vector2(1300,145),37,Color.white);
            mission.status=Hud("ANDROID",s.view,new Vector3(0,-.47f,1.45f),new Vector2(1200,60),24,Ice);
            mission.navigationAnchor=new GameObject("Navegación / anclaje accesible en VR").transform;mission.navigationAnchor.SetParent(s.view.transform,false);
            for(int i=0;i<4;i++)
            {
                var b=Button(new[]{"PORTAL","LOG","ESCUDO","PRUEBAS"}[i],s.view.transform.TransformPoint(new Vector3(-.48f+i*.32f,-.6f,1.45f)),new Vector2(.29f,.075f),10+i);
                b.transform.SetParent(mission.navigationAnchor,true);actions.Last().label.transform.parent.SetParent(mission.navigationAnchor,true);
            }
            var audio=Button("AUDIO",s.view.transform.TransformPoint(new Vector3(-.35f,.62f,1.65f)),new Vector2(.24f,.065f),14);audio.transform.SetParent(mission.navigationAnchor,true);actions.Last().label.transform.parent.SetParent(mission.navigationAnchor,true);
            var center=Button("CENTRAR",s.view.transform.TransformPoint(new Vector3(0,.62f,1.65f)),new Vector2(.27f,.065f),15);center.transform.SetParent(mission.navigationAnchor,true);actions.Last().label.transform.parent.SetParent(mission.navigationAnchor,true);
            var sub=Button("SUBS",s.view.transform.TransformPoint(new Vector3(.35f,.62f,1.65f)),new Vector2(.24f,.065f),16);sub.transform.SetParent(mission.navigationAnchor,true);actions.Last().label.transform.parent.SetParent(mission.navigationAnchor,true);
            var reticle=Circle("Retícula",cyan,.012f);reticle.transform.SetParent(s.view.transform,false);reticle.transform.localPosition=new Vector3(0,0,1);reticle.transform.localRotation=Quaternion.Euler(90,0,0);reticle.startWidth=reticle.endWidth=.0015f;mission.reticle=reticle;
            var dwell=Line("Progreso de mirada",cyan,new[]{Vector3.zero,Vector3.zero},.003f);dwell.useWorldSpace=false;dwell.transform.SetParent(s.view.transform,false);dwell.transform.localPosition=new Vector3(0,0,1);mission.dwellRing=dwell;
            mission.rating=Label("",new Vector3(0,2.25f,1.5f),new Vector2(1350,950),85,Ice);
            mission.music=Audio("Música / Nightfall",null,.16f);mission.fx=Audio("Efectos / UI",null,.7f);
            ComposeMusic();AssetDatabase.ImportAsset(Asset("Audio/Nightfall.wav"),ImportAssetOptions.ForceSynchronousImport);
            mission.music.clip=AssetDatabase.LoadAssetAtPath<AudioClip>(Asset("Audio/Nightfall.wav"));
            mission.voices=Enumerable.Range(0,9).Select(i=>AssetDatabase.LoadAssetAtPath<AudioClip>(Asset("Audio/Mission_"+i+".wav"))).ToArray();
            mission.selectSound=Clip("select");mission.scanSound=Clip("scan");mission.successSound=Clip("success");mission.failSound=Clip("fail");mission.teleportSound=Clip("teleport");mission.breachSound=Clip("breach");
            s.actions=actions.ToArray();s.narrator.pitch=.88f;
            var tags=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);var layers=tags.FindProperty("layers");int interactive=LayerMask.NameToLayer("Interactive");
            if(interactive<0)for(int i=8;i<32;i++)if(string.IsNullOrEmpty(layers.GetArrayElementAtIndex(i).stringValue)){layers.GetArrayElementAtIndex(i).stringValue="Interactive";interactive=i;break;}
            tags.ApplyModifiedPropertiesWithoutUndo();if(interactive>=0)foreach(var a in actions)a.gameObject.layer=interactive;
            // Static batching drastically reduces the draw cost of the repeated server geometry.
            foreach(var r in UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Where(r=>r.gameObject.scene==scene))
            {
                if(r.GetComponent<DataCenterAction>()||s.leds.Contains(r)||r.transform.IsChildOf(s.view.transform)||r.transform.IsChildOf(s.core)||r.transform.IsChildOf(mission.shield)||r.transform.IsChildOf(mission.cablePlug)||mission.packets.Contains(r.transform)||r.transform==mission.sweep||r.transform==mission.paper)continue;
                GameObjectUtility.SetStaticEditorFlags(r.gameObject,StaticEditorFlags.BatchingStatic);
            }
            EditorSceneManager.SaveScene(scene,Path);AssetDatabase.SaveAssets();
            for(int i=SceneManager.sceneCount-1;i>=0;i--){var other=SceneManager.GetSceneAt(i);if(other!=scene&&!other.isDirty)EditorSceneManager.CloseScene(other,true);}
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(Path,true)}.Concat(EditorBuildSettings.scenes.Where(x=>x.path!=Path)).ToArray();
            Debug.Log("NIGHTFALL_BUILD_OK");
        }
        static Material Mat(string name,Color c)
        {
            var p=Asset("Materials/"+name+".mat");var m=AssetDatabase.LoadAssetAtPath<Material>(p);if(m)return m;
            m=new Material(Shader.Find("Universal Render Pipeline/Unlit"));m.color=c;m.enableInstancing=true;AssetDatabase.CreateAsset(m,p);return m;
        }
        static GameObject Box(string name,Vector3 p,Vector3 size,Material mat,bool collider=true){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.position=p;g.transform.localScale=size;g.GetComponent<Renderer>().sharedMaterial=mat;if(!collider)UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());return g;}
        static TMP_Text Label(string text,Vector3 p,Vector2 size,int font,Color color,float scale=.002f)
        {
            var g=new GameObject("Pantalla / "+text,typeof(RectTransform),typeof(Canvas));g.GetComponent<Canvas>().renderMode=RenderMode.WorldSpace;g.transform.position=p;g.transform.localScale=Vector3.one*scale;g.GetComponent<RectTransform>().sizeDelta=size;
            var label=new GameObject("Texto",typeof(RectTransform),typeof(TextMeshProUGUI));label.transform.SetParent(g.transform,false);var t=label.GetComponent<TextMeshProUGUI>();t.rectTransform.sizeDelta=size;t.font=TMP_Settings.defaultFontAsset;t.text=text;t.fontSize=font;t.color=color;t.alignment=TextAlignmentOptions.Midline;t.raycastTarget=false;return t;
        }
        static TMP_Text Hud(string text,Camera view,Vector3 p,Vector2 size,int font,Color color){var t=Label(text,view.transform.TransformPoint(p),size,font,color,.001f);t.transform.parent.SetParent(view.transform,true);return t;}
        static GameObject Button(string text,Vector3 p,Vector2 size,int id)
        {
            var g=Box("Acción / "+text,p,new Vector3(size.x,size.y,.035f),dark);var a=g.AddComponent<DataCenterAction>();a.simulation=mission.sim;a.id=id;a.label=Label(text,p+Vector3.back*.024f,new Vector2(size.x/.001f-12,size.y/.001f-6),size.y<.1f?23:80,Ice,.001f);a.label.enableAutoSizing=true;a.label.fontSizeMin=16;actions.Add(a);return g;
        }
        static LineRenderer Line(string name,Material m,Vector3[] points,float width){var g=new GameObject(name);var l=g.AddComponent<LineRenderer>();l.sharedMaterial=m;l.positionCount=points.Length;l.SetPositions(points);l.startWidth=l.endWidth=width;return l;}
        static LineRenderer Circle(string name,Material m,float radius){var l=Line(name,m,new Vector3[64],.012f);l.useWorldSpace=false;l.loop=true;for(int i=0;i<64;i++){var a=i*Mathf.PI*2/64;l.SetPosition(i,new Vector3(Mathf.Cos(a)*radius,0,Mathf.Sin(a)*radius));}return l;}
        static AudioSource Audio(string name,AudioClip clip,float v){var a=new GameObject(name).AddComponent<AudioSource>();a.clip=clip;a.volume=v;a.playOnAwake=false;return a;}
        static AudioClip Clip(string name)=>AssetDatabase.LoadAssetAtPath<AudioClip>(Asset("Audio/"+name+".ogg"));
        public static void ComposeMusic()
        {
            string p=Asset("Audio/Nightfall.wav");if(File.Exists(p))return;const int rate=22050;const float length=20;int samples=(int)(rate*length);var data=new short[samples*2];int[] roots={45,41,48,43};
            var rng=new System.Random(3103);
            for(int i=0;i<samples;i++)
            {
                float t=(float)i/rate,beat=t/.625f,phase=beat-Mathf.Floor(beat);int root=roots[(int)(beat/8)%4];float f=440*Mathf.Pow(2,(root-69)/12f);
                float bass=Mathf.Sin(2*Mathf.PI*f*t)*Mathf.Exp(-phase*5)*.23f;
                float pad=(Mathf.Sin(2*Mathf.PI*f*2*t)+Mathf.Sin(2*Mathf.PI*f*2.378414f*t)+Mathf.Sin(2*Mathf.PI*f*2.996614f*t))*.045f;
                float kick=Mathf.Sin(2*Mathf.PI*(48*t+.08f*Mathf.Exp(-phase*32)))*Mathf.Exp(-phase*28)*.22f;
                float eighth=beat*2-Mathf.Floor(beat*2),hat=((float)rng.NextDouble()*2-1)*Mathf.Exp(-eighth*48)*.045f;
                int arpIndex=(int)(beat*2)%8;float af=f*4*Mathf.Pow(2,new[]{0,7,12,15,12,7,3,7}[arpIndex]/12f);float arp=Mathf.Sin(2*Mathf.PI*af*t)*Mathf.Exp(-eighth*9)*.055f;
                float fade=Mathf.Min(1,t/.08f,(length-t)/.08f);float value=(bass+pad+kick+hat+arp)*fade;
                data[i*2]=(short)(Mathf.Clamp(value,-.95f,.95f)*32767);data[i*2+1]=(short)(Mathf.Clamp(value+Mathf.Sin(2*Mathf.PI*f*2.002f*t)*.015f,-.95f,.95f)*32767);
            }
            using(var w=new BinaryWriter(File.Create(p))){w.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));w.Write(36+data.Length*2);w.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));w.Write(16);w.Write((short)1);w.Write((short)2);w.Write(rate);w.Write(rate*4);w.Write((short)4);w.Write((short)16);w.Write(System.Text.Encoding.ASCII.GetBytes("data"));w.Write(data.Length*2);foreach(var v in data)w.Write(v);}
        }

        [MenuItem("CyberShield/Nightfall Android/Configurar Cardboard Android")]
        public static void ConfigureAndroid()
        {
            #if UNITY_ANDROID
            var project=Directory.GetParent(Application.dataPath).FullName;
            var tools=new[]{System.IO.Path.GetFullPath(System.IO.Path.Combine(project,"..","AndroidToolchain")),System.IO.Path.Combine(project,"SourceAssets","AndroidToolchain")}.FirstOrDefault(p=>File.Exists(System.IO.Path.Combine(p,"OpenJDK","bin","java.exe")));
            if(tools!=null)
            {
                UnityEditor.Android.AndroidExternalToolsSettings.jdkRootPath=System.IO.Path.Combine(tools,"OpenJDK");
                UnityEditor.Android.AndroidExternalToolsSettings.ndkRootPath=System.IO.Path.Combine(tools,"NDK");
                UnityEditor.Android.AndroidExternalToolsSettings.sdkRootPath=System.IO.Path.Combine(tools,"SDK");
                UnityEditor.Android.AndroidExternalToolsSettings.maxJvmHeapSize=2048;
            }
            #endif
            var target=UnityEditor.Build.NamedBuildTarget.Android;
            PlayerSettings.companyName="Equipo Nightfall";PlayerSettings.productName="Nightfall VR";
            GenerateIcon();
            PlayerSettings.SetApplicationIdentifier(target,"com.equipo.nightfallvr");PlayerSettings.bundleVersion="3.0";PlayerSettings.Android.bundleVersionCode=3;
            PlayerSettings.Android.minSdkVersion=AndroidSdkVersions.AndroidApiLevel26;PlayerSettings.Android.targetSdkVersion=AndroidSdkVersions.AndroidApiLevel35;
            PlayerSettings.SetScriptingBackend(target,ScriptingImplementation.IL2CPP);PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64;
            PlayerSettings.defaultInterfaceOrientation=UIOrientation.LandscapeLeft;PlayerSettings.Android.optimizedFramePacing=false;
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android,false);PlayerSettings.SetGraphicsAPIs(BuildTarget.Android,new[]{GraphicsDeviceType.OpenGLES3});
            PlayerSettings.Android.forceInternetPermission=true;PlayerSettings.Android.applicationEntry=AndroidApplicationEntry.Activity;
            var settings=XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Android);
            if(settings==null)throw new Exception("No existe configuración XR para Android.");
            settings.InitManagerOnStart=false;var manager=settings.Manager;
            manager.automaticLoading=false;manager.automaticRunning=false;
            foreach(var loader in manager.activeLoaders.Where(x=>x).ToArray())XRPackageMetadataStore.RemoveLoader(manager,loader.GetType().FullName,BuildTargetGroup.Android);
            if(!XRPackageMetadataStore.AssignLoader(manager,"Google.XR.Cardboard.XRLoader",BuildTargetGroup.Android))throw new Exception("No se pudo asignar Cardboard.");
            EditorUtility.SetDirty(settings);EditorUtility.SetDirty(manager);AssetDatabase.SaveAssets();Debug.Log("NIGHTFALL_ANDROID_CONFIGURED");
        }
        static void GenerateIcon()
        {
            string p=Asset("Nightfall-Icon.png");
            if(!File.Exists(p))
            {
                var t=new Texture2D(512,512,TextureFormat.RGB24,false);
                for(int y=0;y<512;y++)for(int x=0;x<512;x++)
                {
                    float u=(x-256)/256f,v=(y-256)/256f;
                    bool shield=Mathf.Abs(u)<.62f&&v<.65f&&v>-.76f&&Mathf.Abs(u)<(.52f+.22f*v);
                    bool inner=Mathf.Abs(u)<.51f&&v<.54f&&v>-.61f&&Mathf.Abs(u)<(.42f+.18f*v);
                    bool n=(Mathf.Abs(u+.22f)<.065f||Mathf.Abs(u-.22f)<.065f||Mathf.Abs(u+.62f*v)<.075f)&&Mathf.Abs(v)<.31f;
                    t.SetPixel(x,y,n&&inner?Color.white:shield&&!inner?Ice:new Color(.015f,.035f,.065f));
                }
                t.Apply();File.WriteAllBytes(p,t.EncodeToPNG());UnityEngine.Object.DestroyImmediate(t);AssetDatabase.ImportAsset(p,ImportAssetOptions.ForceSynchronousImport);
            }
            var icon=AssetDatabase.LoadAssetAtPath<Texture2D>(p);PlayerSettings.SetIcons(UnityEditor.Build.NamedBuildTarget.Android,new[]{icon},IconKind.Application);
        }
        [MenuItem("CyberShield/Nightfall Android/Generar APK")]
        public static void BuildAPK()
        {
            if(!File.Exists(Path))Build();ConfigureAndroid();Directory.CreateDirectory("Builds/Android");
            var report=BuildPipeline.BuildPlayer(new[]{Path},"Builds/Android/NightfallVR-3.apk",BuildTarget.Android,BuildOptions.None);
            if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("APK: "+report.summary.result);
            Debug.Log("NIGHTFALL_APK_OK");
        }
    }
}
