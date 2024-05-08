
using System;
using System.Linq;
using UdonSharp;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Serialization.OdinSerializer.Utilities;

namespace PeskyBox.SeatCollisionManager
{
    // Enum for the layers /shrug
    public enum LayerEnums
    {
        // Builtin layers
        Default,
        TransparentFX = 1,
        IgnoreRaycast = 2,
        Water = 4,
        UI = 5,

        // User layers
        User3 = 3,
        User6 = 6,
        User7 = 7,
        Interactive = 8,
        Player = 9,
        PlayerLocal = 10,
        Environment = 11,
        UiMenu = 12,
        Pickup = 13,
        PickupNoEnvironment = 14,
        StereoLeft = 15,
        StereoRight = 16,
        Walkthrough = 17,
        MirrorReflection = 18,
        Reserved2 = 19,
        Reserved3 = 20,
        Reserved4 = 21,
        User22 = 22,
        User23 = 23,
        User24 = 24,
        User25 = 25,
        User26 = 26,
        User27 = 27,
        User28 = 28,
        User29 = 29,
        User30 = 30,
        User31 = 31
    }
    
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class SeatTogglingManager : UdonSharpBehaviour
    {
        
        [Header("Seat Marker Settings")] [Space(5)] [Header("General Settings")] [SerializeField]
        public bool seatCollidersEnabledByDefault = true;
        public bool tableCollidersEnabledByDefault = true;
        public LayerEnums collisionLayerNameForPlayerCollision = LayerEnums.Default;
        public LayerEnums collisionLayerForNonPlayerCollision = LayerEnums.Walkthrough;

        [Header("VRCSeats and VRCStations")]
        public bool VRCSeatsEnabledByDefault = false;
        public bool VRCSeatsEnabledByDefaultOnDesktop = false;
        
        [Space(5)] [Header("Toggling Objects")]
        public GameObject[] seats;

        public GameObject[] tables;
        
        public VRCStation[] chairStations;
        

        private Collider[] _seats = Array.Empty<Collider>();
        private Collider[] _tables = Array.Empty<Collider>();
        private Collider[] _vrcStationsColliders = Array.Empty<Collider>();

        private bool areTableCollidersOn = true;
        private bool areSeatCollidersOn = true;
        private bool areVRCSeatsEnabled = false;

        public bool AreSeatCollidersOn => areSeatCollidersOn;
        public bool AreTableCollidersOn => areTableCollidersOn;
        public bool AreVRCSeatsEnabled => areVRCSeatsEnabled;

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            Debug.Log($"SeatTogglingManager: Player {player.displayName} joined, updating role");
        }

        private void Start()
        {
            //Find all colliders inside the of the seats and tables
            foreach (var seatObject in seats)
            {
                //Check if the object is null
                if (seatObject == null) continue;
                var seatColliders = seatObject.GetComponentsInChildren<Collider>();
                //Check if the seatColliders array is null
                if (seatColliders == null) continue;
                //Find the parent collider
                var parentCollider = seatObject.GetComponent<Collider>();
                //If the parent collider is not null, add it to the seatColliders array
                if (parentCollider != null)
                {
                    _seats = _AppendColliderToArray(_seats, parentCollider);
                }

                //Add the seatColliders to the _seats array using the append function
                foreach (var collider in seatColliders)
                {
                    _seats = _AppendColliderToArray(_seats, collider);
                }

            }

            foreach (var tableObject in tables)
            {
                //Check if the object is null
                if (tableObject == null) continue;
                var tableColliders = tableObject.GetComponentsInChildren<Collider>();
                //Check if the seatColliders array is null
                if (tableColliders == null) continue;
                //Find the parent collider
                var parentCollider = tableObject.GetComponent<Collider>();
                //If the parent collider is not null, add it to the seatColliders array
                if (parentCollider != null)
                {
                    _tables = _AppendColliderToArray(_tables, parentCollider);
                }

                //Add the seatColliders to the _seats array using the append function
                foreach (var collider in tableColliders)
                {
                    _tables = _AppendColliderToArray(_tables, collider);
                }
            }
            
            // Find all colliders on the VRCStations Objects
            foreach (var vrcStation in chairStations)
            {
                // Find the collider on the VRCStation object
                var collider = vrcStation.GetComponent<Collider>();
                // If the collider is null, skip this object
                if (collider == null) continue;
                // Add the collider to the _vrcStationsColliders array
                _vrcStationsColliders = _AppendColliderToArray(_vrcStationsColliders, collider);
            }

            SetSeatColliderState(seatCollidersEnabledByDefault);
            areSeatCollidersOn = seatCollidersEnabledByDefault;
            SetTableColliderState(tableCollidersEnabledByDefault);
            areTableCollidersOn = tableCollidersEnabledByDefault;
            
            // Check if the VRCSeats are enabled by default on the current platform
            if (!Networking.LocalPlayer.IsUserInVR())
            {
                SetVRCSeatState(VRCSeatsEnabledByDefaultOnDesktop);
            }
            if (InputManager.GetLastUsedInputMethod() == VRCInputMethod.Touch)
            {
                SetVRCSeatState(VRCSeatsEnabledByDefault);
            }
            else
            {
                SetVRCSeatState(VRCSeatsEnabledByDefault);
            }
        }

