using TMPro;
using UnityEngine;

namespace CyberShield.DataCenter
{
    public sealed class DataCenterAction : MonoBehaviour
    {
        public DataCenterSimulation simulation;
        public int id;
        public TMP_Text label;
        MaterialPropertyBlock block;
        public void Activate()
        {
            var mission=simulation.GetComponent<DataCenterMobileMission>();
            if(mission&&mission.enabled)mission.Act(id);else simulation.Act(id);
        }
        public void Highlight(bool on)
        {
            if (block == null) block = new MaterialPropertyBlock();
            block.SetColor("_BaseColor", on ? new Color(.07f,.32f,.25f) : new Color(.018f,.07f,.075f));
            GetComponent<Renderer>().SetPropertyBlock(block);
        }
    }
}
