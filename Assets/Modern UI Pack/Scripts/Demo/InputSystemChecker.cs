﻿using UnityEngine;
#if ENABLE_INPUT_SYSTEM
#endif

namespace Michsky.MUIP
{
    public class InputSystemChecker : MonoBehaviour
    {
        private void Awake()
        {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            InputSystemUIInputModule tempModule = gameObject.GetComponent<InputSystemUIInputModule>();

            if (tempModule == null)
            {
                Debug.LogError("<b>[Modern UI Pack]</b> Input System is enabled, but <b>'Input System UI Input Module'</b> is missing. " +
                    "Select the event system object, and click the <b>'Replace'</b> button.");
            }
#endif
        }
    }
}