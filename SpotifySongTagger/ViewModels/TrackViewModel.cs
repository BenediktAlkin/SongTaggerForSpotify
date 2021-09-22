using Backend.Entities;

namespace SpotifySongTagger.ViewModels
{
    public class TrackViewModel : BaseViewModel
    {
        private bool isPlaying;
        public bool IsPlaying
        {
            get => isPlaying;
            set => SetProperty(ref isPlaying, value, nameof(IsPlaying));
        }
        private Track track;
        public Track Track
        {
            get => track;
            set => SetProperty(ref track, value, nameof(Track));
        }
        public TrackViewModel(Track track)
        {
            Track = track;
        }
    }
}
