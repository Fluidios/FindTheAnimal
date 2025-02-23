using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public abstract class Game : MonoBehaviour
    {
        public Action<bool> OnGameEnds;
        public abstract void StartGameLevel();
        public abstract void ClearGameLevel();
    }
}
