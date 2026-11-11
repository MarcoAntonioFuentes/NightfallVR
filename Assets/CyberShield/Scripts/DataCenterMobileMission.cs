using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using UnityEngine.XR.Management;
using Google.XR.Cardboard;
using CommonUsages = UnityEngine.XR.CommonUsages;

namespace CyberShield.DataCenter
{
    public enum NightfallStage { Ready, Attack, Evidence, Contain, Mfa, Schedule, Siem, Defense, Debrief }

    public sealed class DataCenterMobileMission : MonoBehaviour
    {
        public DataCenterSimulation sim;
        public TMP_Text headline, hint, stats, status, combo, rating;
        public AudioSource music, fx;
        public AudioClip[] voices;
        public AudioClip selectSound, scanSound, successSound, failSound, teleportSound, breachSound;
        public Transform[] packets, memoryPads;
        public Renderer[] padFaces;
        public Transform sweep, cablePlug, paper, shield, navigationAnchor;
        public LineRenderer network, dwellRing, reticle;
        public Material safeMaterial, dangerMaterial;
        public NightfallStage Stage { get; private set; }
        public int Score { get; private set; }
        public int EvidenceCount { get; private set; }
        public int Mistakes { get; private set; }
        public bool VrEnabled { get; private set; }
        public float DwellProgress { get; private set; }
        public float TimingPosition => Mathf.PingPong(stageTime*.65f,1);
        public int Combo => codeStep;
        public bool Transitioning { get; private set; }
        public bool AudioMuted { get; private set; }
        bool logFound, printerFound, switchFound, subtitles, xrBusy, dragging;
        int codeStep;
        float stageTime, runTime, yaw, pitch, dwell, activateAfter, lastLed;
        Vector2 touchStart;
        Quaternion sensorReference=Quaternion.identity;
        bool calibrated;
        DataCenterAction focus;
        MaterialPropertyBlock block;
        string transcript;
        Coroutine jump;
        GameObject[] padHalos;
        LineRenderer impactRing;
        AudioClip interceptCue, victoryCue;
        readonly int[] code={0,1,2};
        readonly int[] waves={0,1,2,0,1};
        int wave, streak, bestStreak;
        float impact, signalTime;
        public int DefenseWave => wave;
        public int DefenseTarget => waves[Mathf.Min(wave, waves.Length - 1)];
        public int MemoryPad(int step) => code[step];
        const float AttackSeconds = 10f;
        static readonly string[] Lines =
        {
            "¡Ey, operador! Hoy te toca salvar el centro de datos. ¿Listo? ¡Vamos!",
            "¡Entraron al sistema! Mira esos paquetes rojos. Cada uno lleva una orden falsa.",
            "¡Ahora tú! Escanea el registro, la impresora y el switch. Tres pistas y avanzamos.",
            "¡Ya los tenemos! Busca el conector rojo y desconéctalo. ¡Corta su salida!",
            "Atento a las luces. Guarda el orden en tu cabeza. ¡Luego repítelo!",
            "Respira. Deja que el pulso llegue al centro verde y… ¡ahora!",
            "Conecta los nodos uno, dos y tres. ¡Que sus huellas queden guardadas!",
            "¡Vuelven al ataque! Toca el módulo rojo. ¡Son cinco oleadas, tú puedes!",
            "¡Lo lograste! Treinta y seis órdenes antes. Ahora, cero. ¡Esa defensa fue tuya!"
        };
        static readonly Vector3[] Stations={new Vector3(0,0,-6.3f),new Vector3(-2.8f,0,.1f),new Vector3(2.8f,0,.1f),new Vector3(0,0,2.8f)};
        readonly Color cyan=new Color(.08f,.85f,1), green=new Color(.1f,1,.55f), orange=new Color(1,.28f,.07f);

        void Start()
        {
            sim.enabled=false;
            #if UNITY_EDITOR
            foreach(var x in FindObjectsByType<UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.XRInteractionSimulator>(FindObjectsSortMode.None))Destroy(x.gameObject);
            #endif
            Screen.sleepTimeout=SleepTimeout.NeverSleep;Screen.orientation=ScreenOrientation.LandscapeLeft;
            QualitySettings.vSyncCount=0;Application.targetFrameRate=60;
            block=new MaterialPropertyBlock();
            impactRing=CreateImpactRing();
            interceptCue=Resources.Load<AudioClip>("Nightfall/Intercept");
            victoryCue=Resources.Load<AudioClip>("Nightfall/Victory");
            padHalos=FindObjectsByType<LineRenderer>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(l=>l.name=="Halo de nodo").Select(l=>l.gameObject).ToArray();
            if(AttitudeSensor.current!=null)InputSystem.EnableDevice(AttitudeSensor.current);
            music.loop=true;music.Play();Restart();
        }

