using UnityEngine;
using UnityEngine.SceneManagement;

public class StageFailedUI : MonoBehaviour
{
    [Header("UI Panel")]
    [SerializeField] private GameObject stageFailedPanel;

    public void ShowStageFailedScreen()
    {
        if (stageFailedPanel != null)
        {
            stageFailedPanel.SetActive(true);
            Time.timeScale = 0f; // Pause the game
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        Scene activeScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(activeScene.name);
    }

    // ฟังก์ชันสำหรับปุ่มกลับไปหน้า Start Menu
    public void GoToStartMenu()
    {
        Time.timeScale = 1f; // สำคัญมาก: ต้องคืนค่าเวลาให้กลับมาเดินปกติ ไม่งั้นฉากใหม่จะค้าง

        // ใส่ชื่อ Scene ให้ตรงกับตัวพิมพ์เล็ก-ใหญ่ (ตัวอย่างนี้ใช้ "startmenu")
        SceneManager.LoadScene("startmenu");
    }
}