using Unity.Netcode;
using UnityEngine.SceneManagement;

public static class SceneLoader {

    public enum Scene {
        MainMenu,
        Game,
    }

    public static void LoadSceneNetwork(Scene targetScene) {
        NetworkManager.Singleton.SceneManager.LoadScene(targetScene.ToString(), LoadSceneMode.Single);
    }
}
