using HutongGames.PlayMaker;
using MSCLoader;
using System.IO;
using UnityEngine;

namespace wrist_watch
{
    public class wrist_watch : Mod
    {
        public override string ID => "Wristwatch";
        public override string Name => "Wristwatch";
        public override string Author => "PigoenBB";
        public override string Version => "0.1";

        public override bool UseAssetsFolder => true;

        Keybind _wristwatchKey = new Keybind("WristwatchKey", "Wristwatch", KeyCode.G);

        private float m_hourDegrees = 30f;
        private float m_minuteDegrees = 6f;

        private GameObject _WRISTWATCH;
        private GameObject _ARM;
        private GameObject _needleHours;
        private GameObject _needleMinutes;

        private FsmFloat m_rawMinutes;
        private FsmFloat m_fsmMinutes;
        private FsmInt m_fsmHours;
        private int m_hours;

        private FsmBool _isPlayerSleeps;
        private FsmBool _isFsmHandLeft;
        private bool _isSlept;
        private bool _isInit = true;

        private Camera _cameraComponent;
        private float _fov;

        //Called when mod is loading
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

            // position.y of watch
            if (_cameraComponent.fieldOfView != _fov)
            {
                var posDelta = _cameraComponent.fieldOfView - _fov;
                _WRISTWATCH.transform.localPosition += new Vector3(0f, -posDelta / 250f);
                _fov = _cameraComponent.fieldOfView;
            }

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

            // rotate watch needle
            _needleMinutes.transform.localEulerAngles = new Vector3(0f, m_fsmMinutes.Value * m_minuteDegrees);
            _needleHours.transform.localEulerAngles = new Vector3(0f, (m_hours * m_hourDegrees) + (m_fsmMinutes.Value * m_minuteDegrees / m_hourDegrees * 2));
        }

        private void Init()
        {
            var FPSCamera = GameObject.Find("FPSCamera/FPSCamera");
            _WRISTWATCH.transform.parent = FPSCamera.transform;
            _cameraComponent = FPSCamera.GetComponent<Camera>();
            _fov = _cameraComponent.fieldOfView;
            /****/
            _WRISTWATCH.transform.localPosition = new Vector3(-0.05f, -0.5f, 0.5f);
            // load clock data
            m_rawMinutes = FsmVariables.GlobalVariables.FindFsmFloat("ClockMinutes");
            _isPlayerSleeps = FsmVariables.GlobalVariables.FindFsmBool("PlayerSleeps");
            _isFsmHandLeft = FsmVariables.GlobalVariables.FindFsmBool("PlayerHandLeft");

            foreach (var fsm in Resources.FindObjectsOfTypeAll<PlayMakerFSM>())
            {
                if (fsm.name == "SUN")
                {
                    m_fsmHours = fsm.FsmVariables.GetFsmInt("Time");
                    m_fsmMinutes = fsm.FsmVariables.GetFsmFloat("Minutes");
                    break;
                }
            }

            m_hours = m_fsmHours.Value;
            m_hours = m_hours > 12 ? m_hours - 12 : m_hours;
            _needleHours.transform.localEulerAngles = new Vector3(0f, m_hours * m_hourDegrees);
        }
    }
}
