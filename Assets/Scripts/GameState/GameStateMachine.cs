using System.Collections.Generic;
using UnityEngine;

namespace GameState
{
    public abstract class GameMultiStateMachine : MonoBehaviour, IGameStateMachine
    {
        private List<IGameObjectState> activeStates;

        public virtual void ActivateState(IGameObjectState state)
        {
            state.OnStateStart();
            activeStates.Add(state);
        }

        public virtual void DisActivateAllStates()
        {
            foreach (var gameObjectState in activeStates) gameObjectState.OnStateEnd();

            activeStates.Clear();
        }
    }

    public abstract class GameStateMachine : MonoBehaviour, IGameStateMachine
    {
        private IGameObjectState activeState;

        private void Awake()
        {
            GameObjectHelper.AssertOnlyComponentOfType<IGameStateMachine>(this);
        }

        public void ActivateState(IGameObjectState state)
        {
            if (activeState != null) activeState.OnStateEnd();

            activeState = state;
            if (activeState != null) activeState?.OnStateStart();
        }
    }
}