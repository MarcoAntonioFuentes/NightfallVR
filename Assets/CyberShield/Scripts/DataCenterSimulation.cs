using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using CommonUsages = UnityEngine.XR.CommonUsages;

namespace CyberShield.DataCenter
{
    // Educational reconstruction: all accounts, logs and transfers are simulated locally.
    public sealed class DataCenterSimulation : MonoBehaviour
    {
        public TMP_Text clock, dashboard, logs, objective, subtitle, printerStatus, controls, result;
        public Camera view;
        public Transform player, core;
        public Light alarmLight;
        public AudioSource narrator, ambience, alarm;
        public AudioClip[] narration;
        public Renderer[] leds;
        public DataCenterAction[] actions;
        public bool Defense { get; private set; }
        public bool Running { get; private set; }
        public bool Complete { get; private set; }
        public bool Evidence { get; private set; }
        public bool PrinterEvidence { get; private set; }
        public bool SwitchEvidence { get; private set; }
        public bool Mfa { get; private set; }
        public bool Schedule { get; private set; }
        public bool Siem { get; private set; }
        public bool Blocked { get; private set; }
        public int Orders { get; private set; }
        public float elapsed;
        public const float Duration = 105f;
        float yaw, pitch, dwell, subtitleUntil;
        bool triggerHeld, teleporting, muted;
        DataCenterAction focused;
        LineRenderer pointer;
        MaterialPropertyBlock ledProperties;
        AudioClip hum, siren;
        public bool AttackSeen { get; private set; }
        public bool DefenseSeen { get; private set; }
        string message = "Bienvenido, operador. Revisa las evidencias antes de iniciar la sesión.";

        void Start()
        {
            #if UNITY_EDITOR
            // This scene provides its own desktop controls; avoid competing simulated head poses.
            foreach (var simulator in FindObjectsByType<UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.XRInteractionSimulator>(FindObjectsSortMode.None))
                Destroy(simulator.gameObject);
            #endif
            Application.targetFrameRate = 72;
            ledProperties = new MaterialPropertyBlock();
            hum = Tone("Ventilación", 3f, t => .025f * Mathf.Sin(t * 2 * Mathf.PI * 72) + .008f * Mathf.Sin(t * 2 * Mathf.PI * 113));
            siren = Tone("Alarma", 1f, t => .13f * Mathf.Sin(2 * Mathf.PI * (520 * t - 110 / (2 * Mathf.PI) * Mathf.Cos(2 * Mathf.PI * t))));
            ambience.clip = hum; ambience.loop = true; ambience.Play();
            alarm.clip = siren; alarm.loop = true;
            pointer = new GameObject("Rayo de interacción").AddComponent<LineRenderer>();
            pointer.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            pointer.sharedMaterial.color = new Color(.1f, 1, .7f);
            pointer.startWidth = .005f; pointer.endWidth = .002f; pointer.positionCount = 2;
            Speak(0, message); Refresh();
        }

        static AudioClip Tone(string name, float length, Func<float,float> sample)
        {
            const int rate = 22050;
            var data = new float[(int)(rate * length)];
            for (int i = 0; i < data.Length; i++) data[i] = sample((float)i / rate);
            var clip = AudioClip.Create(name, data.Length, 1, rate, false); clip.SetData(data, 0); return clip;
        }