        public void Restart()
        {
            sim.Reset(false);Score=0;Mistakes=0;EvidenceCount=0;codeStep=0;
            wave=streak=bestStreak=0;
            impact=signalTime=0;
            Shuffle(code);
            waves[0]=Random.Range(0,3);
            for(int i=1;i<waves.Length;i++) waves[i]=(waves[i-1]+Random.Range(1,3))%3;
            logFound=printerFound=switchFound=false;runTime=0;
            if(cablePlug)cablePlug.localPosition=Vector3.zero;
            if(paper)paper.gameObject.SetActive(false);
            if(shield)shield.gameObject.SetActive(false);
            sim.player.position=Stations[0];yaw=pitch=0;sim.view.transform.localRotation=Quaternion.identity;
            SetStage(NightfallStage.Ready,0,"TU TURNO, OPERADOR","Mira el botón y toca para comenzar.");
            Refresh();
        }

        public void Act(int id)
        {
            if(Time.unscaledTime<activateAfter||Transitioning)return;
            if(id==0){Restart();return;}
            if(id==1){ToggleVR();return;}
            if(id>=10&&id<=13){Teleport(id-10);return;}
            if(id==14){AudioMuted=!AudioMuted;music.mute=fx.mute=sim.narrator.mute=AudioMuted;Refresh();return;}
            if(id==15){Recenter();return;}
            if(id==16){subtitles=!subtitles;Refresh();return;}
            if(id==17){if(VrEnabled)Api.ScanDeviceParams();return;}
            if(id==9){Voice((int)Stage);return;}
            if(Stage==NightfallStage.Ready&&id==2)
            {
                sim.Act(2);Cue(breachSound);SetStage(NightfallStage.Attack,1,"INTRUSIÓN DETECTADA","Observa cómo salen las órdenes.");return;
            }
            if(Stage==NightfallStage.Evidence)
            {
                if(id==3&&!logFound){logFound=true;sim.Act(3);EvidenceCount++;Success(100);}
                else if(id==7&&!printerFound){printerFound=true;sim.Act(7);EvidenceCount++;Success(100);StartCoroutine(RecoverPaper());}
                else if(id==8&&!switchFound){switchFound=true;sim.Act(8);EvidenceCount++;Success(100);}
                else return;
                Refresh();
                if(EvidenceCount==3){SetStage(NightfallStage.Contain,3,"CORTA EL ENLACE","Mira el conector rojo y actívalo.");Teleport(3);}
                return;
            }
            if(Stage==NightfallStage.Contain&&id==8)
            {
                Success(150);if(cablePlug)cablePlug.localPosition=new Vector3(0,0,-.35f);
                sim.Reset(true);sim.Act(3);sim.Act(7);sim.Act(8);
                SetStage(NightfallStage.Mfa,4,"MEMORIZA LAS LUCES","Observa los tres pulsos.");Teleport(2);return;
            }
            if(Stage==NightfallStage.Mfa&&id>=20&&id<=22)
            {
                if(stageTime<3){hint.text="Observa primero la secuencia.";return;}
                if(id-20==code[codeStep]){codeStep++;Cue(selectSound);if(codeStep==3){sim.Act(4);Success(200);codeStep=0;SetStage(NightfallStage.Schedule,5,"BLOQUEA EL HORARIO","Activa BLOQUEAR cuando el pulso esté en verde.");}}
                else {codeStep=0;stageTime=0;Error();}
                Refresh();return;
            }
            if(Stage==NightfallStage.Schedule&&id==5)
            {
                if(Mathf.Abs(TimingPosition-.5f)<.23f){sim.Act(5);Success(150);codeStep=0;SetStage(NightfallStage.Siem,6,"CONECTA EL SIEM","Une los nodos: 1 → 2 → 3.");}
                else Error();Refresh();return;
            }
            if(Stage==NightfallStage.Siem&&id>=20&&id<=22)
            {
                if(id-20==codeStep){codeStep++;Cue(scanSound);if(codeStep==3){sim.Act(6);Success(200);sim.Act(2);SetStage(NightfallStage.Defense,7,"CONTRAATAQUE","Intercepta el módulo rojo. Cinco oleadas.");if(shield)shield.gameObject.SetActive(true);}}
                else {codeStep=0;Error();}Refresh();return;
            }
            if(Stage==NightfallStage.Defense&&id>=20&&id<=22)
            {
                if(id-20!=DefenseTarget)
                {
                    Error();
                    hint.text="Sigue el módulo rojo.";
                    return;
                }

                int bonus=Mathf.RoundToInt(40f*(1f-Mathf.Clamp01(signalTime/5f)));
                impactRing.transform.position=memoryPads[DefenseTarget].position+Vector3.back*.15f;
                Success(80+bonus);
                Cue(interceptCue);
                signalTime=0;
                wave++;
                if(wave==waves.Length)
                {
                    sim.Finish();
                    Cue(victoryCue);
                    SetStage(NightfallStage.Debrief,8,"MISIÓN COMPLETADA","36 órdenes antes. 0 con tu defensa.");
                    Teleport(0);
                }
                return;
            }
            if(Stage==NightfallStage.Debrief&&id==2)Restart();
        }

