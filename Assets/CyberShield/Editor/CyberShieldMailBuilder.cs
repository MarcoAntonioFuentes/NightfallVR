using System;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using static CyberShield.Editor.CyberShieldTacticalBuilder;

namespace CyberShield.Editor
{
    public static class CyberShieldMailBuilder
    {
        [MenuItem("CyberShield/Instalar investigación de correo")]
        public static string Install()
        {
            var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if(!scene.path.EndsWith("CyberShieldVR_ShieldOps.unity"))throw new Exception("Abre Shield Ops primero.");
            if(UnityEngine.Object.FindFirstObjectByType<CyberShieldMailInvestigation>())throw new Exception("El correo investigable ya está instalado.");
            var flow=UnityEngine.Object.FindFirstObjectByType<CyberShieldReferenceExperience>();
            var root=new GameObject("CYBERSHIELD / FORENSIC MAIL").transform;
            var mail=root.gameObject.AddComponent<CyberShieldMailInvestigation>();mail.game=flow.game;
            var panel=SharedPanel(root,"Correo / investigación",new Vector3(0,1.8f,1.1f),new Vector2(1180,830),.0019f);mail.viewer=panel.gameObject;
            mail.subject=SharedText(panel,"ABRE UN CORREO",new Vector2(0,345),new Vector2(1120,70),39,new Color(.05f,.8f,1));
            mail.message=SharedText(panel,"",new Vector2(-265,145),new Vector2(560,300),32,new Color(.82f,.9f,.97f));mail.message.alignment=TextAlignmentOptions.TopLeft;
            mail.finding=SharedText(panel,"SELECCIONA UNA PISTA",new Vector2(-265,-125),new Vector2(560,240),27,new Color(.15f,.85f,1));mail.finding.alignment=TextAlignmentOptions.TopLeft;
            mail.inspectionFrames=new Image[3];mail.reasonFrames=new Image[3];
            string[] clues={"01 / REMITENTE","02 / DESTINO REAL","03 / CONTEXTO"};
            for(int i=0;i<3;i++)
            {
                var b=SharedButton(panel,clues[i],new Vector2(315,245-i*95),new Vector2(470,75));b.GetComponentInChildren<TMP_Text>().fontSize=28;mail.inspectionFrames[i]=b.GetComponent<Image>();UnityEventTools.AddIntPersistentListener(b.onClick,mail.Inspect,i);
            }
            SharedText(panel,"¿QUÉ PRUEBA EL ENGAÑO?",new Vector2(315,-5),new Vector2(470,55),27,Color.white);
            string[] reasons={"PARECE UN CORREO DE TI","PIDE CLAVES FUERA DE LA EMPRESA","LA HORA DEL MENSAJE"};
            for(int i=0;i<3;i++)
            {
                var b=SharedButton(panel,reasons[i],new Vector2(315,-65-i*65),new Vector2(470,55));b.GetComponentInChildren<TMP_Text>().fontSize=24;mail.reasonFrames[i]=b.GetComponent<Image>();UnityEventTools.AddIntPersistentListener(b.onClick,mail.SelectReason,i);
            }
            mail.status=SharedText(panel,"",new Vector2(0,-270),new Vector2(1120,70),26,new Color(1,.55f,.1f));
            mail.reportButton=SharedButton(panel,"REPORTAR PHISHING",new Vector2(0,-355),new Vector2(1030,80));mail.reportButton.GetComponent<Image>().color=new Color(.7f,.17f,.035f);UnityEventTools.AddPersistentListener(mail.reportButton.onClick,mail.Report);
            for(int i=0;i<3;i++)
            {
                var b=mail.game.choices[i];
                while(b.onClick.GetPersistentEventCount()>0)UnityEventTools.RemovePersistentListener(b.onClick,0);
                UnityEventTools.AddIntPersistentListener(b.onClick,mail.Open,i);
            }
            panel.gameObject.SetActive(false);
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);return "Bandeja conectada al visor de correo investigable.";
        }
    }
}
