using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace CyberShield.Editor
{
    public static class CyberShieldArcadeBuilder
    {
        public const string Path="Assets/CyberShield/Scenes/CyberShieldVR_Arcade.unity";
        static Transform root;
        static Material cyan,red,gold;
        [MenuItem("CyberShield/Crear experiencia Arcade")]
        public static string Build()
        {
            if(AssetDatabase.LoadAssetAtPath<SceneAsset>(Path))throw new Exception("Arcade ya existe");
            var current=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if(current.isDirty)throw new Exception("Guarda la escena actual antes de continuar");
            var scene=EditorSceneManager.OpenScene(CyberShieldCorporateBuilder.Path);
            EditorSceneManager.SaveScene(scene,Path,true);scene=EditorSceneManager.OpenScene(Path);
            root=new GameObject("CYBERSHIELD / Experiencia Arcade").transform;
            System.IO.Directory.CreateDirectory("Assets/CyberShield/Materials/Arcade");
            cyan=Mat("Holograma",new Color(.04f,.8f,1));red=Mat("Amenaza",new Color(1,.12f,.04f));gold=Mat("Victoria",new Color(1,.7f,.08f));
            var game=UnityEngine.Object.FindFirstObjectByType<CyberShieldGame>();
            var a=game.gameObject.AddComponent<CyberShieldArcade>();a.game=game;
            a.clues=new XRSimpleInteractable[3];a.clueVisuals=new Renderer[3];
            string[] titles={"01 / IDENTIDAD","02 / ENLACE","03 / PRESIÓN"};
            string[] details={"¿Quién lo envía?","¿A dónde te lleva?","¿Por qué tanta prisa?"};
            for(int i=0;i<3;i++)
            {
                var g=new GameObject(titles[i]);g.transform.SetParent(root);g.transform.position=new Vector3(-3.25f+i*.63f,1.6f,1.15f);
                var shape=Primitive("Pista holográfica",PrimitiveType.Cube,Vector3.zero,Vector3.one*.19f,cyan);shape.transform.SetParent(g.transform,false);
                UnityEngine.Object.DestroyImmediate(shape.GetComponent<Collider>());
                var collider=g.AddComponent<SphereCollider>();collider.radius=.23f;
                a.clues[i]=g.AddComponent<XRSimpleInteractable>();a.clueVisuals[i]=shape.GetComponent<Renderer>();
                var text=Label(titles[i]+"\n<size=65%>"+details[i]+"</size>",Vector3.zero,new Vector2(.6f,.3f),1.1f,Color.white);
                text.transform.SetParent(g.transform,false);text.transform.localPosition=new Vector3(0,.32f,0);
            }
            var core=Primitive("Núcleo de amenaza",PrimitiveType.Sphere,new Vector3(0,1.75f,2.15f),Vector3.one*.3f,red);
            UnityEngine.Object.DestroyImmediate(core.GetComponent<Collider>());a.threat=core.transform;a.threatVisual=core.GetComponent<Renderer>();
            a.rings=new LineRenderer[3];
            for(int i=0;i<3;i++)
            {
                var g=new GameObject("Anillo de contención");g.transform.SetParent(root);g.transform.position=core.transform.position;g.transform.rotation=Quaternion.Euler(i*50,0,i*35);
                var line=g.AddComponent<LineRenderer>();line.useWorldSpace=false;line.loop=true;line.positionCount=48;line.widthMultiplier=.012f;line.sharedMaterial=cyan;
                for(int p=0;p<48;p++){float angle=p*Mathf.PI*2/48;line.SetPosition(p,new Vector3(Mathf.Cos(angle)*.36f,0,Mathf.Sin(angle)*.36f));}a.rings[i]=line;
            }
            a.objective=Label("CYBERSHIELD",new Vector3(0,2.6f,2.6f),new Vector2(5,.5f),3,Color.cyan);
            a.compactStatus=Label("00/100",new Vector3(0,3.05f,2.6f),new Vector2(4,.5f),3.5f,Color.white);
            a.toast=Label("",new Vector3(0,2.15f,1.8f),new Vector2(4,.4f),2, new Color(.4f,1,.7f));
            a.markers=new Transform[3];Vector3[] positions={new Vector3(-2.5f,.05f,.2f),new Vector3(0,.05f,.2f),new Vector3(2.45f,.05f,.1f)};
            for(int i=0;i<3;i++)
            {
                var marker=Primitive("Objetivo activo / "+i,PrimitiveType.Cylinder,positions[i],new Vector3(.65f,.015f,.65f),cyan);
                UnityEngine.Object.DestroyImmediate(marker.GetComponent<Collider>());a.markers[i]=marker.transform;
            }
            a.celebration=new Transform[18];for(int i=0;i<18;i++){var p=Primitive("Fragmento de victoria",PrimitiveType.Cube,Vector3.zero,Vector3.one*.04f,i%2==0?gold:cyan);UnityEngine.Object.DestroyImmediate(p.GetComponent<Collider>());p.SetActive(false);a.celebration[i]=p.transform;}
            // Replace large instruction walls with concise decisions; keep the existing game wiring.
            string[] options={"CALENDARIO\nReunión a las 15:00","SOPORTE\nVerifica tu contraseña ahora","BIBLIOTECA\nReserva confirmada"};
            for(int i=0;i<3;i++)game.choices[i].GetComponentInChildren<TMP_Text>().text=options[i];
            foreach(string name in new[]{"02 / AISLAR","03 / RESTAURAR","Estado de misión","Servicios corporativos","Comunicaciones del equipo"}){var g=GameObject.Find(name);if(g)g.SetActive(false);}
            var email=GameObject.Find("01 / DETECTAR");email.transform.position=new Vector3(-2.5f,2.65f,2.8f);email.transform.localScale*=.82f;
            var instruction=email.transform.Find("Instrucción");if(instruction)instruction.GetComponent<TMP_Text>().text="Toca dos pistas holográficas. Después reporta el correo falso.";
            var evidence=GameObject.Find("Expediente de correo");evidence.transform.position=new Vector3(-2.5f,.95f,1.3f);evidence.transform.localScale*=.8f;
            var hands=game.GetComponent<CyberShieldHandsOn>();hands.isolationLabel.fontSizeMax=1.35f;
            var atmosphere=game.GetComponent<CyberShieldAtmosphere>();
            foreach(var t in atmosphere.briefing.GetComponentsInChildren<TMP_Text>())
            {
                if(t.text.Contains("Contabilidad reporta"))t.text="Una amenaza entró en Nexora.\nTienes tres acciones para proteger la empresa.";
                if(t.text.StartsWith("01"))t.text="INVESTIGA  →  AISLA  →  RECUPERA";
                if(t.text.Contains("Gatillo:"))t.text="Apunta y selecciona. Agarra y suelta las tarjetas.\nPC: WASD · ratón · clic / T · G para agarrar.";
            }
            foreach(var c in UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include,FindObjectsSortMode.None))
                if(c.GetComponent<UnityEngine.XR.Interaction.Toolkit.UI.TrackedDeviceGraphicRaycaster>()&&!c.GetComponent<UnityEngine.UI.GraphicRaycaster>())c.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            if(!EditorBuildSettings.scenes.Any(s=>s.path==Path))EditorBuildSettings.scenes=EditorBuildSettings.scenes.Concat(new[]{new EditorBuildSettingsScene(Path,true)}).ToArray();
            return Path;
        }
        static Material Mat(string name,Color color){var m=new Material(Shader.Find("Universal Render Pipeline/Unlit"));m.SetColor("_BaseColor",color);AssetDatabase.CreateAsset(m,"Assets/CyberShield/Materials/Arcade/"+name+".mat");return m;}
        static GameObject Primitive(string name,PrimitiveType type,Vector3 p,Vector3 scale,Material mat){var g=GameObject.CreatePrimitive(type);g.name=name;g.transform.SetParent(root);g.transform.position=p;g.transform.localScale=scale;g.GetComponent<Renderer>().sharedMaterial=mat;return g;}
        static TMP_Text Label(string content,Vector3 p,Vector2 size,float font,Color color){var g=new GameObject("HUD / "+content,typeof(TextMeshPro));g.transform.SetParent(root);g.transform.position=p;var t=g.GetComponent<TextMeshPro>();t.text=content;t.font=TMP_Settings.defaultFontAsset;t.fontSize=font;t.color=color;t.alignment=TextAlignmentOptions.Center;t.rectTransform.sizeDelta=size;return t;}
    }
}