        void SetStage(NightfallStage stage,int voice,string title,string instruction)
        {
            Stage=stage;stageTime=0;headline.text=title;hint.text=instruction;activateAfter=Time.unscaledTime+.35f;
            if(stage==NightfallStage.Debrief)
            {
                var panel=rating.transform.parent;
                panel.SetParent(sim.view.transform,false);panel.localPosition=new Vector3(0,0,1.5f);panel.localScale=Vector3.one*.001f;
                panel.GetComponent<RectTransform>().sizeDelta=new Vector2(1350,650);rating.rectTransform.sizeDelta=new Vector2(1250,650);
                var background=panel.GetComponent<UnityEngine.UI.Image>();if(!background)background=panel.gameObject.AddComponent<UnityEngine.UI.Image>();
                background.color=new Color(.006f,.02f,.04f,.95f);background.raycastTarget=false;
            }
            Voice(voice);Refresh();
        }
        void Voice(int index)
        {
            index=Mathf.Clamp(index,0,Lines.Length-1);
            transcript=Lines[index];
            sim.narrator.Stop();
            sim.narrator.pitch=1;
            if(index<voices.Length&&voices[index]){sim.narrator.clip=voices[index];sim.narrator.Play();}
        }
        void Cue(AudioClip clip){if(clip)fx.PlayOneShot(clip,.6f);}
        void Success(int points)
        {
            streak++;
            bestStreak=Mathf.Max(bestStreak,streak);
            Score+=points+Mathf.Min(5,streak-1)*15;
            impact=1;
            Cue(successSound);
            HandheldVibration();
        }

        void Error()
        {
            Mistakes++;
            streak=0;
            Score=Mathf.Max(0,Score-25);
            Cue(failSound);
            hint.text="Inténtalo otra vez. Tú puedes.";
            HandheldVibration();
        }

        static void Shuffle(int[] pads)
        {
            for(int i=pads.Length-1;i>0;i--)
            {
                int j=Random.Range(0,i+1);
                (pads[i],pads[j])=(pads[j],pads[i]);
            }
        }

        LineRenderer CreateImpactRing()
        {
            var ring=new GameObject("Onda de intercepción").AddComponent<LineRenderer>();
            ring.transform.SetParent(transform,false);
            ring.sharedMaterial=safeMaterial;
            ring.useWorldSpace=false;
            ring.loop=true;
            ring.positionCount=32;
            for(int i=0;i<ring.positionCount;i++)
            {
                float angle=i*Mathf.PI*2/ring.positionCount;
                ring.SetPosition(i,new Vector3(Mathf.Cos(angle),Mathf.Sin(angle),0));
            }
            ring.enabled=false;
            return ring;
        }
        void HandheldVibration(){if(Application.isMobilePlatform)Handheld.Vibrate();}

