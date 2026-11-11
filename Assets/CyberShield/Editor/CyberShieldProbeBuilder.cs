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
    public static class CyberShieldProbeBuilder
    {
        public static string Install()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new Exception("Detén Play.");var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();if(!scene.path.EndsWith("CyberShieldVR_ShieldOps.unity"))throw new Exception("Abre Shield Ops.");
            var existing=UnityEngine.Object.FindObjectsByType<CyberShieldForensicProbe>(FindObjectsInactive.Include,FindObjectsSortMode.None).FirstOrDefault();if(existing)return Complete(existing);
            var root=new GameObject("CYBERSHIELD / FORENSIC PROBE");root.SetActive(false);var p=root.AddComponent<CyberShieldForensicProbe>();p.game=UnityEngine.Object.FindFirstObjectByType<CyberShieldGame>();p.flow=UnityEngine.Object.FindFirstObjectByType<CyberShieldReferenceExperience>();p.tactical=UnityEngine.Object.FindFirstObjectByType<CyberShieldTacticalNetwork>();p.arms=UnityEngine.Object.FindFirstObjectByType<CyberShieldOperatorArms>();
            var sound=p.game.GetComponent<CyberShieldSoundDirector>();p.audioSource=sound.ui;p.scanSound=sound.scan;
            var panel=p.tactical.details.transform;
            p.tactical.heading.rectTransform.anchoredPosition=new Vector2(-50,250);p.tactical.heading.rectTransform.sizeDelta=new Vector2(510,65);
            var scanner=new GameObject("Analizador / progreso",typeof(RectTransform),typeof(CanvasRenderer),typeof(CyberShieldPanelArt));scanner.transform.SetParent(panel,false);scanner.GetComponent<RectTransform>().anchoredPosition=new Vector2(270,255);scanner.GetComponent<RectTransform>().sizeDelta=Vector2.one*90;p.scanner=scanner.GetComponent<CyberShieldPanelArt>();p.scanner.radar=true;p.scanner.raycastTarget=false;
            p.analyzeButton=SharedButton(panel,"ANALIZAR",new Vector2(-155,-205),new Vector2(280,85));p.analyzeLabel=p.analyzeButton.GetComponentInChildren<TMP_Text>();p.analyzeLabel.fontSize=28;UnityEventTools.AddPersistentListener(p.analyzeButton.onClick,p.BeginScan);Feel(p.analyzeButton);
            p.flow.isolateButton.GetComponent<RectTransform>().anchoredPosition=new Vector2(155,-205);p.flow.isolateButton.GetComponent<RectTransform>().sizeDelta=new Vector2(280,85);p.flow.isolationAction.fontSize=28;p.flow.isolationAction.rectTransform.sizeDelta=new Vector2(260,75);
            return Complete(p);
        }
        static string Complete(CyberShieldForensicProbe p)
        {
            if(p.grade)return "El analizador ya está instalado.";var root=p.gameObject;var scene=p.game.gameObject.scene;var sound=p.game.GetComponent<CyberShieldSoundDirector>();
            p.nodeSymbols=new CyberShieldGlyph[p.tactical.selectionFrames.Length];for(int i=0;i<p.nodeSymbols.Length;i++)p.nodeSymbols[i]=p.tactical.selectionFrames[i].GetComponentInParent<Canvas>(true).GetComponentInChildren<CyberShieldGlyph>(true);
            var sweep=new GameObject("Analizador / barrido 3D",typeof(LineRenderer));sweep.transform.SetParent(root.transform,false);p.sweep=sweep.GetComponent<LineRenderer>();p.sweep.sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/CyberShield/Materials/ShieldOps/SSD luz.mat");p.sweep.positionCount=2;p.sweep.startWidth=p.sweep.endWidth=.005f;p.sweep.enabled=false;
            var atmosphere=p.game.GetComponent<CyberShieldAtmosphere>();var body=UnityEngine.Object.Instantiate(atmosphere.muteButton.gameObject,atmosphere.muteButton.transform.parent);body.name="Vista del operador";body.transform.position=atmosphere.muteButton.transform.position+Vector3.down*.18f;var bodyButton=body.GetComponent<Button>();while(bodyButton.onClick.GetPersistentEventCount()>0)UnityEventTools.RemovePersistentListener(bodyButton.onClick,0);bodyButton.onClick.RemoveAllListeners();UnityEventTools.AddPersistentListener(bodyButton.onClick,p.ToggleBody);p.bodyLabel=body.GetComponentInChildren<TMP_Text>();p.bodyLabel.text="SOLO GUANTES";
            sound.buttons=sound.buttons.Concat(new[]{p.analyzeButton,bodyButton}).Distinct().ToArray();
            var badge=Rect(p.flow.victoryPanel.transform,new Vector2(365,210),new Vector2(200,270),new Color(.025f,.13f,.19f));SharedText(badge.transform,"RANGO",new Vector2(0,105),new Vector2(180,45),28,new Color(.1f,.8f,1));p.grade=SharedText(badge.transform,"S",new Vector2(0,30),new Vector2(170,140),115,Color.white);p.scanSummary=SharedText(badge.transform,"EQUIPOS\n0/8",new Vector2(0,-72),new Vector2(185,90),26,new Color(.5f,.8f,.9f));
            p.stars=new Image[3];for(int i=0;i<3;i++){p.stars[i]=Rect(badge.transform,new Vector2(-60+i*60,-120),new Vector2(38,38),new Color(1,.7f,.1f));var g=new GameObject("Estrella",typeof(RectTransform),typeof(CanvasRenderer),typeof(CyberShieldGlyph));g.transform.SetParent(p.stars[i].transform,false);g.GetComponent<RectTransform>().sizeDelta=Vector2.one*32;g.GetComponent<CyberShieldGlyph>().symbol=12;g.GetComponent<CyberShieldGlyph>().color=Color.white;g.GetComponent<CyberShieldGlyph>().raycastTarget=false;}
            root.SetActive(true);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);return "Analizador, rango de misión y modo de vista instalados.";
        }
        static Image Rect(Transform parent,Vector2 position,Vector2 size,Color color){var g=new GameObject("Placa",typeof(RectTransform),typeof(Image));g.transform.SetParent(parent,false);g.GetComponent<RectTransform>().anchoredPosition=position;g.GetComponent<RectTransform>().sizeDelta=size;g.GetComponent<Image>().color=color;g.GetComponent<Image>().raycastTarget=false;return g.GetComponent<Image>();}
        static void Feel(Button button){var feel=button.gameObject.AddComponent<CyberShieldButtonFeel>();var g=new GameObject("Marco",typeof(RectTransform),typeof(CanvasRenderer),typeof(CyberShieldPanelArt));g.transform.SetParent(button.transform,false);var r=g.GetComponent<RectTransform>();r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;feel.outline=g.GetComponent<CyberShieldPanelArt>();feel.outline.raycastTarget=false;}
    }
}
