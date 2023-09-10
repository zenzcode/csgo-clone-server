using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Helper
{
    public class PersistentSceneLoader : MonoBehaviour
    {
        private void Awake()
        {
            SceneManager.LoadScene("Lobby", LoadSceneMode.Additive);
        }
    }
}
