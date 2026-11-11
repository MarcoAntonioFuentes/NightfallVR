using System;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using static CyberShield.Editor.CyberShieldTacticalBuilder;

namespace CyberShield.Editor
{
    public static class CyberShieldVisualBuilder
    {
        static Color cyan=new Color(.03f,.75f,1), ink=new Color(.025f,.09f,.15f);
        [MenuItem("CyberShield/Instalar presentación visual")]
        public static string Install()
        {
            var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if(!scene.path.EndsWith("CyberShieldVR_ShieldOps.unity"))throw new Exception("Abre Shield Ops.");
            if(UnityEngine.Object.FindFirstObjectByType<CyberShieldVisualDirector>())throw new Exception("La presentación visual ya existe.");
            var root=new GameObject("CYBERSHIELD / VISUAL MISSION");var v=root.AddComponent<CyberShieldVisualDirector>();
            v.flow=UnityEngine.Object.FindFirstObjectByType<CyberShieldReferenceExperience>();v.game=v.flow.game;v.mail=UnityEngine.Object.FindFirstObjectByType<CyberShieldMailInvestigation>();v.tactical=UnityEngine.Object.FindFirstObjectByType<CyberShieldTacticalNetwork>();
            v.game.GetComponent<CyberShieldArcade>().screenInvestigationOnly=true;
            v.game.corporateScenario.evidenceText.gameObject.SetActive(false);v.game.corporateScenario.colleagueText.gameObject.SetActive(false);v.game.corporateScenario.serviceText.gameObject.SetActive(false);v.game.corporateScenario.scannerText.gameObject.SetActive(false);
            v.steps=new Image[3];v.stepChecks=new Image[3];
            var ribbon=v.tactical.banner.transform.parent;v.tactical.banner.gameObject.SetActive(false);
            for(int i=0;i<3;i++)
            {
                var step=Rect(ribbon,new Vector2(-470+i*470,0),new Vector2(400,138),ink);v.steps[i]=step;
                Glyph(step.transform,i==0?0:i==1?9:6,new Vector2(-125,0),90);
                SharedText(step.transform,new[]{"DETECTA","AÍSLA","RECUPERA"}[i],new Vector2(65,0),new Vector2(240,70),36,Color.white);
                v.stepChecks[i]=Rect(step.transform,new Vector2(-175,-43),new Vector2(36,36),new Color(.02f,.25f,.14f));Glyph(v.stepChecks[i].transform,7,Vector2.zero,30);
            }
            v.score=SharedText(ribbon,"0/100",new Vector2(750,0),new Vector2(170,110),57,Color.white);
            var mail=v.mail;
            mail.message.rectTransform.anchoredPosition=new Vector2(-185,165);mail.message.rectTransform.sizeDelta=new Vector2(400,180);mail.message.fontSize=34;
            Glyph(mail.viewer.transform,0,new Vector2(-475,170),100);
            mail.finding.rectTransform.anchoredPosition=new Vector2(-265,-55);mail.finding.rectTransform.sizeDelta=new Vector2(560,150);mail.finding.fontSize=34;
            v.evidenceChecks=new Image[3];
            for(int i=0;i<3;i++)
            {
                var frame=mail.inspectionFrames[i];var t=frame.GetComponentInChildren<TMP_Text>();t.text=new[]{"IDENTIDAD","ENLACE","CONTEXTO"}[i];t.rectTransform.anchoredPosition=new Vector2(30,0);t.rectTransform.sizeDelta=new Vector2(320,60);t.fontSize=32;
                Glyph(frame.transform,i+1,new Vector2(-180,0),50);
                v.evidenceChecks[i]=Rect(frame.transform,new Vector2(205,23),new Vector2(30,30),new Color(.03f,.3f,.2f));Glyph(v.evidenceChecks[i].transform,7,Vector2.zero,25);
                var reason=mail.reasonFrames[i].GetComponentInChildren<TMP_Text>();reason.text=new[]{"IDENTIDAD","DOMINIO EXTERNO","HORARIO"}[i];reason.fontSize=28;
            }
            foreach(var text in mail.viewer.GetComponentsInChildren<TMP_Text>(true))if(text.text=="¿QUÉ PRUEBA EL ENGAÑO?")text.text="MOTIVO";
            mail.reportButton.GetComponentInChildren<TMP_Text>().text="REPORTAR";
            v.tactical.activity.gameObject.SetActive(false);v.tactical.heading.fontSize=40;
            v.telemetryBars=new Image[3];v.telemetryValues=new TMP_Text[3];
            var detail=v.tactical.details.transform;
            for(int i=0;i<3;i++)
            {
                var row=Rect(detail,new Vector2(0,155-i*120),new Vector2(590,102),ink);Glyph(row.transform,new[]{4,5,6}[i],new Vector2(-245,0),60);
                v.telemetryValues[i]=SharedText(row.transform,"—",new Vector2(35,18),new Vector2(440,45),29,Color.white);
                Rect(row.transform,new Vector2(35,-28),new Vector2(440,9),new Color(.06f,.2f,.26f));
                v.telemetryBars[i]=Rect(row.transform,new Vector2(35,-28),new Vector2(440,9),cyan);Fill(v.telemetryBars[i]);
            }
            v.flow.isolateButton.GetComponent<RectTransform>().anchoredPosition=new Vector2(0,-205);
            v.backupBadges=new Image[3];v.backupStates=new TMP_Text[3];
            for(int i=0;i<3;i++)
            {
                var card=v.flow.backupFrames[i];
                foreach(Transform child in card.transform)if(child.name=="Trazo")child.gameObject.SetActive(false);
                var text=card.GetComponentInChildren<TMP_Text>();text.text=new[]{"AYER 18:00\nOFFLINE","HOY 10:15\nPOSTINCIDENTE","RED\nSINCRONIZADA"}[i];text.fontSize=27;text.rectTransform.anchoredPosition=new Vector2(0,-30);text.rectTransform.sizeDelta=new Vector2(245,100);
                Glyph(card.transform,6,new Vector2(0,62),70);SharedText(card.transform,((char)('A'+i)).ToString(),new Vector2(-96,110),new Vector2(50,40),34,Color.white);
                v.backupBadges[i]=Rect(card.transform,new Vector2(0,-105),new Vector2(242,40),ink);v.backupStates[i]=SharedText(v.backupBadges[i].transform,"SIN ANALIZAR",Vector2.zero,new Vector2(232,40),22,Color.white);
            }
            var meter=Rect(v.flow.recoveryPanel.transform,new Vector2(0,-285),new Vector2(840,20),ink);v.restoreMeter=meter.gameObject;
            v.restoreFill=Rect(meter.transform,Vector2.zero,new Vector2(840,20),cyan);Fill(v.restoreFill);
            BuildPath(root.transform,v);
            for(int i=0;i<v.tactical.selectionFrames.Length;i++)Glyph(v.tactical.selectionFrames[i].transform.parent,i==2?10:8,new Vector2(0,140),70);
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();return "Presentación con iconos, indicadores, estados y ruta visual instalada.";
        }
        static void BuildPath(Transform root,CyberShieldVisualDirector v)
        {
            string path="Assets/CyberShield/Materials/ShieldOps/Ruta visual.mat";var material=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(!material){material=new Material(Shader.Find("Universal Render Pipeline/Unlit"));material.color=cyan;AssetDatabase.CreateAsset(material,path);}
            v.floorMarkers=new Transform[9];v.markerRenderers=new Renderer[9];
            for(int i=0;i<9;i++){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name="Ruta / paso "+i;g.transform.SetParent(root);g.transform.localScale=new Vector3(.11f,.005f,.05f);UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());g.GetComponent<Renderer>().sharedMaterial=material;v.floorMarkers[i]=g.transform;v.markerRenderers[i]=g.GetComponent<Renderer>();g.SetActive(false);}
        }
        static Image Rect(Transform parent,Vector2 p,Vector2 size,Color color){var g=new GameObject("Indicador",typeof(RectTransform),typeof(Image));g.transform.SetParent(parent,false);var r=g.GetComponent<RectTransform>();r.anchoredPosition=p;r.sizeDelta=size;var image=g.GetComponent<Image>();image.color=color;image.raycastTarget=false;return image;}
        static void Fill(Image image)
        {
            const string path="Assets/CyberShield/Materials/ShieldOps/Barra sprite.asset";
            var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if(!sprite)
            {
                var texture=new Texture2D(2,2,TextureFormat.RGBA32,false);texture.SetPixels(new[]{Color.white,Color.white,Color.white,Color.white});texture.Apply();
                AssetDatabase.CreateAsset(texture,"Assets/CyberShield/Materials/ShieldOps/Barra textura.asset");
                sprite=Sprite.Create(texture,new UnityEngine.Rect(0,0,2,2),Vector2.one*.5f);AssetDatabase.CreateAsset(sprite,path);
            }
            image.sprite=sprite;image.type=Image.Type.Filled;image.fillMethod=Image.FillMethod.Horizontal;image.fillOrigin=0;image.fillAmount=0;
        }
        static void Glyph(Transform parent,int symbol,Vector2 p,float size){var g=new GameObject("Icono",typeof(RectTransform),typeof(CyberShieldGlyph));g.transform.SetParent(parent,false);var t=g.GetComponent<RectTransform>();t.anchoredPosition=p;t.sizeDelta=Vector2.one*size;var icon=g.GetComponent<CyberShieldGlyph>();icon.symbol=symbol;icon.color=Color.white;icon.raycastTarget=false;}
    }
}
