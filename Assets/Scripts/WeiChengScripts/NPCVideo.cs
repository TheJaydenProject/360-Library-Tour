using UnityEngine;
using UnityEngine.Video;

public class NPCVideo : MonoBehaviour
{
    VideoPlayer videoPlayer;

    void Start()
    {
        videoPlayer = GetComponent<VideoPlayer>();
    }
    
    public void SelectDialogue(VideoClip clip)
    {
        videoPlayer.clip = clip;
        videoPlayer.Play();
    }
}
