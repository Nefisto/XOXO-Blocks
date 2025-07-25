using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
#endif

namespace Michsky.MUIP
{
    [AddComponentMenu("Modern UI Pack/Tools/Element Tabbing")]
    public class ElementTabbing : MonoBehaviour
    {
        public enum ObjectType
        {
            Button,
            InputField
        }

        private bool catchedObject;

        // Helpers
        private int currentIndex = -1;
        private ObjectType type;

        private void Update()
        {
#if ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(KeyCode.Tab))
#elif ENABLE_INPUT_SYSTEM
            if (Keyboard.current.tabKey.wasPressedThisFrame)
#endif
            {
                if (currentIndex > transform.childCount - 2)
                {
                    SelectElement(0);
                    return;
                }

                if (catchedObject && type == ObjectType.Button)
                    transform.GetChild(currentIndex).GetComponent<ButtonManager>().OnDeselect(null);
                else if (catchedObject && type == ObjectType.InputField)
                    transform.GetChild(currentIndex).GetComponent<CustomInputField>().inputText.DeactivateInputField();

                currentIndex++;

                for (var i = 0; i < transform.childCount; ++i)
                {
                    if (i < currentIndex)
                        continue;

                    if (transform.GetChild(i).GetComponent<ButtonManager>() != null)
                    {
                        transform.GetChild(i).GetComponent<ButtonManager>().OnSelect(null);
                        EventSystem.current.SetSelectedGameObject(transform.GetChild(i).gameObject);
                        type = ObjectType.Button;
                        break;
                    }

                    if (transform.GetChild(i).GetComponent<CustomInputField>() != null)
                    {
                        transform.GetChild(i).GetComponent<CustomInputField>().inputText.ActivateInputField();
                        EventSystem.current.SetSelectedGameObject(transform.GetChild(i).gameObject);
                        type = ObjectType.InputField;
                        break;
                    }

                    catchedObject = false;
                }
            }
        }

        private void SelectElement (int index)
        {
            currentIndex = index;

            if (transform.GetChild(index).GetComponent<ButtonManager>() != null)
            {
                transform.GetChild(index).GetComponent<ButtonManager>().OnSelect(null);
                EventSystem.current.SetSelectedGameObject(transform.GetChild(index).gameObject);
                type = ObjectType.Button;
            }

            else if (transform.GetChild(index).GetComponent<CustomInputField>() != null)
            {
                transform.GetChild(index).GetComponent<CustomInputField>().inputText.ActivateInputField();
                EventSystem.current.SetSelectedGameObject(transform.GetChild(index).gameObject);
                type = ObjectType.InputField;
            }

            else
            {
                catchedObject = false;
            }
        }
    }
}