        public void Act(int id)
        {
            if (id == 0) { Reset(false); return; }
            if (id == 1) { Reset(true); return; }
            if (id == 2)
            {
                if (Running || Complete) return;
                Running = true;
                if (Defense && (Mfa || Schedule))
                {
                    Blocked = true; alarm.Play();
                    Speak(4, "Alerta. Acceso fuera de horario. Sesión bloqueada antes de emitir órdenes.");
                }
                else Speak(1, "Son las veinte treinta y seis. Una sesión con credenciales robadas ha accedido a SWIFT. Observa el reloj y busca los huecos del registro.");
            }
            else if (id == 3)
            {
                Evidence = true;
                Speak(2, "Evidencia preservada. Faltan registros de auditoría. Un registro local puede ser manipulado; centralizarlo permite conservar una copia independiente.");
            }
            else if (id >= 4 && id <= 6)
            {
                if (!Defense) { message = "Cambia a MODO DEFENSA para preparar los controles."; Refresh(); return; }
                if (Running || Complete) { message = "Los controles se preparan antes de iniciar. Reinicia el modo defensa."; Refresh(); return; }
                if (id == 4) Mfa = !Mfa;
                if (id == 5) Schedule = !Schedule;
                if (id == 6) Siem = !Siem;
                Speak(3, "Configuración actualizada. Prepara MFA, bloqueo fuera de horario y SIEM. Después, inicia la sesión para comparar el resultado.");
            }
            else if (id == 7) { PrinterEvidence = true; Speak(6, "Impresora apagada. No se imprimieron confirmaciones. La falta de papel no demuestra que no hubo actividad."); }
            else if (id == 8) { SwitchEvidence = true; Speak(7, "Switch inspeccionado. La conectividad está activa. Conservar evidencia y bloquear accesos es más seguro que apagar equipos sin investigar."); }
            else if (id == 9) { if (Running) Finish(); }
            else if (id >= 10 && id <= 13) Teleport(id - 10);
            else if (id == 14) { muted = !muted; narrator.mute = muted; ambience.mute = muted; alarm.mute = muted; }
            Refresh();
        }

        public void Reset(bool defense)
        {
            Defense = defense; Running = Complete = Blocked = false;
            Mfa = Schedule = Siem = false; elapsed = 0; Orders = 0;
            Evidence = PrinterEvidence = SwitchEvidence = false;
            alarm.Stop(); alarmLight.enabled = false;
            result.text = "INFORME / PENDIENTE\nCompleta una sesión para comparar resultados.";
            message = defense ? "Prepara los tres controles y después inicia la sesión." : "Reconstrucción 2016: inspecciona el log, la impresora y el switch. Inicia cuando estés listo.";
            narrator.Stop(); subtitle.text = "ASISTENTE / " + message; subtitleUntil = Time.time + 12;
            Refresh();
        }

        void Update()
        {
            Advance(Time.deltaTime);
            alarmLight.enabled = Blocked && (Mathf.Sin(Time.time * 8) > 0);
            if (core) core.Rotate(0, Time.deltaTime * 18, 0);
            for (int i = 0; i < leds.Length; i++)
            {
                var c = (Running && !Blocked && i % 11 == 0) ? new Color(1,.08f,.04f) : new Color(.03f,1,.47f);
                c *= Mathf.Sin(Time.time * (2 + i % 5) + i) > -.55f ? 1 : .14f;
                ledProperties.SetColor("_BaseColor", c); ledProperties.SetColor("_EmissionColor", c * 2);
                leds[i].SetPropertyBlock(ledProperties);
            }
            ReadInput();
            if (Time.time > subtitleUntil) subtitle.text = "ASISTENTE / EN LÍNEA     ·     " + (muted ? "AUDIO SILENCIADO" : "VOZ SINTÉTICA ORIGINAL");
        }

        public void Advance(float seconds)
        {
            if (Running)
            {
                elapsed = Mathf.Min(Duration, elapsed + Mathf.Max(0,seconds));
                // The 36 orders leave during the first 90 simulated minutes, not across the whole night.
                Orders = Blocked ? 0 : Mathf.Min(36, Mathf.FloorToInt(elapsed / Duration * 443f / 90f * 36));
                if (elapsed >= Duration) Finish();
                Refresh();
            }
        }

