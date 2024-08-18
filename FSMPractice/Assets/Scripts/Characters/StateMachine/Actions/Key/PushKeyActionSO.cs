﻿using UnityEngine;
using Pudding.StateMachine;
using Pudding.StateMachine.ScriptableObjects;

[CreateAssetMenu(fileName = "PushKeyAction", menuName = "State Machines/Actions/Push Key Action")]
public class PushKeyActionSO : StateActionSO<PushKeyAction>
{
    public float pushForce = 5.0f;
    public float pushHeight = 50.0f;
}

public class PushKeyAction : StateAction
{
    private PushKeyActionSO _originSO => (PushKeyActionSO)base.OriginSO;
    private InteractionManager _interactionManager;
    private Rigidbody _interactiveObjectRigidbody;

    public override void Awake(StateMachine stateMachine)
    {
        _interactionManager = stateMachine.GetComponent<InteractionManager>();
    }

    public override void OnUpdate() { }
	
	public override void OnStateEnter()
	{
        // 플레이어가 들고 있는 키와 상호작용할 수 있는 오브젝트 감지
        

        if (_interactionManager.currentInteractionType == InteractionType.Key)
        {
            GameObject currentObject = _interactionManager.currentInteractiveObject;

            Collider[] hitColliders = Physics.OverlapSphere(currentObject.transform.position, 1.0f);

            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.CompareTag("Normal"))
                {
                    if (hitCollider.TryGetComponent(out InteractionEventListener listener) && currentObject.TryGetComponent(out Key key))
                    {
                        if (listener.RequiredKey.ID == key.GetKeyID())
                        {
                            InteractWithObject(hitCollider.gameObject, key);
                            return;
                        }
                        else
                        {
                            Debug.Log("키 다름");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("There are no Interaction Event Listener or Key _ PushKeyActionSO.cs");
                    }
                }
            }

            Debug.Log("그냥 던져");

            // 상호작용 못 하면 그냥 던지는 동작
            _interactiveObjectRigidbody = _interactionManager.currentInteractiveObject.GetComponent<Rigidbody>();

            // Init Position to Player position and Add
            _interactiveObjectRigidbody.transform.position = _interactionManager.transform.position + _interactiveObjectRigidbody.transform.forward * 0.1f;
            _interactiveObjectRigidbody.velocity = _interactiveObjectRigidbody.transform.forward * _originSO.pushForce;
        }
    }

    private void InteractWithObject(GameObject target, Key key)
    {
        Debug.Log("키로 오브젝트와 상호작용 중: " + target.name);

        // 키 삭제
        key.Destroy();

        // 오브젝트 교체, 타임라인도 수정해야함
        if (target.TryGetComponent(out ReplaceObject _replaceObject))
        {
            _replaceObject.ChangeObject();
        }
    }

    public override void OnStateExit()
    {
        _interactionManager.currentInteractionType = InteractionType.None;
        _interactionManager.currentInteractiveObject = null;
    }
}
