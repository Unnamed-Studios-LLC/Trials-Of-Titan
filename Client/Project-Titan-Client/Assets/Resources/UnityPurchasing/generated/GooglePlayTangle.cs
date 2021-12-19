#if UNITY_ANDROID || UNITY_IPHONE || UNITY_STANDALONE_OSX || UNITY_TVOS
// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("EemZQGvA7nC9QkMCBTJgPmDv09XHHtVl8A/Lo0hB5CdS9afpWepzAGZjn1Aj8AncwFA64RLC3jPz9qNpqOp0wcIzRbl3UNuIuvElgcXIPVcFhoiHtwWGjYUFhoaHI7lHIaRTTf4gHl0DHS3CvRHgKMoYupyz+Q6XfT5hWAUovDHJn8Wc1SBzc+hk/an8vv81B0a5cAQaXkPUO4urFSqx3VWaHjBL78jOf2sLsVLR0a17jHO+AFIstqCgJy2NzCYWL7Fgd7yizhO3BYalt4qBjq0BzwFwioaGhoKHhCjil7HinNKVY+sjDHcbaAPPpNMmXfFMY323KvSzyFbNEWuCf8nNZ+G8BO2V5LMhhSYXLiU05LLMC3ImHScofHMRvhWlRoWEhoeG");
        private static int[] order = new int[] { 9,8,7,13,8,12,11,13,10,13,13,11,13,13,14 };
        private static int key = 135;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
#endif