        void ReadInput()
        {
            var head = InputDevices.GetDeviceAtXRNode(XRNode.Head);
            bool xr = head.isValid;
            if (xr)
            {
                if (head.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rotation)) view.transform.localRotation = rotation;
                if (head.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 position)) view.transform.localPosition = position + Vector3.up * (position.y < .5f ? 1.65f : 0);
            }
            var kb = Keyboard.current; var mouse = Mouse.current;
            if (!xr && mouse != null && mouse.rightButton.isPressed)
            {
                var delta = mouse.delta.ReadValue(); yaw += delta.x * .12f; pitch = Mathf.Clamp(pitch - delta.y * .12f, -65, 65);
                view.transform.localRotation = Quaternion.Euler(pitch, yaw, 0);
            }
            if (kb != null)
            {
                if (kb.digit1Key.wasPressedThisFrame) Teleport(0);
                if (kb.digit2Key.wasPressedThisFrame) Teleport(1);
                if (kb.digit3Key.wasPressedThisFrame) Teleport(2);
                if (kb.digit4Key.wasPressedThisFrame) Teleport(3);
                if (kb.rKey.wasPressedThisFrame) Reset(Defense);
                if (kb.mKey.wasPressedThisFrame) Act(14);
            }
            Ray ray = new Ray(view.transform.position, view.transform.forward);
            if (!xr && mouse != null && !mouse.rightButton.isPressed) ray = view.ScreenPointToRay(mouse.position.ReadValue());
            var hand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
            bool trigger = false;
            if (xr && hand.isValid)
            {
                if (hand.TryGetFeatureValue(CommonUsages.devicePosition,out Vector3 p) && hand.TryGetFeatureValue(CommonUsages.deviceRotation,out Quaternion q))
                    ray = new Ray(player.TransformPoint(p), player.rotation * q * Vector3.forward);
                hand.TryGetFeatureValue(CommonUsages.triggerButton, out trigger);
            }
            bool pressed = (trigger && !triggerHeld) || (mouse != null && mouse.leftButton.wasPressedThisFrame) || (kb != null && kb.eKey.wasPressedThisFrame) || (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame);
            triggerHeld = trigger;
            DataCenterAction target = null;
            float distance = 7;
            if (Physics.Raycast(ray, out RaycastHit hit, 12)) { distance = hit.distance; target = hit.collider.GetComponent<DataCenterAction>(); }
            pointer.SetPosition(0, ray.origin + ray.direction * .12f); pointer.SetPosition(1, ray.GetPoint(distance));
            pointer.enabled = xr;
            if (focused != target) { if (focused) focused.Highlight(false); focused = target; dwell = 0; if (focused) focused.Highlight(true); }
            if (target)
            {
                dwell += Time.deltaTime;
                // Gaze fallback for phone/Cardboard: look away to rearm after each activation.
                if (pressed || (xr && !hand.isValid && dwell > 1.8f && dwell < 100)) { target.Activate(); dwell = 1000; }
            }
        }

        public void Teleport(int station) { if (!teleporting) StartCoroutine(Jump(station)); }
        IEnumerator Jump(int station)
        {
            teleporting = true;
            // Brief opaque fade prevents perceived continuous locomotion.
            var overlay = new GameObject("Transición", typeof(Canvas), typeof(UnityEngine.UI.Image));
            var canvas = overlay.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 1000;
            var image = overlay.GetComponent<UnityEngine.UI.Image>(); image.color = Color.black;
            yield return null;
            Vector3[] positions = { new Vector3(0,0,-6.3f), new Vector3(-2.8f,0,.1f), new Vector3(2.8f,0,.1f), new Vector3(0,0,2.3f) };
            player.position = positions[Mathf.Clamp(station,0,3)];
            yaw = 0; pitch = 0;
            if (!InputDevices.GetDeviceAtXRNode(XRNode.Head).isValid) view.transform.localRotation = Quaternion.Euler(0,yaw,0);
            yield return new WaitForSeconds(.12f); Destroy(overlay); teleporting = false;
        }

        public void Finish()
        {
            if (!Running) return;
            elapsed = Duration; Orders = Blocked ? 0 : 36; Running = false; Complete = true;
            alarm.Stop();
            if (Defense) DefenseSeen = true; else AttackSeen = true;
            int evidence = (Evidence ? 1 : 0) + (PrinterEvidence ? 1 : 0) + (SwitchEvidence ? 1 : 0);
            result.text = "INFORME / " + (Blocked ? "INTRUSIÓN CONTENIDA" : "ÓRDENES EMITIDAS") +
                "\nATAQUE DE REFERENCIA    36 órdenes / USD 951 M\nTU SESIÓN    " + Orders + " órdenes / USD " + (Orders == 0 ? "0" : "951 M") +
                "\nEvidencias " + evidence + "/3  ·  Controles " + ((Mfa?1:0)+(Schedule?1:0)+(Siem?1:0)) + "/3" +
                "\n" + (Siem ? "SIEM: auditoría centralizada preservada." : "Auditoría local: vulnerable a borrado.") +
                "\n" + (AttackSeen && DefenseSeen ? "COMPARACIÓN COMPLETADA" : "Prueba el otro modo para completar la comparación.");
            Speak(Blocked ? 8 : 5, Blocked ? "Simulación finalizada. Cero órdenes emitidas. Revisa si activaste también SIEM para preservar la auditoría." : "Simulación finalizada. Treinta y seis órdenes emitidas por novecientos cincuenta y un millones de dólares. Repite en modo defensa para impedir el acceso.");
            Refresh();
        }

        void Speak(int index, string text)
        {
            if (!isActiveAndEnabled) return;
            message = text; subtitle.text = "ASISTENTE / " + text;
            subtitleUntil = Time.time + 14;
            narrator.Stop();
            if (index < narration.Length && narration[index]) { narrator.clip = narration[index]; narrator.Play(); subtitleUntil = Time.time + narration[index].length / narrator.pitch + 1; }
        }

        public void Refresh()
        {
            int minutes = (20*60+36 + Mathf.RoundToInt(elapsed / Duration * 443)) % 1440;
            clock.text = $"{minutes/60:00}:{minutes%60:00}";
            dashboard.text = (Defense ? "DEFENSA / " : "ATAQUE 2016 / ") + (Complete ? "FINALIZADO" : Running ? "SESIÓN EN CURSO" : "EN ESPERA") +
                $"\n<color=#40FFAA>{Orders:00}</color> ÓRDENES    /    USD {(Orders * 951f / 36).ToString("0.0",System.Globalization.CultureInfo.InvariantCulture)} M\n04 FEB 2016    ·    20:36 → 03:59";
            controls.text = $"MFA                         {(Mfa ? "[ACTIVO]" : "[INACTIVO]")}\nFUERA DE HORARIO   {(Schedule ? "[BLOQUEO]" : "[SIN ALERTA]")}\nSIEM                        {(Siem ? "[INMUTABLE]" : "[LOCAL]")}";
            logs.text = "SWIFT / AUDITORÍA" + (Evidence ? "  [EVIDENCIA GUARDADA]" : "") +
                "\n20:36:00  LOGIN / cuenta privilegiada" + (Blocked ? " / DENEGADO" : " / ACEPTADO") +
                "\n20:36:02  MFA / " + (Mfa ? "DESAFÍO REQUERIDO" : "NO CONFIGURADO") +
                "\n20:41:17  " + (Siem ? "COPIA SIEM / EVENTO PRESERVADO" : "<color=#FF675B>████ HUECO / REGISTRO BORRADO ████</color>") +
                "\n21:02:45  PRINT / DISPOSITIVO OFFLINE\n22:06:00  " + (Blocked ? "TRANSFERENCIAS BLOQUEADAS" : (Orders == 36 ? "36 ÓRDENES EMITIDAS" : "COLA DE TRANSFERENCIAS")) +
                "\n03:59:00  " + (Complete ? "FIN DE RECONSTRUCCIÓN" : "[PENDIENTE]");
            printerStatus.text = "IMPRESORA / APAGADA\n" + (PrinterEvidence ? "EVIDENCIA PRESERVADA" : "Sin confirmaciones impresas");
            objective.text = "MISIÓN 03 / CENTRO DE DATOS\n" + message + "\n\n" +
                (Evidence ? "✓" : "○") + " Log    " + (PrinterEvidence ? "✓" : "○") + " Impresora    " + (SwitchEvidence ? "✓" : "○") + " Switch";
            foreach (var action in actions) if (action && action.label && action.id >= 4 && action.id <= 6)
                action.label.text = (action.id == 4 ? "01 / MFA" : action.id == 5 ? "02 / HORARIO" : "03 / SIEM") + "  /  " +
                    ((action.id == 4 ? Mfa : action.id == 5 ? Schedule : Siem) ? "ACTIVO" : "ACTIVAR");
        }

        void OnDestroy()
        {
            if(pointer) { Destroy(pointer.sharedMaterial); Destroy(pointer.gameObject); }
            if(hum) Destroy(hum); if(siren) Destroy(siren);
        }
    }
}