        void Update()
        {
            AdvanceMission(Time.deltaTime);
            AnimateWorld();ReadInput();RefreshCounters();
            if(VrEnabled)
            {
                if(Api.IsCloseButtonPressed)ToggleVR();
                if(Api.IsGearButtonPressed)Api.ScanDeviceParams();
                if(Api.IsTriggerHeldPressed)Api.Recenter();
                if(Api.HasNewDeviceParams())Api.ReloadDeviceParams();
                Api.UpdateScreenParams();
            }
            bool action=Stage==NightfallStage.Attack||Stage==NightfallStage.Defense;
            music.pitch=Mathf.MoveTowards(music.pitch,action?1.12f:1f,Time.deltaTime*.2f);
            music.volume=Mathf.MoveTowards(music.volume,sim.narrator.isPlaying?.075f:action?.27f:.16f,Time.deltaTime*.4f);
            impact=Mathf.MoveTowards(impact,0,Time.deltaTime*2.2f);
        }

        public void AdvanceMission(float seconds)
        {
            stageTime+=seconds;if(Stage!=NightfallStage.Ready&&Stage!=NightfallStage.Debrief)runTime+=seconds;
            if(Stage==NightfallStage.Attack)
            {
                sim.Advance(seconds*DataCenterSimulation.Duration/AttackSeconds);
                if(stageTime>=AttackSeconds){sim.Finish();SetStage(NightfallStage.Evidence,2,"BUSCA 3 EVIDENCIAS","Escanea el log, la impresora y el switch.");Teleport(1);}
            }
            if(Stage==NightfallStage.Defense)
            {
                signalTime+=seconds;
                sim.Advance(seconds*DataCenterSimulation.Duration/18f);
            }
        }

        void RefreshCounters()
        {
            int mins=(20*60+36+Mathf.RoundToInt(sim.elapsed/DataCenterSimulation.Duration*443))%1440;
            sim.clock.text=$"{mins/60:00}:{mins%60:00}";
            stats.text=Stage==NightfallStage.Mfa?$"MFA   {codeStep}/3":Stage==NightfallStage.Siem?$"SIEM   {codeStep}/3":Stage==NightfallStage.Attack?$"<color=#FF593D>{sim.Orders:00}</color> ÓRDENES":Stage==NightfallStage.Defense?"<color=#47FFC4>00</color> ÓRDENES":$"{Score:0000} PTS   ·   {EvidenceCount}/3";
            status.text=Stage==NightfallStage.Ready?"ANDROID / TOCA O MIRA":Stage==NightfallStage.Attack||Stage==NightfallStage.Defense?$"04 FEB 2016   /   {sim.clock.text}":$"{Mathf.Clamp((int)Stage,1,8):00} / 08   ·   {(int)runTime/60:00}:{(int)runTime%60:00}";
            if(subtitles)hint.text=transcript;
            if(Stage==NightfallStage.Mfa)
            {
                headline.text=stageTime<3?"MEMORIZA LAS LUCES":"REPITE LA SECUENCIA";
                if(!subtitles)hint.text=stageTime<3?"Observa los tres pulsos.":"Activa los módulos en el mismo orden.";
            }
            if(Stage==NightfallStage.Defense)stats.text=$"OLEADAS  {wave}/{waves.Length}    ·    0 ÓRDENES";
            combo.text=impact>0&&streak>1&&Stage!=NightfallStage.Debrief?$"<color=#69FFCA>RACHA ×{streak}</color>":"";
            if(Stage==NightfallStage.Debrief)rating.text=$"{(Mistakes==0?"S":Mistakes<3?"A":"B")}  ·  {Score} PUNTOS\n36 → 0 ÓRDENES\n5 OLEADAS INTERCEPTADAS\nRACHA MÁXIMA ×{bestStreak}";
        }
        public void Refresh()
        {
            sim.logs.text=logFound?"EVIDENCIA GUARDADA\n01 / 03":"REGISTRO BORRADO\nESCANEA LA ANOMALÍA";
            sim.printerStatus.text=printerFound?"02 / 03\nEVIDENCIA GUARDADA":"IMPRESORA OFFLINE";
            sim.controls.text=$"MFA {(sim.Mfa?"●":"○")}   HORARIO {(sim.Schedule?"●":"○")}   SIEM {(sim.Siem?"●":"○")}";
            foreach(var a in sim.actions)
            {
                if(!a||!a.label)continue;
                string t=a.id==0?"REINICIAR":a.id==1?(VrEnabled?"SALIR DEL VISOR":"MODO VISOR"):a.id==2?(Stage==NightfallStage.Debrief?"OTRA MISIÓN":"INICIAR MISIÓN"):a.id==3?(logFound?"LOG GUARDADO":"ESCANEAR LOG"):a.id==5?"BLOQUEAR":a.id==7?(printerFound?"PAPEL GUARDADO":"ESCANEAR IMPRESORA"):a.id==8?(Stage==NightfallStage.Contain?"DESCONECTAR":switchFound?"SWITCH LOCALIZADO":"ESCANEAR SWITCH"):a.id==9?"ESCUCHAR PISTA":a.id==14?(AudioMuted?"ACTIVAR AUDIO":"AUDIO"):a.id==15?"CENTRAR":a.id==16?"SUBTÍTULOS":a.id==17?"QR VISOR":a.id>=20&&a.id<=22?(a.id-19).ToString():null;
                if(t!=null)a.label.text=t;
                bool enabled=a.id!=4&&a.id!=6;
                if(a.id>=20&&a.id<=22)enabled=Stage==NightfallStage.Mfa||Stage==NightfallStage.Siem||Stage==NightfallStage.Defense;
                if(a.id==5)enabled=Stage==NightfallStage.Schedule;
                if(a.id==2)enabled=Stage==NightfallStage.Ready||Stage==NightfallStage.Debrief;
                a.gameObject.SetActive(enabled);if(a.label)a.label.transform.parent.gameObject.SetActive(enabled);
            }
            if(sweep)sweep.parent.gameObject.SetActive(Stage==NightfallStage.Schedule);
            if(padHalos!=null)foreach(var halo in padHalos)halo.SetActive(Stage==NightfallStage.Mfa||Stage==NightfallStage.Siem||Stage==NightfallStage.Defense);
            if(rating)rating.transform.parent.gameObject.SetActive(Stage==NightfallStage.Debrief);
            RefreshCounters();
        }

