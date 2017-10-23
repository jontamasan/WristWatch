using HutongGames.PlayMaker;
using MSCLoader;
using UnityEngine;

namespace wrist_watch
{
    public class WristWatch : Mod
    {
        public override string ID => "Wristwatch";
        public override string Name => "Wristwatch";
        public override string Author => "PigoenBB";
        public override string Version => "0.1";

        public override bool UseAssetsFolder => true;

        Keybind _wristwatchKey = new Keybind("WristwatchKey", "Wristwatch", KeyCode.G);

        private const float HOUR_DEGREES = 30f;
        private const float MINUTE_DEGREES = 6f;

        // object
        private GameObject _WRISTWATCH;
        private GameObject _ARM;
        private GameObject _needleHours;
        private GameObject _needleMinutes;

        // clock data
        private FsmFloat m_rawMinutes;
        private FsmInt m_fsmHours;
        private int m_hours;
        private int _hourCount;
        private float m_minutes = 0f;
        private bool _doUpdateHour;

        // flags
        private FsmBool _isPlayerSleeps;
        private FsmBool _isFsmHandLeft;
        private bool _isSlept;
        private bool _isInit = true;

        // fov
        private Camera _cameraComponent;
        private float _fov;

        // Called when mod is loading
        public override void OnLoad()
        {
            Keybind.Add(this, _wristwatchKey);

            // load bundle
            var ab = LoadAssets.LoadBundle(this, "wrist_watch.unity3d");
            _WRISTWATCH = Object.Instantiate<GameObject>(ab.LoadAsset<GameObject>("hand_left"));
            _WRISTWATCH.name = "Wristwatch";
            _ARM = _WRISTWATCH.transform.GetChild(0).gameObject;
            _needleHours = _ARM.transform.GetChild(0).gameObject.transform.GetChild(1).gameObject;
            _needleMinutes = _ARM.transform.GetChild(0).gameObject.transform.GetChild(2).gameObject;
            ab.Unload(false);
        }

        // Update is called once per frame
        public override void Update()
        {
            if (Application.loadedLevelName != "GAME")
            {
                return;
            }

            if (_isInit)
            {
                if (GameObject.Find("FPSCamera/FPSCamera") == null) return;
                Init();
                _isInit = false;
            }

            if (_isPlayerSleeps.Value == true)
            {
                _isSlept = true;
            }

            if (_isSlept && _isPlayerSleeps.Value == false)
            {
                _isInit = true;
                _isSlept = false;
            }

            // positioning pos.y of watch depending on fov
            if (_cameraComponent.fieldOfView != _fov)
            {
                var posDelta = _cameraComponent.fieldOfView - _fov;
                _WRISTWATCH.transform.localPosition += new Vector3(0f, -posDelta / 250f);
                _fov = _cameraComponent.fieldOfView;
            }

            // update hours
            if (System.Math.Floor(m_rawMinutes.Value) != 0 && _doUpdateHour == false)
            {
                _doUpdateHour = true;
            }
            if (System.Math.Floor(m_rawMinutes.Value) == 0 && _doUpdateHour == true)
            {
                _hourCount++;
                if (_hourCount >= 2)
                {
                    m_hours++;
                    m_hours = m_hours >= 12 ? m_hours - 12 : m_hours;
                    _hourCount = 0;
                }
                _doUpdateHour = false;
            }
            // update minutes
            if (_hourCount == 0)
            {
                m_minutes = m_rawMinutes.Value / 2;
            }
            else
            {
                m_minutes = m_rawMinutes.Value / 2 + 30;
            }
            // rotate watch needle
            _needleMinutes.transform.localEulerAngles = new Vector3(0f, m_minutes * MINUTE_DEGREES);
            _needleHours.transform.localEulerAngles = new Vector3(0f, (m_hours * HOUR_DEGREES) + (m_minutes * MINUTE_DEGREES / 12));

            // show&hide wristwatch
            if (_wristwatchKey.IsDown())
            {
                var isRise = _ARM.GetComponent<Animator>().GetBool("isRise");
                if (_isFsmHandLeft.Value && isRise)
                {
                    _ARM.GetComponent<Animator>().SetBool("isRise", false);
                    _isFsmHandLeft.Value = false;
                }
                else if (_isFsmHandLeft.Value == false && isRise == false)
                {
                    _ARM.GetComponent<Animator>().SetBool("isRise", true);
                    _isFsmHandLeft.Value = true;
                }
            }
        }

        // Initialize and when player slept
        private void Init()
        {
            var FPSCamera = GameObject.Find("FPSCamera/FPSCamera");
            _WRISTWATCH.transform.parent = FPSCamera.transform;
            _cameraComponent = FPSCamera.GetComponent<Camera>();
            _fov = _cameraComponent.fieldOfView;
            /****/
            _WRISTWATCH.transform.localPosition = new Vector3(-0.05f, -0.5f, 0.5f);
            // load clock data
            m_rawMinutes = FsmVariables.GlobalVariables.FindFsmFloat("ClockMinutes"); // 1 = 1/2 minute, reseted per 60 = 30 minutes
            _isPlayerSleeps = FsmVariables.GlobalVariables.FindFsmBool("PlayerSleeps");
            _isFsmHandLeft = FsmVariables.GlobalVariables.FindFsmBool("PlayerHandLeft");

            foreach (var fsm in Resources.FindObjectsOfTypeAll<PlayMakerFSM>())
            {
                if (fsm.name == "SUN")
                {
                    m_fsmHours = fsm.FsmVariables.GetFsmInt("Time");
                    /*m_fsmMinutes = fsm.FsmVariables.GetFsmFloat("Minutes");*/
                    break;
                }
            }

            _hourCount = 0;
            m_minutes = m_rawMinutes.Value / 2;
            m_hours = m_fsmHours.Value >= 12 ? m_fsmHours.Value - 12 : m_fsmHours.Value;

            _WRISTWATCH.transform.localEulerAngles = new Vector3(0f, 270f);
        }

#if DEBUG
        public override void OnGUI()
        {
            GUI.Label(new Rect(0, 20, 500, 500), "minutes: " + m_minutes);
            GUI.Label(new Rect(0, 40, 500, 500), "hour: " + m_hours);
            GUI.Label(new Rect(0, 60, 500, 500), "hour: " + m_fsmHours.Value);
        }
#endif
    }
}
