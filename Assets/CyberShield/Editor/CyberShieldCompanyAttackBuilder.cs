using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using static CyberShield.Editor.CyberShieldTacticalBuilder;

namespace CyberShield.Editor
{
    public static class CyberShieldCompanyAttackBuilder
    {
        const string AudioFolder="Assets/CyberShield/Audio/Operator/";
        public static string PrepareAudio()
        {
            CyberShieldImmersionBuilder.Save("IntrusionImpact",1.6f,t=>Mathf.Min(1,t*80)*(Mathf.Sin(2*Mathf.PI*(100*t-20*t*t))*Mathf.Exp(-t*3)*.28f+Mathf.Sin(t*87311)*Mathf.Exp(-t*60)*.07f));
            CyberShieldImmersionBuilder.Save("DataTraffic",4,t=>(Mathf.Sin(2*Mathf.PI*58*t)*.08f+Mathf.Sin(2*Mathf.PI*440*t)*.009f)*(.3f+.7f*Mathf.Exp(-Mathf.Repeat(t,.5f)*12)));
            CyberShieldImmersionBuilder.Save("NetworkRecovery",1,t=>{float frequency=t<.25f?440:t<.5f?554.36f:659.25f;return Mathf.Sin(2*Mathf.PI*frequency*t)*Mathf.Sin(Mathf.PI*t)*.12f;});
            foreach(string name in new[]{"IntrusionImpact","DataTraffic","NetworkRecovery"})AssetDatabase.ImportAsset(AudioFolder+name+".wav",ImportAssetOptions.ForceSynchronousImport);
            return "Impacto, tráfico y recuperación preparados.";
        }
        public static string Install()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new Exception("Detén Play primero.");
            var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();if(!scene.path.EndsWith("CyberShieldVR_ShieldOps.unity"))throw new Exception("Abre Shield Ops.");
            if(UnityEngine.Object.FindObjectsByType<CyberShieldCompanyAttack>(FindObjectsInactive.Include,FindObjectsSortMode.None).Any())throw new Exception("La simulación corporativa ya existe.");
            foreach(string name in new[]{"IntrusionImpact","DataTraffic","NetworkRecovery"})if(!Clip(name))throw new Exception("Prepara el audio primero.");
            var root=new GameObject("NEXORA / COMPANY UNDER ATTACK");root.SetActive(false);var attack=root.AddComponent<CyberShieldCompanyAttack>();attack.game=UnityEngine.Object.FindFirstObjectByType<CyberShieldGame>();
            foreach(var t in UnityEngine.Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include,FindObjectsSortMode.None))if(t.text=="NEXORA"||t.text=="TECNOLOGÍA QUE CUIDA TU OPERACIÓN")t.gameObject.SetActive(false);
            var banner=Panel(root.transform,"Nexora / estado de incidente",new Vector3(0,3.65f,3.65f),new Vector2(2800,430),.0015f);
            attack.headlineSymbol=Glyph(banner,8,new Vector2(-1240,40),190);
            attack.headline=SharedText(banner,"NEXORA / SISTEMAS EN LÍNEA",new Vector2(100,55),new Vector2(2450,190),94,new Color(.06f,.8f,1));
            SharedText(banner,"CS-1042   /   PC-02",new Vector2(0,-120),new Vector2(2450,70),48,new Color(.6f,.75f,.85f));
            var files=Panel(root.transform,"Archivos / incidente",new Vector3(-4.61f,2.35f,.4f),new Vector2(1200,950),.0017f);files.rotation=Quaternion.Euler(0,-90,0);
            SharedText(files,"ARCHIVOS DE RED",new Vector2(0,390),new Vector2(1100,95),65,new Color(.08f,.8f,1));
            attack.fileCells=new Image[32];attack.fileSymbols=new CyberShieldGlyph[32];
            for(int i=0;i<32;i++){var cell=Rect(files,new Vector2(-455+(i%8)*130,200-(i/8)*140),new Vector2(112,100),new Color(.04f,.12f,.18f));attack.fileCells[i]=cell;attack.fileSymbols[i]=Glyph(cell.transform,6,Vector2.zero,62);}
            attack.counter=SharedText(files,"ARCHIVOS AFECTADOS 00/32",new Vector2(0,-365),new Vector2(1120,90),52,Color.white);
            var services=Panel(root.transform,"Servicios / respuesta",new Vector3(4.61f,2.35f,.4f),new Vector2(1200,950),.0017f);services.rotation=Quaternion.Euler(0,90,0);
            SharedText(services,"CENTRO DE RESPUESTA",new Vector2(0,390),new Vector2(1120,95),58,new Color(.08f,.8f,1));
            attack.eventLog=SharedText(services,"CS-1042 / MONITORIZACIÓN",new Vector2(0,195),new Vector2(1100,240),43,Color.white);
            attack.serviceBars=new Image[3];var sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/CyberShield/Materials/ShieldOps/Barra sprite.asset");
            for(int i=0;i<3;i++)
            {
                SharedText(services,new[]{"ERP","ARCHIVOS","ACCESOS"}[i],new Vector2(0,-10-i*140),new Vector2(1100,65),42,new Color(.6f,.78f,.88f));Rect(services,new Vector2(0,-65-i*140),new Vector2(1000,25),new Color(.03f,.1f,.15f));
                var bar=Rect(services,new Vector2(0,-65-i*140),new Vector2(1000,25),Color.cyan);bar.sprite=sprite;bar.type=Image.Type.Filled;bar.fillMethod=Image.FillMethod.Horizontal;bar.fillOrigin=0;bar.fillAmount=1;attack.serviceBars[i]=bar;
            }
            var screens=UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(t=>t.name.StartsWith("computerScreen")&&t.parent&&(t.parent.name=="Mobiliario Kenney"||t.parent.name=="Puestos / ampliación")).ToArray();
            attack.officePanels=new Image[screens.Length];attack.officeSymbols=new CyberShieldGlyph[screens.Length];attack.officeStatus=new TMP_Text[screens.Length];
            for(int i=0;i<screens.Length;i++)
            {
                var bounds=screens[i].GetComponentInChildren<Renderer>().bounds;var monitor=Panel(root.transform,"Terminal corporativo "+i,bounds.center+new Vector3(0,.045f,-bounds.extents.z-.012f),new Vector2(600,360),.00087f);
                attack.officePanels[i]=monitor.GetComponent<Image>();SharedText(monitor,"NEXORA / INTRANET",new Vector2(0,125),new Vector2(570,60),33,new Color(.3f,.8f,1));attack.officeSymbols[i]=Glyph(monitor,8,new Vector2(0,15),115);attack.officeStatus[i]=SharedText(monitor,"EN LÍNEA",new Vector2(0,-112),new Vector2(570,65),29,Color.white);
            }
            var material=new Material(Shader.Find("Universal Render Pipeline/Unlit"));material.color=new Color(1,.18f,.035f);string path="Assets/CyberShield/Materials/ShieldOps/Tráfico de ataque.mat";var existing=AssetDatabase.LoadAssetAtPath<Material>(path);if(existing){UnityEngine.Object.DestroyImmediate(material);material=existing;}else AssetDatabase.CreateAsset(material,path);
            attack.outbound=Line(root.transform,"Conexión exterior",material,3,.012f);attack.outbound.SetPositions(new[]{new Vector3(-.95f,1.45f,1.15f),new Vector3(0,3.55f,2.55f),new Vector3(4.35f,3.1f,2.5f)});attack.outbound.enabled=false;
            attack.outboundPackets=new Transform[10];for(int i=0;i<10;i++){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name="Tráfico exterior / simulado";g.transform.SetParent(root.transform,false);g.transform.localScale=new Vector3(.05f,.025f,.025f);g.GetComponent<Renderer>().sharedMaterial=material;UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());attack.outboundPackets[i]=g.transform;g.SetActive(false);}
            var remote=Panel(root.transform,"Destino externo",new Vector3(4.15f,3.05f,2.5f),new Vector2(500,230),.0013f);Glyph(remote,5,new Vector2(-130,0),120);SharedText(remote,"EXTERNO",new Vector2(70,0),new Vector2(300,100),53,new Color(1,.35f,.05f));
            var green=AssetDatabase.LoadAssetAtPath<Material>("Assets/CyberShield/Materials/ShieldOps/SSD luz.mat");attack.containmentWave=Line(root.transform,"Onda de contención",green,65,.018f);attack.containmentWave.enabled=false;
            attack.impact=Source(root.transform,"Audio / incidente",null,false,0,1);attack.impactClip=Clip("IntrusionImpact");attack.recoveryClip=Clip("NetworkRecovery");attack.traffic=Source(root.transform,"Audio / tráfico",Clip("DataTraffic"),true,.4f,0);attack.traffic.transform.position=new Vector3(0,2,2);
            attack.game.GetComponent<CyberShieldBreachAtmosphere>().lightStrength=3.2f;
            root.SetActive(true);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();return "Ataque corporativo conectado a cuatro terminales, archivos, servicios y contención.";
        }
        static Transform Panel(Transform parent,string name,Vector3 position,Vector2 size,float scale){var g=new GameObject(name,typeof(RectTransform),typeof(Canvas),typeof(Image));g.transform.SetParent(parent,false);g.transform.position=position;g.transform.localScale=Vector3.one*scale;g.GetComponent<RectTransform>().sizeDelta=size;g.GetComponent<Canvas>().renderMode=RenderMode.WorldSpace;g.GetComponent<Image>().color=new Color(.012f,.035f,.055f);g.GetComponent<Image>().raycastTarget=false;var border=new GameObject("Borde",typeof(RectTransform),typeof(CanvasRenderer),typeof(CyberShieldPanelArt));border.transform.SetParent(g.transform,false);var r=border.GetComponent<RectTransform>();r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;border.GetComponent<CyberShieldPanelArt>().color=new Color(.04f,.7f,1);border.GetComponent<CyberShieldPanelArt>().raycastTarget=false;return g.transform;}
        static Image Rect(Transform parent,Vector2 position,Vector2 size,Color color){var g=new GameObject("Indicador",typeof(RectTransform),typeof(Image));g.transform.SetParent(parent,false);var r=g.GetComponent<RectTransform>();r.anchoredPosition=position;r.sizeDelta=size;var image=g.GetComponent<Image>();image.color=color;image.raycastTarget=false;return image;}
        static CyberShieldGlyph Glyph(Transform parent,int symbol,Vector2 position,float size){var g=new GameObject("Símbolo",typeof(RectTransform),typeof(CanvasRenderer),typeof(CyberShieldGlyph));g.transform.SetParent(parent,false);var r=g.GetComponent<RectTransform>();r.anchoredPosition=position;r.sizeDelta=Vector2.one*size;var glyph=g.GetComponent<CyberShieldGlyph>();glyph.symbol=symbol;glyph.color=Color.white;glyph.raycastTarget=false;return glyph;}
        static LineRenderer Line(Transform parent,string name,Material material,int count,float width){var g=new GameObject(name,typeof(LineRenderer));g.transform.SetParent(parent,false);var line=g.GetComponent<LineRenderer>();line.sharedMaterial=material;line.positionCount=count;line.startWidth=line.endWidth=width;line.useWorldSpace=true;return line;}
        static AudioSource Source(Transform parent,string name,AudioClip clip,bool loop,float spatial,float volume){var g=new GameObject(name);g.transform.SetParent(parent,false);var s=g.AddComponent<AudioSource>();s.clip=clip;s.loop=loop;s.playOnAwake=false;s.spatialBlend=spatial;s.volume=volume;s.minDistance=1;s.maxDistance=10;s.dopplerLevel=0;return s;}
        static AudioClip Clip(string name)=>AssetDatabase.LoadAssetAtPath<AudioClip>(AudioFolder+name+".wav");
    }
}
