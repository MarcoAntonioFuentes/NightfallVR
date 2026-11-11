using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace CyberShield.Editor
{
    public static class CyberShieldImmersionBuilder
    {
        const string AudioFolder="Assets/CyberShield/Audio/Operator/";
        const string Materials="Assets/CyberShield/Materials/ShieldOps/";
        public static string GenerateAudio()
        {
            Directory.CreateDirectory(AudioFolder);
            Save("Click",.065f,t=>Mathf.Sin(t*2*Mathf.PI*1400)*Mathf.Exp(-t*85)*.18f);
            Save("Grab",.11f,t=>(Mathf.Sin(t*2*Mathf.PI*170)+Mathf.Sin(t*93013)*.25f)*Mathf.Exp(-t*40)*.2f);
            Save("Drop",.16f,t=>(Mathf.Sin(t*2*Mathf.PI*95)+Mathf.Sin(t*78341)*.15f)*Mathf.Exp(-t*33)*.24f);
            Save("Scan",.5f,t=>Mathf.Sin(2*Mathf.PI*(620*t+660*t*t))*Mathf.Sin(Mathf.PI*t/.5f)*.14f);
            Save("Contain",1.1f,t=>(Mathf.Sin(2*Mathf.PI*220*t)+Mathf.Sin(2*Mathf.PI*330*t)*.45f)*Mathf.Sin(Mathf.PI*t/1.1f)*.11f);
            Save("Victory",2.8f,t=>{float[] notes={523.25f,659.25f,783.99f,1046.5f};int index=Mathf.Min(3,(int)(t/.35f));float phase=Mathf.Repeat(t,.35f);return (Mathf.Sin(2*Mathf.PI*notes[index]*t)*Mathf.Exp(-phase*6)+Mathf.Sin(2*Mathf.PI*261.625f*t)*.25f)*Mathf.Min(1,t*15)*Mathf.Clamp01((2.8f-t)*2)*.14f;});
            float filtered=0;var random=new System.Random(1042);
            Save("ServerFan",4,t=>{filtered=filtered*.93f+((float)random.NextDouble()*2-1)*.07f;return filtered*.28f+Mathf.Sin(t*2*Mathf.PI*55)*.026f;});
            Save("Tension",16,t=>{float beat=Mathf.Repeat(t,2f/3f);float pulse=Mathf.Exp(-beat*11);float fade=Mathf.Min(1,t*2)*Mathf.Min(1,(16-t)*2);return fade*(Mathf.Sin(2*Mathf.PI*55*t)*(.1f+pulse*.22f)+Mathf.Sin(2*Mathf.PI*82.5f*t)*.045f+Mathf.Sin(2*Mathf.PI*110*t)*.025f);});
            AssetDatabase.Refresh();
            foreach(string name in new[]{"Click","Grab","Drop","Scan","Contain","Victory","ServerFan","Tension"})
            {
                string path=AudioFolder+name+".wav";var importer=(AudioImporter)AssetImporter.GetAtPath(path);if(!importer)continue;
                importer.forceToMono=true;var settings=importer.defaultSampleSettings;settings.loadType=name=="Tension"||name=="ServerFan"?AudioClipLoadType.CompressedInMemory:AudioClipLoadType.DecompressOnLoad;settings.compressionFormat=AudioCompressionFormat.Vorbis;settings.quality=.65f;importer.defaultSampleSettings=settings;importer.SaveAndReimport();
            }
            return "Ocho sonidos originales generados e importados.";
        }
        internal static void Save(string name,float duration,Func<float,float> sample)
        {
            string path=AudioFolder+name+".wav";if(File.Exists(path))return;
            const int rate=24000;int count=Mathf.RoundToInt(duration*rate);
            using(var writer=new BinaryWriter(File.Create(path)))
            {
                writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));writer.Write(36+count*2);writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));writer.Write(16);writer.Write((short)1);writer.Write((short)1);writer.Write(rate);writer.Write(rate*2);writer.Write((short)2);writer.Write((short)16);writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));writer.Write(count*2);
                for(int i=0;i<count;i++)writer.Write((short)(Mathf.Clamp(sample(i/(float)rate),-1,1)*short.MaxValue));
            }
        }
        public static string InstallArms()
        {
            var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();if(!scene.path.EndsWith("CyberShieldVR_ShieldOps.unity"))throw new Exception("Abre Shield Ops.");
            if(UnityEngine.Object.FindFirstObjectByType<CyberShieldOperatorArms>())throw new Exception("Los brazos ya están instalados.");
            var rig=UnityEngine.Object.FindFirstObjectByType<Unity.XR.CoreUtils.XROrigin>();
            var root=new GameObject("CYBERSHIELD / OPERADOR VR");root.transform.SetParent(rig.transform,false);
            var arms=root.AddComponent<CyberShieldOperatorArms>();arms.head=rig.Camera.transform;
            arms.controllers=new Transform[2];arms.wrists=new Transform[2];arms.upperArms=new Transform[2];arms.forearms=new Transform[2];arms.elbows=new Transform[2];arms.armRoots=new Transform[2];arms.inputs=new XRBaseInputInteractor[2];
            var fabric=Mat("Operador traje",new Color(.045f,.075f,.11f),false,.22f);var glove=Mat("Operador guante",new Color(.055f,.085f,.12f),false,.35f);var armor=Mat("Operador armadura",new Color(.13f,.2f,.26f),false,.65f);var cyan=Mat("Operador luz",new Color(.02f,.75f,1),true,0);
            var fingers=new List<Transform>();var hands=new List<int>();var indices=new List<bool>();
            for(int hand=0;hand<2;hand++)
            {
                string side=hand==0?"Left":"Right";
                var controller=rig.GetComponentsInChildren<Transform>(true).First(t=>t.name==side+" Controller");arms.controllers[hand]=controller;
                arms.inputs[hand]=controller.GetComponentsInChildren<XRBaseInputInteractor>(true).FirstOrDefault(t=>t is NearFarInteractor)??controller.GetComponentInChildren<XRBaseInputInteractor>(true);
                var model=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Samples/XR Hands/1.8.1/HandVisualizer/Models/"+side+"Hand.fbx");
                var instance=(GameObject)PrefabUtility.InstantiatePrefab(model,controller);instance.name="Guante / "+side;instance.transform.localPosition=new Vector3(0,-.015f,.03f);instance.transform.localRotation=Quaternion.identity;instance.transform.localScale=Vector3.one;
                foreach(var renderer in instance.GetComponentsInChildren<SkinnedMeshRenderer>()){renderer.sharedMaterial=glove;renderer.localBounds=new Bounds(Vector3.zero,Vector3.one*.5f);}
                foreach(var bone in instance.GetComponentsInChildren<Transform>(true))
                {
                    if(bone.name.EndsWith("Wrist"))arms.wrists[hand]=bone;
                    if(!bone.name.Contains("Thumb")&&(bone.name.Contains("Proximal")||bone.name.Contains("Intermediate")||bone.name.Contains("Distal"))){fingers.Add(bone);hands.Add(hand);indices.Add(bone.name.Contains("Index"));}
                }
                var limbRoot=new GameObject("Brazo / "+side).transform;limbRoot.SetParent(root.transform,false);arms.armRoots[hand]=limbRoot;
                arms.upperArms[hand]=Primitive(limbRoot,"Manga",PrimitiveType.Capsule,Vector3.zero,Vector3.one*.1f,fabric).transform;
                arms.forearms[hand]=Primitive(limbRoot,"Antebrazo",PrimitiveType.Capsule,Vector3.zero,Vector3.one*.1f,armor).transform;
                arms.elbows[hand]=Primitive(limbRoot,"Codo",PrimitiveType.Sphere,Vector3.zero,Vector3.one*.085f,fabric).transform;
                var cuff=Primitive(arms.wrists[hand],"Puño / protección",PrimitiveType.Cylinder,Vector3.zero,new Vector3(.082f,.012f,.082f),armor);cuff.transform.localRotation=Quaternion.Euler(90,0,0);
                var ring=Primitive(arms.wrists[hand],"Puño / luz",PrimitiveType.Cylinder,new Vector3(0,0,.008f),new Vector3(.083f,.003f,.083f),cyan);ring.transform.localRotation=Quaternion.Euler(90,0,0);
                Primitive(instance.transform,"Protección dorsal",PrimitiveType.Cube,new Vector3(0,.005f,.012f),new Vector3(.058f,.008f,.055f),armor);
                for(int stripe=-1;stripe<=1;stripe+=2)Primitive(instance.transform,"Guante / luz",PrimitiveType.Cube,new Vector3(stripe*.018f,.01f,.012f),new Vector3(.004f,.003f,.035f),cyan);
                foreach(var t in controller.GetComponentsInChildren<Transform>(true))if(t.name=="UniversalController"){t.gameObject.SetActive(false);PrefabUtility.RecordPrefabInstancePropertyModifications(t.gameObject);}
            }
            arms.fingerJoints=fingers.ToArray();arms.jointHands=hands.ToArray();arms.indexJoints=indices.ToArray();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();return "Guantes articulados y brazos de operador instalados.";
        }
        public static string InstallSound()
        {
            var game=UnityEngine.Object.FindFirstObjectByType<CyberShieldGame>();if(!game.gameObject.scene.path.EndsWith("CyberShieldVR_ShieldOps.unity"))throw new Exception("Abre Shield Ops.");
            if(game.GetComponent<CyberShieldSoundDirector>())throw new Exception("El audio ya está instalado.");
            foreach(string name in new[]{"Click","Grab","Drop","Scan","Contain","Victory","ServerFan","Tension"})if(!Clip(name))throw new Exception("Genera primero los sonidos.");
            var audio=game.gameObject.AddComponent<CyberShieldSoundDirector>();audio.game=game;audio.hands=game.GetComponent<CyberShieldHandsOn>();
            audio.click=Clip("Click");audio.grab=Clip("Grab");audio.drop=Clip("Drop");audio.scan=Clip("Scan");audio.contain=Clip("Contain");audio.victory=Clip("Victory");
            var rig=UnityEngine.Object.FindFirstObjectByType<Unity.XR.CoreUtils.XROrigin>();audio.ui=Source(rig.Camera.transform,"Audio / interfaz",null,false,0,1);audio.scoreBed=Source(rig.Camera.transform,"Audio / tensión",Clip("Tension"),true,0,0);
            audio.cardSources=audio.hands.cards.Select(c=>Source(c.transform,"Audio / tarjeta",null,false,1,1)).ToArray();
            audio.fans=new AudioSource[2];for(int i=0;i<2;i++){audio.fans[i]=Source(game.transform,"Audio / servidor",Clip("ServerFan"),true,1,.12f);audio.fans[i].transform.position=new Vector3(i==0?-4.08f:4.08f,1.3f,2.4f);}
            audio.buttons=UnityEngine.Object.FindObjectsByType<Button>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(b=>b.gameObject.scene==game.gameObject.scene).ToArray();audio.haptics=rig.GetComponentsInChildren<HapticImpulsePlayer>(true);
            EditorSceneManager.MarkSceneDirty(game.gameObject.scene);EditorSceneManager.SaveOpenScenes();return "Tarjetas, interfaz, servidores, música y cierre conectados.";
        }
        public static string PolishGloves()
        {
            var arms=UnityEngine.Object.FindFirstObjectByType<CyberShieldOperatorArms>();arms.game=UnityEngine.Object.FindFirstObjectByType<CyberShieldGame>();
            foreach(var wrist in arms.wrists)foreach(var part in wrist.GetComponentsInChildren<Transform>(true))
            {
                if(part.name=="Puño / protección")part.localScale=new Vector3(.062f,.008f,.062f);
                if(part.name=="Puño / luz")part.localScale=new Vector3(.063f,.0018f,.063f);
            }
            var model=arms.controllers[0].GetComponentsInChildren<Transform>(true).First(t=>t.name=="Guante / Left");
            var material=AssetDatabase.LoadAssetAtPath<Material>(Materials+"Operador luz.mat");arms.missionLeds=new Renderer[3];
            for(int i=0;i<3;i++)
            {
                string name="Mission LED "+i;var part=model.Find(name);
                if(!part)part=Primitive(model,name,PrimitiveType.Cube,new Vector3(-.017f+i*.017f,.012f,.012f),new Vector3(.01f,.003f,.018f),material).transform;
                arms.missionLeds[i]=part.GetComponent<Renderer>();
            }
            var armor=AssetDatabase.LoadAssetAtPath<Material>(Materials+"Operador armadura.mat");armor.SetFloat("_Smoothness",.42f);EditorUtility.SetDirty(armor);
            foreach(var controller in arms.controllers)foreach(var renderer in controller.GetComponentsInChildren<SkinnedMeshRenderer>(true))PrefabUtility.RecordPrefabInstancePropertyModifications(renderer);
            foreach(var controller in arms.controllers)foreach(var t in controller.GetComponentsInChildren<Transform>(true))if(t.name.StartsWith("Guante /")){PrefabUtility.RecordPrefabInstancePropertyModifications(t);PrefabUtility.RecordPrefabInstancePropertyModifications(t.gameObject);}
            EditorSceneManager.MarkSceneDirty(arms.gameObject.scene);EditorSceneManager.SaveOpenScenes();AssetDatabase.SaveAssets();return "Puños afinados y progreso de misión añadido al guante izquierdo.";
        }
        static AudioClip Clip(string name)=>AssetDatabase.LoadAssetAtPath<AudioClip>(AudioFolder+name+".wav");
        static AudioSource Source(Transform parent,string name,AudioClip clip,bool loop,float spatial,float volume){var g=new GameObject(name);g.transform.SetParent(parent,false);var s=g.AddComponent<AudioSource>();s.clip=clip;s.loop=loop;s.playOnAwake=false;s.spatialBlend=spatial;s.volume=volume;s.minDistance=.6f;s.maxDistance=8;s.rolloffMode=AudioRolloffMode.Linear;s.dopplerLevel=0;return s;}
        static GameObject Primitive(Transform parent,string name,PrimitiveType type,Vector3 p,Vector3 scale,Material material){var g=GameObject.CreatePrimitive(type);g.name=name;g.transform.SetParent(parent,false);g.transform.localPosition=p;g.transform.localScale=scale;g.GetComponent<Renderer>().sharedMaterial=material;UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());return g;}
        static Material Mat(string name,Color color,bool unlit,float smooth){var path=Materials+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(m)return m;m=new Material(Shader.Find(unlit?"Universal Render Pipeline/Unlit":"Universal Render Pipeline/Lit"));m.color=color;if(!unlit)m.SetFloat("_Smoothness",smooth);AssetDatabase.CreateAsset(m,path);return m;}
    }
}
