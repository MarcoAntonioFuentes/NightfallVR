using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using static CyberShield.Editor.CyberShieldTacticalBuilder;

namespace CyberShield.Editor
{
    public static class CyberShieldDeluxeBuilder
    {
        const string Folder="Assets/CyberShield/Meshes/Deluxe/";
        const string Materials="Assets/CyberShield/Materials/ShieldOps/";
        static Material metal,glass,gold,cyan;
        public static string Prepare()
        {
            Directory.CreateDirectory(Folder);AssetDatabase.Refresh();
            metal=Mat("SSD titanio",new Color(.22f,.3f,.38f),false,.72f,.48f);glass=Mat("SSD vidrio",new Color(.018f,.045f,.07f),false,.2f,.7f);gold=Mat("SSD contactos",new Color(.75f,.46f,.12f),false,.8f,.55f);cyan=Mat("SSD luz",new Color(.02f,.8f,1),true,0,0);
            if(!AssetDatabase.LoadAssetAtPath<Mesh>(Folder+"Carcasa.asset"))AssetDatabase.CreateAsset(Beveled(),Folder+"Carcasa.asset");AssetDatabase.SaveAssets();return "Carcasa biselada y materiales preparados.";
        }
        public static string Install()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new Exception("Detén Play antes de instalar.");
            var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();if(!scene.path.EndsWith("CyberShieldVR_ShieldOps.unity"))throw new Exception("Abre Shield Ops.");
            var existing=UnityEngine.Object.FindObjectsByType<CyberShieldPolishDirector>(FindObjectsInactive.Include,FindObjectsSortMode.None).FirstOrDefault();if(existing)return Complete(existing);
            var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(Folder+"Carcasa.asset");if(!mesh)throw new Exception("Prepara los materiales primero.");
            metal=AssetDatabase.LoadAssetAtPath<Material>(Materials+"SSD titanio.mat");glass=AssetDatabase.LoadAssetAtPath<Material>(Materials+"SSD vidrio.mat");gold=AssetDatabase.LoadAssetAtPath<Material>(Materials+"SSD contactos.mat");cyan=AssetDatabase.LoadAssetAtPath<Material>(Materials+"SSD luz.mat");
            var root=new GameObject("CYBERSHIELD / DELUXE TERMINALS");root.SetActive(false);var p=root.AddComponent<CyberShieldPolishDirector>();p.game=UnityEngine.Object.FindFirstObjectByType<CyberShieldGame>();p.hands=p.game.GetComponent<CyberShieldHandsOn>();p.flow=p.game.GetComponent<CyberShieldReferenceExperience>();p.driveStrips=new Renderer[3];p.driveStatus=new TMP_Text[3];p.driveHalos=new LineRenderer[3];
            for(int i=0;i<3;i++)
            {
                var card=p.hands.cards[i];card.GetComponent<MeshRenderer>().enabled=false;
                foreach(var text in card.GetComponentsInChildren<TMP_Text>(true))text.gameObject.SetActive(false);
                var body=new GameObject("SSD / dispositivo").transform;body.SetParent(card.transform,false);var s=card.transform.lossyScale;body.localScale=new Vector3(1/s.x,1/s.y,1/s.z);
                var shell=new GameObject("Carcasa",typeof(MeshFilter),typeof(MeshRenderer));shell.transform.SetParent(body,false);shell.GetComponent<MeshFilter>().sharedMesh=mesh;shell.GetComponent<MeshRenderer>().sharedMaterial=metal;
                Cube(body,"Placa de datos",new Vector3(0,.027f,.012f),new Vector3(.18f,.004f,.22f),glass);
                for(int j=0;j<6;j++)Cube(body,"Contacto",new Vector3(-.075f+j*.03f,0,-.147f),new Vector3(.014f,.012f,.024f),gold);
                for(int edge=-1;edge<=1;edge+=2)for(int slot=0;slot<5;slot++)Cube(body,"Disipador",new Vector3(edge*.108f,0,-.085f+slot*.04f),new Vector3(.005f,.017f,.022f),glass);
                for(int sx=-1;sx<=1;sx+=2)for(int sz=-1;sz<=1;sz+=2)Cube(body,"Tornillo",new Vector3(sx*.087f,.026f,sz*.12f),new Vector3(.009f,.002f,.009f),gold);
                p.driveStrips[i]=Cube(body,"Status LED",new Vector3(0,.031f,.126f),new Vector3(.15f,.003f,.006f),cyan).GetComponent<Renderer>();
                var face=new GameObject("SSD / etiqueta",typeof(RectTransform),typeof(Canvas));face.transform.SetParent(body,false);face.transform.localPosition=new Vector3(0,.031f,.015f);face.transform.localRotation=Quaternion.Euler(90,0,0);face.transform.localScale=Vector3.one*.0008f;face.GetComponent<RectTransform>().sizeDelta=new Vector2(215,250);face.GetComponent<Canvas>().renderMode=RenderMode.WorldSpace;
                SharedText(face.transform,((char)('A'+i)).ToString(),new Vector2(-67,79),new Vector2(60,60),55,Color.white);
                Glyph(face.transform,6,new Vector2(54,74),62);
                SharedText(face.transform,new[]{"AYER · 18:00","HOY · 10:15","SINCRONIZADO"}[i],new Vector2(0,0),new Vector2(210,40),24,new Color(.55f,.75f,.86f));
                p.driveStatus[i]=SharedText(face.transform,"OFFLINE",new Vector2(0,-53),new Vector2(210,50),30,Color.cyan);
                for(int bar=0;bar<19;bar++)Cube(body,"Identificador",new Vector3(-.06f+bar*.0065f,.031f,-.065f),new Vector3(bar%3==0?.0035f:.0015f,.001f,.024f),gold);
                var halo=new GameObject("SSD / foco",typeof(LineRenderer)).GetComponent<LineRenderer>();halo.transform.SetParent(body,false);halo.sharedMaterial=cyan;halo.useWorldSpace=false;halo.loop=true;halo.positionCount=4;halo.SetPositions(new[]{new Vector3(-.125f,-.028f,-.17f),new Vector3(.125f,-.028f,-.17f),new Vector3(.125f,-.028f,.17f),new Vector3(-.125f,-.028f,.17f)});halo.startWidth=halo.endWidth=.003f;halo.enabled=false;p.driveHalos[i]=halo;
                Combine(body,i);
            }
            return Complete(p);
        }
        static string Complete(CyberShieldPolishDirector p)
        {
            if(p.scopePanel)return "La renovación ya está completa.";
            var scene=p.game.gameObject.scene;var root=p.gameObject;
            metal=AssetDatabase.LoadAssetAtPath<Material>(Materials+"SSD titanio.mat");cyan=AssetDatabase.LoadAssetAtPath<Material>(Materials+"SSD luz.mat");
            foreach(var canvas in new[]{UnityEngine.Object.FindFirstObjectByType<CyberShieldMailInvestigation>().viewer.transform,UnityEngine.Object.FindFirstObjectByType<CyberShieldTacticalNetwork>().details.transform,p.flow.recoveryPanel.transform,p.flow.victoryPanel.transform,p.game.GetComponent<CyberShieldAtmosphere>().briefing.transform})Frame(canvas);
            foreach(var b in UnityEngine.Object.FindObjectsByType<Button>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(b=>b.gameObject.scene==scene))
            {
                if(b.GetComponent<CyberShieldButtonFeel>())continue;var feel=b.gameObject.AddComponent<CyberShieldButtonFeel>();feel.outline=Frame(b.transform);feel.outline.rectTransform.anchorMin=Vector2.zero;feel.outline.rectTransform.anchorMax=Vector2.one;feel.outline.rectTransform.offsetMin=Vector2.one*2;feel.outline.rectTransform.offsetMax=Vector2.one*-2;
            }
            var scope=new GameObject("Monitor / scope",typeof(RectTransform),typeof(Canvas),typeof(Image));scope.transform.SetParent(root.transform,false);scope.transform.position=new Vector3(0,1.55f,2.65f);scope.transform.localScale=Vector3.one*.0022f;scope.GetComponent<RectTransform>().sizeDelta=new Vector2(1000,740);scope.GetComponent<Canvas>().renderMode=RenderMode.WorldSpace;scope.GetComponent<Image>().color=new Color(.008f,.022f,.038f);scope.GetComponent<Image>().raycastTarget=false;p.scopePanel=scope;
            var radar=new GameObject("Radar",typeof(RectTransform),typeof(CanvasRenderer),typeof(CyberShieldPanelArt));radar.transform.SetParent(scope.transform,false);radar.GetComponent<RectTransform>().sizeDelta=new Vector2(660,640);p.scope=radar.GetComponent<CyberShieldPanelArt>();p.scope.radar=true;p.scope.raycastTarget=false;
            SharedText(scope.transform,"NEXORA / DEFENSE",new Vector2(0,305),new Vector2(850,60),32,new Color(.1f,.75f,1));Frame(scope.transform);
            Dock(root.transform,p.game.corporateScenario.scanZone.position,"ANALIZADOR");Dock(root.transform,p.hands.reader.position,"RESTORE");
            var sweep=new GameObject("Escáner / barrido",typeof(LineRenderer));sweep.transform.SetParent(root.transform,false);var line=sweep.GetComponent<LineRenderer>();line.sharedMaterial=cyan;line.useWorldSpace=false;line.positionCount=2;line.SetPositions(new[]{new Vector3(-.23f,0,0),new Vector3(.23f,0,0)});line.startWidth=line.endWidth=.005f;p.scannerSweep=sweep.transform;sweep.SetActive(false);
            p.transferPackets=new Transform[8];for(int i=0;i<8;i++){var packet=Cube(root.transform,"Transferencia",Vector3.zero,Vector3.one*.018f,cyan);p.transferPackets[i]=packet.transform;packet.SetActive(false);}
            root.SetActive(true);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();return "Dispositivos de respaldo, pantallas, radar y bandejas renovados.";
        }
        public static string FinishDocks()
        {
            var p=UnityEngine.Object.FindFirstObjectByType<CyberShieldPolishDirector>();var root=p.transform;
            var dark=AssetDatabase.LoadAssetAtPath<Material>(Materials+"SSD vidrio.mat");var amber=Mat("SSD luz ámbar",new Color(1,.4f,.025f),true,0,0);
            foreach(string name in new[]{"Bandeja de análisis","Lector de respaldo"}){var body=GameObject.Find(name);if(body&&body.GetComponent<Renderer>())body.GetComponent<Renderer>().sharedMaterial=dark;}
            foreach(var r in root.GetComponentsInChildren<MeshRenderer>())if(r.name=="ANALIZADOR / guía")r.sharedMaterial=amber;
            p.hands.readerLabel.gameObject.SetActive(false);
            EditorSceneManager.MarkSceneDirty(root.gameObject.scene);EditorSceneManager.SaveOpenScenes();AssetDatabase.SaveAssets();return "Bandejas con acabados oscuros y guías ámbar/cian guardadas.";
        }
        static void Dock(Transform parent,Vector3 center,string label)
        {
            Cube(parent,label+" / base",center-Vector3.up*.028f,new Vector3(.54f,.04f,.45f),metal);
            for(int side=-1;side<=1;side+=2)Cube(parent,label+" / guía",center+new Vector3(side*.255f,0,0),new Vector3(.008f,.007f,.42f),cyan);
        }
        static CyberShieldPanelArt Frame(Transform parent)
        {
            var g=new GameObject("Borde / control",typeof(RectTransform),typeof(CanvasRenderer),typeof(CyberShieldPanelArt));g.transform.SetParent(parent,false);var r=g.GetComponent<RectTransform>();r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;var graphic=g.GetComponent<CyberShieldPanelArt>();graphic.color=new Color(.03f,.7f,1,.85f);graphic.raycastTarget=false;return graphic;
        }
        static void Glyph(Transform parent,int symbol,Vector2 position,float size){var g=new GameObject("Icono",typeof(RectTransform),typeof(CanvasRenderer),typeof(CyberShieldGlyph));g.transform.SetParent(parent,false);var r=g.GetComponent<RectTransform>();r.anchoredPosition=position;r.sizeDelta=Vector2.one*size;var icon=g.GetComponent<CyberShieldGlyph>();icon.symbol=symbol;icon.color=Color.white;icon.raycastTarget=false;}
        static GameObject Cube(Transform parent,string name,Vector3 position,Vector3 scale,Material material){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(parent,false);g.transform.localPosition=position;g.transform.localScale=scale;g.GetComponent<Renderer>().sharedMaterial=material;UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());return g;}
        static Material Mat(string name,Color color,bool unlit,float metallic,float smooth){string path=Materials+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(m)return m;m=new Material(Shader.Find(unlit?"Universal Render Pipeline/Unlit":"Universal Render Pipeline/Lit"));m.color=color;if(!unlit){m.SetFloat("_Metallic",metallic);m.SetFloat("_Smoothness",smooth);}AssetDatabase.CreateAsset(m,path);return m;}
        static void Combine(Transform root,int card)
        {
            var groups=root.GetComponentsInChildren<MeshFilter>().Where(f=>f.name!="Status LED").GroupBy(f=>f.GetComponent<MeshRenderer>().sharedMaterial).ToArray();int group=0;
            foreach(var batch in groups)
            {
                var mesh=new Mesh();mesh.name="SSD combinado";mesh.CombineMeshes(batch.Select(f=>new CombineInstance{mesh=f.sharedMesh,transform=root.worldToLocalMatrix*f.transform.localToWorldMatrix}).ToArray(),true,true);
                AssetDatabase.CreateAsset(mesh,Folder+"SSD_"+card+"_"+(group++)+".asset");var g=new GameObject("Acabado combinado",typeof(MeshFilter),typeof(MeshRenderer));g.transform.SetParent(root,false);g.GetComponent<MeshFilter>().sharedMesh=mesh;g.GetComponent<MeshRenderer>().sharedMaterial=batch.Key;foreach(var f in batch)f.GetComponent<MeshRenderer>().enabled=false;
            }
        }
        static Mesh Beveled()
        {
            var vertices=new List<Vector3>();var triangles=new List<int>();var rings=new Vector3[4][];
            for(int r=0;r<4;r++)
            {
                float w=r==0||r==3?.102f:.11f,d=r==0||r==3?.137f:.145f,c=.014f,y=new[]{-.026f,-.018f,.018f,.026f}[r];
                rings[r]=new[]{new Vector3(-w+c,y,-d),new Vector3(w-c,y,-d),new Vector3(w,y,-d+c),new Vector3(w,y,d-c),new Vector3(w-c,y,d),new Vector3(-w+c,y,d),new Vector3(-w,y,d-c),new Vector3(-w,y,-d+c)};
            }
            for(int r=0;r<3;r++)for(int i=0;i<8;i++){int n=(i+1)%8,k=vertices.Count;vertices.AddRange(new[]{rings[r][i],rings[r+1][i],rings[r+1][n],rings[r][n]});triangles.AddRange(new[]{k,k+1,k+2,k,k+2,k+3});}
            for(int i=0;i<8;i++){int n=(i+1)%8,k=vertices.Count;vertices.AddRange(new[]{new Vector3(0,.026f,0),rings[3][n],rings[3][i],new Vector3(0,-.026f,0),rings[0][i],rings[0][n]});triangles.AddRange(new[]{k,k+1,k+2,k+3,k+4,k+5});}
            var mesh=new Mesh();mesh.name="SSD / biseles";mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;
        }
    }
}