        void AnimateWorld()
        {
            impactRing.enabled=Stage==NightfallStage.Defense&&wave>0&&impact>0;
            if(impactRing.enabled)
            {
                impactRing.transform.localScale=Vector3.one*Mathf.Lerp(.85f,.25f,impact);
                impactRing.widthMultiplier=.025f*impact;
            }
            bool hostile=Stage==NightfallStage.Attack;
            network.sharedMaterial=hostile||Stage==NightfallStage.Evidence||Stage==NightfallStage.Contain?dangerMaterial:safeMaterial;
            network.enabled=Stage==NightfallStage.Attack||Stage==NightfallStage.Evidence||Stage==NightfallStage.Contain||Stage==NightfallStage.Siem||Stage==NightfallStage.Defense;
            if(Stage==NightfallStage.Siem)
            {
                for(int i=0;i<3;i++)network.SetPosition(i,memoryPads[i].position+Vector3.up*.48f);
            }
            else
            {
                network.SetPosition(0,new Vector3(-2.7f,2.15f,2.2f));network.SetPosition(1,new Vector3(0,3.4f,3.7f));network.SetPosition(2,Stage==NightfallStage.Defense?memoryPads[DefenseTarget].position+Vector3.up*.35f:new Vector3(3.2f,3.1f,5.6f));
            }
            for(int i=0;i<packets.Length;i++)
            {
                bool show=Stage==NightfallStage.Attack||Stage==NightfallStage.Defense||Stage==NightfallStage.Siem;
                packets[i].gameObject.SetActive(show);if(!show)continue;
                float t=Mathf.Repeat(Time.time*(Stage==NightfallStage.Defense?.8f:.32f)+i/(float)packets.Length,1);
                var from=new Vector3(-2.7f,2.15f,2.2f);var to=Stage==NightfallStage.Defense?memoryPads[DefenseTarget].position+Vector3.up*.35f:new Vector3(3.2f,3.1f,5.6f);
                packets[i].position=Stage==NightfallStage.Siem?Vector3.Lerp(memoryPads[0].position,memoryPads[2].position,t)+Vector3.up*.48f:Vector3.Lerp(from,to,t)+Vector3.up*Mathf.Sin(t*Mathf.PI)*1.05f;
                packets[i].GetComponent<Renderer>().sharedMaterial=hostile||Stage==NightfallStage.Defense&&impact<.4f?dangerMaterial:safeMaterial;
            }
            sim.alarmLight.enabled=hostile&&Mathf.Sin(Time.time*4)>0;
            if(sim.core){sim.core.Rotate(9*Time.deltaTime,25*Time.deltaTime,5*Time.deltaTime);sim.core.localScale=Vector3.one*(1+Mathf.Sin(Time.time*2)*.06f);}
            if(shield&&shield.gameObject.activeSelf)
            {
                shield.Rotate(0,Time.deltaTime*(24+impact*100),0);
                shield.localScale=Vector3.one*(1+impact*.25f);
            }
            if(sweep)sweep.localPosition=new Vector3(Mathf.Lerp(-1.05f,1.05f,TimingPosition),0,-.04f);
            for(int i=0;i<padFaces.Length;i++)
            {
                bool on=Stage==NightfallStage.Mfa&&stageTime<3&&stageTime%1<.72f&&code[Mathf.Clamp((int)stageTime,0,2)]==i||Stage==NightfallStage.Siem&&i<codeStep;
                bool incoming=Stage==NightfallStage.Defense&&DefenseTarget==i;
                Color color=incoming?orange*(1.4f+.4f*Mathf.Sin(Time.time*9)):on?green*2:new Color(.02f,.09f,.13f);
                block.SetColor("_BaseColor",color);
                padFaces[i].SetPropertyBlock(block);
            }
            if(Time.time>lastLed){lastLed=Time.time+.07f;for(int j=0;j<8;j++){int i=(Time.frameCount*7+j*13)%sim.leds.Length;block.SetColor("_BaseColor",(hostile&&i%7==0?orange:green)*(Random.value>.2f?1:.15f));sim.leds[i].SetPropertyBlock(block);}}
        }

