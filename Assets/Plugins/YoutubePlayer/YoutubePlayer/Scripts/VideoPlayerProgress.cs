using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;

namespace YoutubePlayer
{
    /// <summary>
    /// A progress bar for VideoPlayer
    /// </summary>
    [RequireComponent(typeof(Image), typeof(RectTransform))]
    public class VideoPlayerProgress : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler
    {
        /// <summary>
        /// Is seeking through the video enabled?
        /// </summary>
        public bool SeekingEnabled;
        
        /// <summary>
        /// The VideoPlayer to synchronize with
        /// </summary>
        public VideoPlayer videoPlayer;


        [SerializeField] private Camera camera;
        
        private Image playbackProgress;
        private RectTransform rectTransform;
        private void Start()
        {
            rectTransform = GetComponent<RectTransform>();
            playbackProgress = GetComponent<Image>();
            
            if (playbackProgress.sprite == null)
            {
                var texture = Texture2D.whiteTexture;
                var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero, 100);
                playbackProgress.sprite = sprite;
            }
        }

        // Update is called once per frame
        private void Update()
        {
            if (videoPlayer.isPlaying)
            {
                playbackProgress.fillAmount =
                    (float) (videoPlayer.length > 0 ? videoPlayer.time / videoPlayer.length : 0);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            Seek(Input.mousePosition);
        }

        public void OnDrag(PointerEventData eventData)
        {
            Seek(Input.mousePosition);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            videoPlayer.Pause();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            videoPlayer.Play();
        }

        private void Seek(Vector2 cursorPosition)
        {
            if(!SeekingEnabled || !videoPlayer.canSetTime)
                return;

            if(!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform, cursorPosition, null, out var localPoint))
                return;
            
            
            
            var x = camera.ScreenToWorldPoint(Input.mousePosition).x;

            x -= camera.transform.position.x;



            //x -= transform.position.x;
            //-4.7 to 4.7
            //9.4 length
            x += rectTransform.rect.width / 2;
            float percent = x / rectTransform.rect.width;

           // timeline.JumpToPercent(percent);
            
           
            
            //var rect = rectTransform.rect;
           // var progress = (localPoint.x - rect.x)  / rect.width;

            
            
            videoPlayer.time = videoPlayer.length * percent;
            playbackProgress.fillAmount = percent - 50;
            
        }
    }
}
