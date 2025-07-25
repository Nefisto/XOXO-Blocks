using UnityEngine;
using UnityEngine.EventSystems;

namespace Michsky.MUIP
{
    [RequireComponent(typeof(Animator))]
    public class AnimatedIconHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        public enum PlayType
        {
            Click,
            Hover,
            None
        }

        [Header("Settings")]
        public PlayType playType;

        public Animator iconAnimator;

        private bool isClicked;

        private void Start()
        {
            if (iconAnimator == null)
                iconAnimator = gameObject.GetComponent<Animator>();
        }

        public void OnPointerClick (PointerEventData eventData)
        {
            if (playType == PlayType.Click)
                ClickEvent();
        }

        public void OnPointerEnter (PointerEventData eventData)
        {
            if (playType == PlayType.Hover)
                iconAnimator.Play("In");
        }

        public void OnPointerExit (PointerEventData eventData)
        {
            if (playType == PlayType.Hover)
                iconAnimator.Play("Out");
        }

        public void PlayIn()
        {
            iconAnimator.Play("In");
        }

        public void PlayOut()
        {
            iconAnimator.Play("Out");
        }

        public void ClickEvent()
        {
            if (isClicked)
            {
                PlayOut();
                isClicked = false;
            }
            else
            {
                PlayIn();
                isClicked = true;
            }
        }
    }
}