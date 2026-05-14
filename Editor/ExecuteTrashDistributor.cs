using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class ExecuteTrashDistributor
{
    static ExecuteTrashDistributor()
    {
        EditorApplication.delayCall += () => {
            if (!SessionState.GetBool("TrashDistributed_V3", false))
            {
                SessionState.SetBool("TrashDistributed_V3", true);
                Debug.Log("[ExecuteTrashDistributor] Automatically executing TrashDistributor.DistributeTrash()...");
                TrashDistributor.DistributeTrash();
            }
        };
    }
}
