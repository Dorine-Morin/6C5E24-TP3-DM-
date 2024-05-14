using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoPlayerCTRL : MonoBehaviour
{
    [SerializeField]
    private Canvas canvasTitre;
    [SerializeField]
    private Canvas canvasVideo;
    [SerializeField]
    private Button btnPlayPause;
    [SerializeField]
    private Slider slider;
    [SerializeField]
    private Button btnStop;
    [SerializeField]
    private Button btnRewind;
    [SerializeField]
    private Sprite imgPlay;
    [SerializeField]
    private Sprite imgPause;
    [SerializeField]
    private VideoPlayer videoPlayer;

    private bool play = false;
    private bool gone = true; //Vrai si on est sortie du Canvas
 //   private bool mouseOverSlider = true; //Vrai si on est sortie du Canvas
    private bool updateEnable = true;

    void Awake()
    {
        //  videoPlayer = GetComponent<VideoPlayer>();
        Pause();
//        mouseOverSlider = true;
    }

    void Start()
    {
        InvokeRepeating("surveillerCmd", 1f, 0.3f);
        InvokeRepeating("sliderUpdate", 1f, 0.5f);
    }

    private void surveillerCmd() {
        if (canvasVideo.enabled == false)
        {
            gone = true;
            Pause();
        }
        else
        {
            if (gone)
                Jouer();
            gone = false;
        }
    }

    private void sliderUpdate()
    {
        if (updateEnable && videoPlayer.frameCount > 0)
        {
            slider.value = (float)videoPlayer.frame / (float)videoPlayer.clip.frameCount;
        }
    }

    public void SlideTo(float fraction)
    {
        slider.value = fraction;
    }

    public void setMouseOverSlider(bool isOverSlider) {
        updateEnable = isOverSlider;
    }

    public void setVideoPosition() {
        updateEnable = false;
        videoPlayer.frame = (long) ((float) slider.value * (float)videoPlayer.clip.frameCount);
        setMouseOverSlider(false);
        updateEnable = true;
    }

    public void OnClick_playPause()
    {
        if (play)
        {
            Pause();
        }
        else
        {
            Jouer();
        }
    }

    public void OnClick_Stop()
    {
        Stop();
    }

    public void OnClick_Rewind()
    {
        Stop();
        Jouer();
    }

    public void OnClick_ExitCanvas()
    {
        canvasVideo.enabled = false;
        canvasTitre.enabled = true;
        Pause();
        gone = true;
    }

    private void Stop()
    {
        play = false;
        videoPlayer.Stop();
        btnPlayPause.GetComponent<Image>().sprite = imgPlay;
    }

    private void Pause()
    {
        if (videoPlayer.isPlaying)
        {
//            Debug.Log("Pause : " + play);
            play = false;
            videoPlayer.Pause();
            btnPlayPause.GetComponent<Image>().sprite = imgPlay;
        }
    }

    void Jouer()
    {
        if (!videoPlayer.isPlaying)
        {
            play = true;
            videoPlayer.Play();
            btnPlayPause.GetComponent<Image>().sprite = imgPause;
        }
    }
}
