﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UniversalStorage
{
    public class USCargoSwitch : PartModule
    {
        [KSPField]
        public string SwitchID = string.Empty;
        [KSPField]
        public string CargoBayCenter = string.Empty;
        [KSPField]
        public string CargoBayRadius = string.Empty;
        [KSPField(isPersistant = true)]
        public int CurrentSelection = 0;
        [KSPField]
        public bool DebugMode = false;

        private ModuleCargoBay cargoModule;

        private int[] _SwitchIndices;

        private Vector3[] _CargoCenter;
        private float[] _CargoRadii;

        private EventData<int, int, Part> onUSSwitch;

        private bool debugDraw;
        private float debugDrawTimer = 0;
        
        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            if (String.IsNullOrEmpty(SwitchID)|| String.IsNullOrEmpty(CargoBayCenter) || String.IsNullOrEmpty(CargoBayRadius))
                return;

            _SwitchIndices = USTools.parseIntegers(SwitchID).ToArray();

            _CargoCenter = USTools.parseVectors(CargoBayCenter).ToArray();

            _CargoRadii = USTools.parseSingles(CargoBayRadius).ToArray();

            cargoModule = part.FindModuleImplementing<ModuleCargoBay>();

            onUSSwitch = GameEvents.FindEvent<EventData<int, int, Part>>("onUSSwitch");

            if (onUSSwitch != null)
                onUSSwitch.Add(onSwitch);

            UpdateCargoModule();
        }

        private void OnDestroy()
        {
            if (onUSSwitch != null)
                onUSSwitch.Remove(onSwitch);
        }

        private void OnGUI()
        {
            if (!debugDraw)
                return;

            if (debugDrawTimer < 10)
            {
                debugDrawTimer += Time.deltaTime;

                Vector3 center = part.partTransform.TransformPoint(cargoModule.lookupCenter);

                USTools.DrawSphere(center, Color.green, cargoModule.lookupRadius);
            }
            else
            {
                debugDraw = false;

                debugDrawTimer = 0;
            }
        }

        private void OnDebugSphere()
        {
            if (DebugMode && cargoModule != null)
            {
                debugDraw = true;
                debugDrawTimer = 0;
            }
        }

        private void onSwitch(int index, int selection, Part p)
        {
            if (p != part)
                return;

            if (_SwitchIndices == null || _SwitchIndices.Length <= 0)
            {
                if (String.IsNullOrEmpty(SwitchID))
                    return;

                _SwitchIndices = USTools.parseIntegers(SwitchID).ToArray();
            }

            for (int i = _SwitchIndices.Length - 1; i >= 0; i--)
            {
                if (_SwitchIndices[i] == index)
                {
                    CurrentSelection = selection;
                    
                    UpdateCargoModule();

                    debugDraw = true;

                    break;
                }
            }
        }

        private void UpdateCargoModule()
        {
            if (cargoModule == null || _CargoCenter == null || _CargoRadii == null)
                return;
            
            if (_CargoCenter.Length > CurrentSelection)
                cargoModule.SetLookupCenter(_CargoCenter[CurrentSelection]);

            if (_CargoRadii.Length > CurrentSelection)
                cargoModule.SetLookupRadius(_CargoRadii[CurrentSelection]);
        }

    }
}