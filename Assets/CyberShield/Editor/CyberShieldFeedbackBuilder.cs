using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using static CyberShield.Editor.CyberShieldTacticalBuilder;

namespace CyberShield.Editor
{
    public static class CyberShieldFeedbackBuilder
    {
        static readonly string[] Names={"Inicio","Fraude","Contencion","Restauracion","Victoria","Revision"};
        const string Folder="Assets/CyberShield/Audio/Radio/";
        public static string ImportRadio()
        {
            foreach(var name in Names)
            {
                string path=Folder+name+".wav";AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
                var importer=AssetImporter.GetAtPath(path) as AudioImporter;if(!importer)throw new Exception("Falta "+name);
                importer.forceToMono=true;var settings=importer.defaultSampleSettings;settings.preloadAudioData=true;settings.loadType=AudioClipLoadType.DecompressOnLoad;settings.compressionFormat=AudioCompressionFormat.Vorbis;settings.quality=.75f;importer.defaultSampleSettings=settings;importer.SaveAndReimport();
            }
            return "Seis comunicaciones de radio importadas.";
        }
        public static string Install()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new Exception("Detén Play.");var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();if(!scene.path.EndsWith("CyberShieldVR_ShieldOps.unity"))throw new Exception("Abre Shield Ops.");
            if(UnityEngine.Object.FindObjectsByType<CyberShieldMissionFeedback>(FindObjectsInactive.Include,FindObjectsSortMode.None).Any())throw new Exception("El sistema de radio ya existe.");
            var clips=Names.Select(n=>AssetDatabase.LoadAssetAtPath<AudioClip>(Folder+n+".wav")).ToArray();if(clips.Any(c=>!c))throw new Exception("Importa las comunicaciones primero.");
            var root=new GameObject("CYBERSHIELD / MISSION RADIO");root.SetActive(false);var p=root.AddComponent<CyberShieldMissionFeedback>();p.game=UnityEngine.Object.FindFirstObjectByType<CyberShieldGame>();p.sound=p.game.GetComponent<CyberShieldSoundDirector>();p.breach=p.game.GetComponent<CyberShieldBreachAtmosphere>();p.head=UnityEngine.Object.FindFirstObjectByType<Unity.XR.CoreUtils.XROrigin>().Camera.transform;p.haptics=p.sound.haptics;p.lines=clips;
            var voice=new GameObject("Marta / radio");voice.transform.SetParent(p.head,false);p.radio=voice.AddComponent<AudioSource>();p.radio.playOnAwake=false;p.radio.spatialBlend=0;p.radio.volume=.62f;p.radio.priority=64;voice.AddComponent<AudioLowPassFilter>().cutoffFrequency=4200;voice.AddComponent<AudioHighPassFilter>().cutoffFrequency=180;
            var card=new GameObject("Comunicación / holograma",typeof(RectTransform),typeof(Canvas),typeof(CanvasGroup),typeof(Image));card.transform.SetParent(root.transform,false);card.GetComponent<RectTransform>().sizeDelta=new Vector2(900,220);card.GetComponent<Canvas>().renderMode=RenderMode.WorldSpace;card.GetComponent<Image>().color=new Color(.012f,.04f,.065f,.96f);card.GetComponent<Image>().raycastTarget=false;p.card=card;p.cardFade=card.GetComponent<CanvasGroup>();p.cardFade.blocksRaycasts=false;
            p.accent=Rect(card.transform,new Vector2(-442,0),new Vector2(8,220),Color.cyan);var icon=new GameObject("Símbolo",typeof(RectTransform),typeof(CanvasRenderer),typeof(CyberShieldGlyph));icon.transform.SetParent(card.transform,false);icon.GetComponent<RectTransform>().anchoredPosition=new Vector2(-350,0);icon.GetComponent<RectTransform>().sizeDelta=Vector2.one*135;p.symbol=icon.GetComponent<CyberShieldGlyph>();p.symbol.raycastTarget=false;p.symbol.color=Color.white;
            p.speaker=SharedText(card.transform,"MARTA / TI",new Vector2(-20,65),new Vector2(580,45),29,new Color(.35f,.75f,.9f));p.caption=SharedText(card.transform,"",new Vector2(-10,-2),new Vector2(590,85),43,Color.white);p.gain=SharedText(card.transform,"",new Vector2(355,0),new Vector2(150,130),65,new Color(.15f,1,.6f));
            var probe=UnityEngine.Object.FindFirstObjectByType<CyberShieldForensicProbe>();var original=probe.bodyLabel.GetComponentInParent<Button>(true);var toggle=UnityEngine.Object.Instantiate(original.gameObject,original.transform.parent);toggle.name="Comunicaciones de radio";toggle.transform.position=original.transform.position+Vector3.right*.86f;var button=toggle.GetComponent<Button>();while(button.onClick.GetPersistentEventCount()>0)UnityEventTools.RemovePersistentListener(button.onClick,0);button.onClick.RemoveAllListeners();UnityEventTools.AddPersistentListener(button.onClick,p.ToggleRadio);p.radioLabel=toggle.GetComponentInChildren<TMP_Text>();p.radioLabel.text="RADIO: SÍ";p.sound.buttons=p.sound.buttons.Concat(new[]{button}).Distinct().ToArray();
            p.recoveryRings=new LineRenderer[2];var material=AssetDatabase.LoadAssetAtPath<Material>("Assets/CyberShield/Materials/ShieldOps/SSD luz.mat");
            for(int i=0;i<2;i++){var g=new GameObject("Servidor / recuperación "+i,typeof(LineRenderer));g.transform.SetParent(root.transform,false);var line=g.GetComponent<LineRenderer>();line.sharedMaterial=material;line.useWorldSpace=true;line.positionCount=49;line.startWidth=line.endWidth=.014f;line.enabled=false;p.recoveryRings[i]=line;}
            p.game.GetComponent<CyberShieldArcade>().useCinematicFeedback=true;card.SetActive(false);root.SetActive(true);ConfigureControls(p.game);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);return "Radio de TI, confirmaciones de misión y recuperación de servidores instaladas.";
        }
        public static string ArrangeControls(){var game=UnityEngine.Object.FindFirstObjectByType<CyberShieldGame>();ConfigureControls(game);EditorSceneManager.MarkSceneDirty(game.gameObject.scene);EditorSceneManager.SaveOpenScenes();return "Controles de audio, vista y radio ampliados.";}
        static void ConfigureControls(CyberShieldGame game)
        {
            var r=(RectTransform)game.GetComponent<CyberShieldAtmosphere>().muteButton.transform.parent;r.sizeDelta=new Vector2(600,390);var pos=r.position;pos.y=.35f;r.position=pos;r.anchoredPosition3D=r.parent?r.parent.InverseTransformPoint(pos):pos;
            var layout=r.GetComponent<VerticalLayoutGroup>();layout.padding=new RectOffset(12,12,18,18);layout.spacing=12;layout.childControlHeight=layout.childControlWidth=true;layout.childForceExpandHeight=false;layout.childForceExpandWidth=true;
            foreach(var button in r.GetComponentsInChildren<Button>(true)){var element=button.GetComponent<LayoutElement>();if(!element)element=button.gameObject.AddComponent<LayoutElement>();element.minHeight=element.preferredHeight=110;element.flexibleHeight=0;}
            LayoutRebuilder.ForceRebuildLayoutImmediate(r);EditorUtility.SetDirty(r);
        }
        static Image Rect(Transform parent,Vector2 position,Vector2 size,Color color){var g=new GameObject("Acento",typeof(RectTransform),typeof(Image));g.transform.SetParent(parent,false);g.GetComponent<RectTransform>().anchoredPosition=position;g.GetComponent<RectTransform>().sizeDelta=size;g.GetComponent<Image>().color=color;g.GetComponent<Image>().raycastTarget=false;return g.GetComponent<Image>();}
    }
}
