using HutongGames.PlayMaker;
using MSCLoader;
using UnityEngine;

namespace wrist_watch
{
    public class wrist_watch : Mod
    {
        private float m_hourDegrees = 30f;
        private float m_minuteDegrees = 6f;

        private GameObject m_WRISTWATCH;
        private GameObject m_needleHours;
        private GameObject m_needleMinutes;

        private FsmFloat m_rawMinutes;
        private FsmFloat m_fsmMinutes;
        private FsmInt m_fsmHours;
        private int m_hours;

        private FsmBool m_isPlayerSleeps;
        private FsmBool m_isFsmHandLeft;
        private bool m_isSlept;
        private bool m_isRise;
        private bool m_isInit = true;

        Keybind m_wristwatchKey = new Keybind("WristwatchKey", "Wristwatch", KeyCode.G);

        public override string ID => "Wristwatch";
        public override string Name => "Wristwatch";
        public override string Author => "PigoenBB";
        public override string Version => "0.1";

        public override bool UseAssetsFolder => true;

        //Called when mod is loading
        public override void OnLoad()
        {
            Keybind.Add(this, m_wristwatchKey);

            // load bundle
            var ab = LoadAssets.LoadBundle(this, "wrist_watch.unity3d");
            m_WRISTWATCH = UnityEngine.Object.Instantiate<GameObject>(ab.LoadAsset<GameObject>("hand_left"));
            m_WRISTWATCH.name = "Wristwatch";
            m_needleHours = m_WRISTWATCH.transform.GetChild(1).gameObject.transform.GetChild(1).gameObject;
            m_needleMinutes = m_WRISTWATCH.transform.GetChild(1).gameObject.transform.GetChild(2).gameObject;
            ab.Unload(false);
        }

        // Update is called once per frame
        public override void Update()
        {
            if (Application.loadedLevelName != "GAME")
            {
                return;
            }

            if (m_isInit)
            {
                Init();
                m_isInit = false;
            }

            if (m_isPlayerSleeps.Value == true)
            {
                m_isSlept = true;
            }

            if (m_isSlept && m_isPlayerSleeps.Value == false)
            {
                m_isInit = true;
                m_isSlept = false;
            }

            // show&hide wristwatch
            if (m_wristwatchKey.IsDown())
            {
                if (m_isFsmHandLeft.Value && m_isRise)
                {
                    m_WRISTWATCH.GetComponent<Animator>().SetBool("isRise", false);
                    m_isRise = false;    
                    m_isFsmHandLeft.Value = false;
                }
                else if (m_isFsmHandLeft.Value == false && m_isRise == false)
                {
                    m_WRISTWATCH.GetComponent<Animator>().SetBool("isRise", true);
                    m_isRise = true;
                    m_isFsmHandLeft.Value = true;
                }
            }

            // rotate watch needle
            m_needleMinutes.transform.localEulerAngles = new Vector3(0f, m_fsmMinutes.Value * m_minuteDegrees);
            m_needleHours.transform.localEulerAngles = new Vector3(0f, (m_hours * m_hourDegrees) + (m_fsmMinutes.Value * m_minuteDegrees / m_hourDegrees * 2));
        }

        private void Init()
        {
            m_WRISTWATCH.transform.parent = GameObject.Find("FPSCamera").transform;
            // load clock data
            m_rawMinutes = FsmVariables.GlobalVariables.FindFsmFloat("ClockMinutes");
            m_isPlayerSleeps = FsmVariables.GlobalVariables.FindFsmBool("PlayerSleeps");
            m_isFsmHandLeft = FsmVariables.GlobalVariables.FindFsmBool("PlayerHandLeft");
            foreach (var fsm in Resources.FindObjectsOfTypeAll<PlayMakerFSM>())
            {
                if (fsm.name == "SUN")
                {
                    m_fsmHours = fsm.FsmVariables.FindFsmInt("Time");
                    m_fsmMinutes = fsm.FsmVariables.FindFsmFloat("Minutes");
                    break;
                }
            }

            m_hours = m_fsmHours.Value;
            m_hours = m_hours > 12 ? m_hours - 12 : m_hours;
            m_needleHours.transform.localEulerAngles = new Vector3(0f, m_hours * m_hourDegrees);
        }
    }
}
