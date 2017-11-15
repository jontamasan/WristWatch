using HutongGames.PlayMaker;
using MSCLoader;
using System;
using UnityEngine;

namespace wrist_watch
{
    public class WristWatch : Mod
    {

        public override string ID => "Wristwatch";
        public override string Name => "Wristwatch";
        public override string Author => "PigoenBB";
        public override string Version => "0.2";

        public override bool UseAssetsFolder => true;

        Keybind _wristwatchKey = new Keybind("WristwatchKey", "Wristwatch", KeyCode.G);

        private const float HOUR_DEGREES = 30f;
        private const float MINUTE_DEGREES = 6f;

        // object
        private GameObject _WRISTWATCH;
        private GameObject _ARM;
        private GameObject _needleHours;
        private GameObject _needleMinutes;
        private GameObject _suomiNeedleHour;
        private GameObject _suomiNeedleMinute;

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
            _WRISTWATCH = UnityEngine.Object.Instantiate<GameObject>(ab.LoadAsset<GameObject>("hand_left"));
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
                if (!Init()) return;
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

            // positioning vector3.y of watch depending on fov
            if (_cameraComponent.fieldOfView != _fov)
            {
                var posDelta = _cameraComponent.fieldOfView - _fov;
                _WRISTWATCH.transform.localPosition += new Vector3(0f, -posDelta / 250f);
                _fov = _cameraComponent.fieldOfView;
            }

            _needleHours.transform.localEulerAngles = new Vector3(0, -_suomiNeedleHour.transform.localEulerAngles.y + 60);
            _needleMinutes.transform.localEulerAngles = new Vector3(0, -_suomiNeedleMinute.transform.localEulerAngles.y);

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
        private bool Init()
        {
            var FPSCamera = GameObject.Find("FPSCamera");
            if (!FPSCamera) return false;
            _WRISTWATCH.transform.parent = FPSCamera.transform;
            _WRISTWATCH.layer = GameObject.Find("PLAYER").layer;
            _WRISTWATCH.transform.localPosition = new Vector3(-0.05f, -0.5f, 0.5f);
            _WRISTWATCH.transform.localEulerAngles = new Vector3(0f, 270f);
            _cameraComponent = FPSCamera.GetComponent<Camera>();
            _fov = _cameraComponent.fieldOfView;

            _isPlayerSleeps = FsmVariables.GlobalVariables.FindFsmBool("PlayerSleeps");
            _isFsmHandLeft = FsmVariables.GlobalVariables.FindFsmBool("PlayerHandLeft");

            _suomiNeedleHour = GameObject.Find("YARD/Building/SuomiClock/Clock/hour/NeedleHour");
            _suomiNeedleMinute = GameObject.Find("YARD/Building/SuomiClock/Clock/minute/NeedleMinute");

            return true;
        }
    }
}
