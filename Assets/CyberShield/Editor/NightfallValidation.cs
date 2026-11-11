using System;
using System.IO;
using System.Linq;
using CyberShield.DataCenter;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CyberShield.Editor
{
    [InitializeOnLoad]
    public static class NightfallValidation
    {
        static double next;
        static int step;
        static NightfallValidation(){if(!AssetDatabase.IsAssetImportWorkerProcess())EditorApplication.update+=Tick;}
        [MenuItem("CyberShield/Nightfall Android/Validar misión completa")]
        public static void Validate()
        {
            DataCenterMobileBuilder.Build();
            if(UnityEngine.SceneManagement.SceneManager.sceneCount!=1)throw new Exception("Guarda los cambios de las otras escenas abiertas antes de validar Nightfall.");
            SessionState.SetBool("Nightfall.Validate",true);SessionState.SetInt("Nightfall.Step",0);
            EditorApplication.isPlaying=true;
        }
        static void Check(bool ok,string message){if(!ok)throw new Exception(message);Debug.Log("NIGHTFALL_PASS / "+message);File.AppendAllText("Logs/Nightfall-results.txt","PASS / "+message+"\n");}
        static void Tick()
        {
            if(!SessionState.GetBool("Nightfall.Validate",false)||!EditorApplication.isPlaying||EditorApplication.isCompiling||EditorApplication.timeSinceStartup<next)return;
            var m=UnityEngine.Object.FindFirstObjectByType<DataCenterMobileMission>();if(!m||m.Transitioning)return;
            step=SessionState.GetInt("Nightfall.Step",0);
            try
            {
                switch(step)
                {
                    case 0:
                        Directory.CreateDirectory("Logs");File.WriteAllText("Logs/Nightfall-results.txt","");
                        Check(!m.sim.enabled,"el modo móvil controla la escena sin entradas duplicadas");
                        Check(m.voices.Length==9&&m.voices.All(v=>v),"nueve instrucciones habladas locales");
                        Check(m.music.clip&&m.selectSound&&m.scanSound&&m.successSound&&m.teleportSound,"música y efectos presentes");
                        Check(m.sim.actions.Count(a=>a.id>=20&&a.id<=22)==3,"tres módulos físicos interactivos");
                        Capture(m,"Portal");break;
                    case 1:m.Act(2);Check(m.Stage==NightfallStage.Attack,"inicio mediante acción del jugador");m.AdvanceMission(24.1f);Check(m.Stage==NightfallStage.Evidence&&m.sim.Orders==36,"ataque completo: 36 órdenes");break;
                    case 2:Capture(m,"Investigacion");m.Act(3);m.Act(3);Check(m.EvidenceCount==1,"la misma evidencia no suma dos veces");m.Act(7);m.Act(8);Check(m.Stage==NightfallStage.Contain&&m.EvidenceCount==3,"tres evidencias desbloquean contención");break;
                    case 3:m.Act(8);Check(m.Stage==NightfallStage.Mfa,"desconectar el enlace desbloquea MFA");next=EditorApplication.timeSinceStartup+3.3;break;
                    case 4:
                        Capture(m,"MFA");
                        m.Act(20+(m.MemoryPad(0)+1)%3);
                        Check(m.Mistakes==1&&!m.sim.Mfa,"secuencia incorrecta vuelve a mostrar las luces");
                        m.AdvanceMission(3.1f);
                        for(int i=0;i<3;i++)m.Act(20+m.MemoryPad(i));
                        Check(m.Stage==NightfallStage.Schedule&&m.sim.Mfa,"repetir la secuencia de la partida activa MFA");
                        break;
                    case 5:if(m.TimingPosition<.35f||m.TimingPosition>.65f)return;m.Act(5);Check(m.Stage==NightfallStage.Siem&&m.sim.Schedule,"pulso en ventana verde configura horario");break;
                    case 6:m.Act(20);m.Act(21);m.Act(22);Check(m.Stage==NightfallStage.Defense&&m.sim.Siem&&m.sim.Blocked,"nodos 1-2-3 activan SIEM y defensa");break;
                    case 7:
                        m.AdvanceMission(20f);
                        Check(m.Stage==NightfallStage.Defense&&m.DefenseWave==0,"esperar no completa el contraataque");
                        Capture(m,"Contraataque");
                        m.Act(20+(m.DefenseTarget+1)%3);
                        Check(m.DefenseWave==0&&m.Mistakes==2,"un módulo incorrecto no intercepta la oleada");
                        for(int i=0;i<5;i++)m.Act(20+m.DefenseTarget);
                        Check(m.Stage==NightfallStage.Debrief&&m.DefenseWave==5&&m.sim.Orders==0,"cinco intercepciones completan la defensa con cero órdenes");
                        Check(m.Score>=1100,"puntuación y penalizaciones coherentes");
                        Capture(m,"Resultado");
                        break;
                    case 8:
                        m.Act(14);Check(m.AudioMuted&&m.music.mute,"silenciar audio desde el visor");m.Act(14);
                        m.Restart();Check(m.Stage==NightfallStage.Ready&&m.EvidenceCount==0&&m.Score==0,"reinicio limpio");
                        Check(UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).All(t=>GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)==0),"escena sin componentes perdidos");
                        Capture(m,"Portal");SessionState.SetBool("Nightfall.Validate",false);Debug.Log("NIGHTFALL_VALIDATION_OK");if(Application.isBatchMode)EditorApplication.Exit(0);else EditorApplication.isPlaying=false;return;
                }
                SessionState.SetInt("Nightfall.Step",step+1);
                if(step!=3)next=EditorApplication.timeSinceStartup+.65;
            }
            catch(Exception e){File.AppendAllText("Logs/Nightfall-results.txt","FAIL / "+e);Debug.LogException(e);SessionState.SetBool("Nightfall.Validate",false);if(Application.isBatchMode)EditorApplication.Exit(1);else EditorApplication.isPlaying=false;}
        }
        static void Capture(DataCenterMobileMission m,string name)
        {
            Directory.CreateDirectory(DataCenterMobileBuilder.Folder+"/Preview");Canvas.ForceUpdateCanvases();var rt=new RenderTexture(1600,900,24);m.sim.view.targetTexture=rt;m.sim.view.Render();RenderTexture.active=rt;
            var image=new Texture2D(1600,900,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1600,900),0,0);image.Apply();File.WriteAllBytes(DataCenterMobileBuilder.Folder+"/Preview/"+name+".png",image.EncodeToPNG());m.sim.view.targetTexture=null;RenderTexture.active=null;UnityEngine.Object.DestroyImmediate(image);UnityEngine.Object.DestroyImmediate(rt);
        }
    }
}
