using UnityEngine;
using UnityEngine.SceneManagement; // Thư viện bắt buộc để chuyển Scene

public class AMenu : MonoBehaviour
{
    // Hàm này sẽ được gọi khi bấm nút PlayA
    public void PlayGame()
    {
        // Tải scene tiếp theo trong Build Settings (thường là Scene Gameplay)
        // Bạn cũng có thể dùng tên Scene: SceneManager.LoadScene("GameplayScene");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    // Hàm này sẽ được gọi khi bấm nút Exit
    public void QuitGame()
    {
        Debug.Log("Game is quitting..."); // Hiện thông báo trong cửa sổ Console

        // Thoát ứng dụng (chỉ có tác dụng khi đã xuất file .exe/.apk)
        Application.Quit();

        // Nếu đang chạy trong Unity Editor thì dừng chế độ Play
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}