﻿using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Michsky.MUIP
{
    public class DemoElementSwayParent : MonoBehaviour
    {
        [SerializeField]
        private Animator titleAnimator;

        [SerializeField]
        private TextMeshProUGUI elementTitle;

        [SerializeField]
        private TextMeshProUGUI elementTitleHelper;

        private readonly List<DemoElementSway> elements = new();
        private int prevIndex;

        private void Awake()
        {
            foreach (Transform child in transform)
                elements.Add(child.GetComponent<DemoElementSway>());
        }

        public void DissolveAll (DemoElementSway currentSway)
        {
            for (var i = 0; i < elements.Count; ++i)
            {
                if (elements[i] == currentSway)
                {
                    elements[i].Active();
                    continue;
                }

                elements[i].Dissolve();
            }
        }

        public void HighlightAll()
        {
            for (var i = 0; i < elements.Count; ++i)
                elements[i].Highlight();
        }

        public void SetWindowManagerButton (int index)
        {
            if (elements.Count == 0)
            {
                StartCoroutine("SWMHelper", index);
                return;
            }

            for (var i = 0; i < elements.Count; ++i)
                if (i == index)
                {
                    elements[i].WindowManagerSelect();
                }
                else
                {
                    if (elements[i].wmSelected == false)
                        continue;
                    elements[i].WindowManagerDeselect();
                }

            if (titleAnimator == null)
                return;

            elementTitleHelper.text = elements[prevIndex].gameObject.name;
            elementTitle.text = elements[index].gameObject.name;

            titleAnimator.Play("Idle");
            titleAnimator.Play("Transition");

            prevIndex = index;
        }

        private IEnumerator SWMHelper (int index)
        {
            yield return new WaitForSeconds(0.1f);
            SetWindowManagerButton(index);
        }
    }
}