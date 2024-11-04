
using Guru;
using UnityEngine;

public class DebugDemo : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GuruDebugger.Init();

        GuruDebugger.Instance.AddOption("INFO/page1", "DebugDemo")
            .AddButton("AAA", () =>
            {
                Debug.Log("AAA");
            })
            .AddLabel("Test");
        
        
        GuruDebugger.Show();
    }

  
}