        void ReadInput()
        {
            bool tap=false;
            var mouse=Mouse.current;var kb=Keyboard.current;var touch=Touchscreen.current;
            Ray ray=new Ray(sim.view.transform.position,sim.view.transform.forward);
            if(VrEnabled)
            {
                var h=InputDevices.GetDeviceAtXRNode(XRNode.Head);
                if(h.TryGetFeatureValue(CommonUsages.deviceRotation,out Quaternion q))sim.view.transform.localRotation=q;
                tap=Api.IsTriggerPressed;
            }
            else
            {
                if(Application.isMobilePlatform&&AttitudeSensor.current!=null&&AttitudeSensor.current.enabled)
                {
                    var a=AttitudeSensor.current.attitude.ReadValue();
                    if(a.x*a.x+a.y*a.y+a.z*a.z+a.w*a.w>.5f)
                    {
                        var q=Quaternion.Euler(90,0,0)*new Quaternion(a.x,a.y,-a.z,-a.w);
                        if(!calibrated){sensorReference=Quaternion.Inverse(Quaternion.Euler(0,q.eulerAngles.y,0));calibrated=true;}
                        sim.view.transform.localRotation=sensorReference*q;
                    }
                }
                if(mouse!=null&&mouse.rightButton.isPressed)Look(mouse.delta.ReadValue()*.12f);
                if(touch!=null)
                {
                    var t=touch.primaryTouch;
                    if(t.press.wasPressedThisFrame){touchStart=t.position.ReadValue();dragging=false;}
                    if(t.press.isPressed&&Vector2.Distance(touchStart,t.position.ReadValue())>20){dragging=true;if(AttitudeSensor.current==null)Look(t.delta.ReadValue()*.12f);}
                    if(t.press.wasReleasedThisFrame&&!dragging){tap=true;ray=sim.view.ScreenPointToRay(t.position.ReadValue());}
                }
                if(mouse!=null&&!mouse.rightButton.isPressed){ray=sim.view.ScreenPointToRay(mouse.position.ReadValue());tap|=mouse.leftButton.wasPressedThisFrame;}
            }
            if(VrEnabled)ray=new Ray(sim.view.transform.position,sim.view.transform.forward);
            if(kb!=null)
            {
                tap|=kb.eKey.wasPressedThisFrame;if(kb.rKey.wasPressedThisFrame)Restart();
                if(kb.digit1Key.wasPressedThisFrame)Teleport(0);if(kb.digit2Key.wasPressedThisFrame)Teleport(1);if(kb.digit3Key.wasPressedThisFrame)Teleport(2);if(kb.digit4Key.wasPressedThisFrame)Teleport(3);
            }
            DataCenterAction target=null;if(Physics.Raycast(ray,out RaycastHit hit,15))target=hit.collider.GetComponent<DataCenterAction>();
            if(target!=focus){if(focus)focus.Highlight(false);focus=target;dwell=0;if(focus)focus.Highlight(true);}
            if(target&&!Transitioning)
            {
                dwell+=Time.deltaTime;DwellProgress=Mathf.Clamp01(dwell/1.35f);
                if(tap||(VrEnabled&&dwell>=1.35f&&dwell<99)){target.Activate();dwell=100;}
            }
            else {dwell=0;DwellProgress=0;}
            if(dwellRing){int count=Mathf.Max(2,Mathf.CeilToInt(DwellProgress*64));dwellRing.positionCount=count;for(int i=0;i<count;i++){float a=i/63f*Mathf.PI*2;dwellRing.SetPosition(i,new Vector3(Mathf.Sin(a)*.018f,Mathf.Cos(a)*.018f,0));}dwellRing.enabled=focus&&dwell<99;}
        }
        void Look(Vector2 d){yaw+=d.x;pitch=Mathf.Clamp(pitch-d.y,-65,65);sim.view.transform.localRotation=Quaternion.Euler(pitch,yaw,0);}
        public void Teleport(int index){if(jump==null)jump=StartCoroutine(Jump(index));}
        IEnumerator Jump(int index)
        {
            Transitioning=true;
            var g=GameObject.CreatePrimitive(PrimitiveType.Quad);Destroy(g.GetComponent<Collider>());g.transform.SetParent(sim.view.transform,false);g.transform.localPosition=new Vector3(0,0,.13f);g.transform.localScale=Vector3.one;
            var m=new Material(Shader.Find("Universal Render Pipeline/Unlit"));m.color=Color.black;g.GetComponent<Renderer>().sharedMaterial=m;
            yield return null;sim.player.position=Stations[Mathf.Clamp(index,0,3)];yaw=pitch=0;
            if(!VrEnabled)sim.view.transform.localRotation=Quaternion.identity;
            Cue(teleportSound);yield return new WaitForSeconds(.16f);Destroy(g);Destroy(m);Transitioning=false;jump=null;
        }
        IEnumerator RecoverPaper(){paper.gameObject.SetActive(true);var start=paper.position;for(float t=0;t<1;t+=Time.deltaTime){paper.position=start+Vector3.up*Mathf.Sin(t*Mathf.PI)*.45f;paper.Rotate(0,Time.deltaTime*80,0);yield return null;}paper.position=start;}
        public void Recenter(){calibrated=false;if(VrEnabled)Api.Recenter();else{yaw=pitch=0;sim.view.transform.localRotation=Quaternion.identity;}}
        public void ToggleVR()
        {
            #if UNITY_ANDROID && !UNITY_EDITOR
            if(!xrBusy)StartCoroutine(SwitchVR());
            #else
            hint.text="El modo visor se activa en el APK Android.";
            #endif
        }
        IEnumerator SwitchVR()
        {
            xrBusy=true;var manager=XRGeneralSettings.Instance?XRGeneralSettings.Instance.Manager:null;
            if(manager==null){hint.text="Configura Cardboard en XR Plug-in Management.";xrBusy=false;yield break;}
            if(VrEnabled){manager.StopSubsystems();manager.DeinitializeLoader();VrEnabled=false;sim.view.ResetAspect();sim.view.fieldOfView=70;}
            else
            {
                yield return manager.InitializeLoader();
                if(manager.activeLoader!=null)
                {
                    manager.StartSubsystems();VrEnabled=true;
                    if(Api.HasDeviceParams())Api.ReloadDeviceParams();else Api.ScanDeviceParams();
                }
                else hint.text="No se pudo iniciar el visor. Puedes jugar en pantalla.";
            }
            xrBusy=false;calibrated=false;Refresh();
            if(navigationAnchor)
            {
                navigationAnchor.SetParent(VrEnabled?sim.player:sim.view.transform,false);
                navigationAnchor.localPosition=VrEnabled?Vector3.up*1.65f:Vector3.zero;
                navigationAnchor.localRotation=Quaternion.identity;
            }
        }
        void OnDestroy(){if(AttitudeSensor.current!=null&&AttitudeSensor.current.enabled)InputSystem.DisableDevice(AttitudeSensor.current);}
    }
}
