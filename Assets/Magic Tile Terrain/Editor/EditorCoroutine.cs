using System.Collections;
using UnityEditor;

namespace MagicSandbox.TileTerrain
{
    public class EditorCoroutine
    {
        readonly IEnumerator coroutine;

        EditorCoroutine(IEnumerator routine)
        {
            this.coroutine = routine;
        }

        public static EditorCoroutine StartCoroutine(IEnumerator routine)
        {
            EditorCoroutine coroutine = new EditorCoroutine(routine);
            coroutine.Start();

            return coroutine;
        }

        void Start()
        {
            EditorApplication.update += EditorUpdate;
        }

        public void Stop()
        {
            if (EditorApplication.update != null)
                EditorApplication.update -= EditorUpdate;
        }

        void EditorUpdate()
        {
            // Stop editor coroutine if it does not continue.
            if (coroutine.MoveNext() == false)
                Stop();

            // Process the different types of EditorCoroutines.
            if (coroutine.Current != null)
            {

            }
        }
    }
}