        public void ToggleSeatColliders()
        {
            // Set the seat colliders to the opposite layer of what they are currently
            foreach (var seat in _seats)
            {
                seat.gameObject.layer = areSeatCollidersOn
                    ? (int)collisionLayerNameForPlayerCollision
                    : (int)collisionLayerForNonPlayerCollision;
            }

            areSeatCollidersOn = !areSeatCollidersOn;
        }

        public void ToggleTableColliders()
        {
            // Set the seat colliders to the opposite layer of what they are currently
            foreach (var table in _tables)
            {
                table.gameObject.layer = areTableCollidersOn
                    ? (int)collisionLayerNameForPlayerCollision
                    : (int)collisionLayerForNonPlayerCollision;
            }

            areTableCollidersOn = !areTableCollidersOn;

        }
        
        public void ToggleVRCSeats()
        {
            // Set the seat colliders to the opposite layer of what they are currently
            foreach (var VRCStation in _vrcStationsColliders)
            {
                VRCStation.enabled = !areVRCSeatsEnabled;
            }
            
            areVRCSeatsEnabled = !areVRCSeatsEnabled;
        }

        public void SetSeatColliderState(bool state)
        {
            //Set the seats to the correct layer
            foreach (var seat in _seats)
            {
                seat.gameObject.layer = state
                    ? (int)collisionLayerNameForPlayerCollision 
                    : (int)collisionLayerForNonPlayerCollision;
            }

            areSeatCollidersOn = state;
        }

        public void SetTableColliderState(bool state)
        {
            //Set the seats to the correct layer
            foreach (var table in _tables)
            {
                table.gameObject.layer = state
                    ? (int)collisionLayerNameForPlayerCollision 
                    : (int)collisionLayerForNonPlayerCollision;
            }

            areTableCollidersOn = state;
        }
        
        public void SetVRCSeatState(bool state)
        {
            //Set the seats to the correct layer
            foreach (var VRCStation in _vrcStationsColliders)
            {
                VRCStation.enabled = state;
            }

            areVRCSeatsEnabled = state;
        }

        private Collider[] _AppendColliderToArray(Collider[] array, Collider value)
        {
            Collider[] newArray = new Collider[array.Length + 1];
            array.CopyTo(newArray, 0);
            newArray[array.Length] = value;
            return newArray;
        }
    }


#if !COMPILER_UDONSHARP && UNITY_EDITOR
    [CustomEditor(typeof(SeatTogglingManager))]
    class SeatTogglingManagerEditor : Editor
    {
        
        [MenuItem("Tools/Seat Collision Manager/Add Seat Collision Manager to Scene")]
        private static void MyCustomFunction()
        {
            // Add a new game object to the scene
            GameObject newGameObject = new GameObject("Seat Collision Manager");
            // Add the Seat Toggle Manager component to the new game object
            newGameObject.AddComponent<SeatTogglingManager>();
        }
        
        public override void OnInspectorGUI()
        {
            SeatTogglingManager seatTogglingManager = (SeatTogglingManager)target;

            //Draw the default inspector
            base.OnInspectorGUI();
            
            GUI.backgroundColor = Color.magenta;
            if (GUILayout.Button("Find all marked objects"))
            {
                // SeatMarker objects
                // Find all objects in the scene with he SeatMarker component and clear the lists
                var seatMarkerList = GameObject.FindObjectsOfType<SeatMarker>();
                seatTogglingManager.seats = Array.Empty<GameObject>();
                seatTogglingManager.tables = Array.Empty<GameObject>();
                
                // Add the objects to the correct list
                foreach (var seatMarker in seatMarkerList)
                {
                    if (seatMarker.furnitureType == seatType.Chair)
                    {
                        seatTogglingManager.seats = seatTogglingManager.seats.Append(seatMarker.gameObject).ToArray();
                    }
                    else if (seatMarker.furnitureType == seatType.Table)
                    {
                        seatTogglingManager.tables = seatTogglingManager.tables.Append(seatMarker.gameObject).ToArray();
                    }
                }
                
                //VRCStation objects
                // Find all VRCStation objects in the scene and clear the list
                var vrcStations = GameObject.FindObjectsOfType<VRCStation>();
                seatTogglingManager.chairStations = Array.Empty<VRCStation>();
                
                // Add the VRCStation objects to the list
                foreach (var vrcStation in vrcStations)
                {
                    // Find the collider on the VRCStation object
                    var collider = vrcStation.GetComponent<Collider>();
                    // If the collider is null, skip this object
                    if (collider == null) continue;
                    // Add the VRCStation object to the list
                    seatTogglingManager.chairStations = seatTogglingManager.chairStations.Append(vrcStation).ToArray();
                    
                }
                
                // Log results
                Debug.Log($"Found {seatTogglingManager.seats.Length} seats and {seatTogglingManager.tables.Length} tables");
                
                // Save the changes
                EditorUtility.SetDirty(target);
            }
            GUI.backgroundColor = Color.white;
        }
    }
#endif

}