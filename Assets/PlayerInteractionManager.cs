// using AvatarHands.Enums;
using UnityEngine;
using UnityEngine.InputSystem;
// using UnityEngine.XR.Interaction.Toolkit;

public class PlayerInteractionManager : MonoBehaviour
{
    // #region [ References ]
    // // - - -
    //     [SerializeField] private XRRayInteractor _leftHandRayInteractor;
    //     [SerializeField] private XRRayInteractor _rightHandRayInteractor;
    //     
    //     [SerializeField] private InputActionReference _leftTriggerDownInputActionReference;
    //     [SerializeField] private InputActionReference _rightTriggerDownInputActionReference;
    // // - - -
    // #endregion
    //
    // #region [ Behaviour ]
    // // - - -
    //     public bool LeftHandVisibility => _logicVisibility && _avatarLeftHandVisibility && _transitionVisibility;
    //     public bool RightHandVisibility => _logicVisibility && _avatarRightHandVisibility && _transitionVisibility;
    //
    //     private bool _logicVisibility = true;
    //     public bool LogicVisibility
    //     {
    //         get => _logicVisibility;
    //         set
    //         {
    //             _logicVisibility = value;
    //
    //             _leftHandRayInteractor.enabled = (LeftHandVisibility && _activeRayInteractionHand == Handedness.Left);
    //             _rightHandRayInteractor.enabled = (RightHandVisibility && _activeRayInteractionHand == Handedness.Right);
    //         }
    //     } 
    //     
    //     private bool _avatarLeftHandVisibility = true;
    //     public bool AvatarLeftHandVisibility
    //     {
    //         get => _avatarLeftHandVisibility;
    //         set
    //         {
    //             _avatarLeftHandVisibility = value;
    //
    //             _leftHandRayInteractor.enabled = (LeftHandVisibility && _activeRayInteractionHand == Handedness.Left);
    //         }
    //     }
    //     
    //     private bool _avatarRightHandVisibility = true;
    //     public bool AvatarRightHandVisibility
    //     {
    //         get => _avatarRightHandVisibility;
    //         set
    //         {
    //             _avatarRightHandVisibility = value;
    //             
    //             _rightHandRayInteractor.enabled = (RightHandVisibility && _activeRayInteractionHand == Handedness.Right);
    //         }
    //     }
    //     
    //     private bool _transitionVisibility = true;
    //     public bool TransitionVisibility
    //     {
    //         get => _transitionVisibility;
    //         set
    //         {
    //             _transitionVisibility = value;
    //
    //             _leftHandRayInteractor.enabled = (LeftHandVisibility && _activeRayInteractionHand == Handedness.Left);
    //             _rightHandRayInteractor.enabled = (RightHandVisibility && _activeRayInteractionHand == Handedness.Right);
    //         }
    //     }
    //     
    //     private Handedness _activeRayInteractionHand;
    //     public Handedness ActiveRayInteractionHand
    //     {
    //         get => _activeRayInteractionHand;
    //         set
    //         {
    //             _activeRayInteractionHand = value;
    //
    //             _leftHandRayInteractor.enabled = (LeftHandVisibility && value == Handedness.Left);
    //             _rightHandRayInteractor.enabled = (RightHandVisibility && value == Handedness.Right);
    //         }
    //     }
    // // - - -
    // #endregion
    //
    //
    //
    //
    //
    // private void OnEnable()
    // {
    //     _leftTriggerDownInputActionReference.action.performed += SetLeftTriggerAsActive;
    //     _rightTriggerDownInputActionReference.action.performed += SetRightTriggerAsActive;
    // }
    //
    // private void OnDisable()
    // {
    //     _leftTriggerDownInputActionReference.action.performed -= SetLeftTriggerAsActive;
    //     _rightTriggerDownInputActionReference.action.performed -= SetRightTriggerAsActive;
    // }
    //
    // private void Start() => ActiveRayInteractionHand = Handedness.Left;
    //
    //
    //
    // private void SetLeftTriggerAsActive(InputAction.CallbackContext callbackContext) => ActiveRayInteractionHand = Handedness.Left;
    // private void SetRightTriggerAsActive(InputAction.CallbackContext callbackContext) => ActiveRayInteractionHand = Handedness.Right;
    //
    // private void OnSceneTransitionStarted() => TransitionVisibility = false;
    // private void OnSceneTransitionEnded() => TransitionVisibility = true;
}
