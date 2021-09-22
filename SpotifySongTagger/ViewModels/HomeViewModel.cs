namespace SpotifySongTagger.ViewModels
{
    public class HomeViewModel : BaseViewModel
    {
        private bool rememberMe = true;
        public bool RememberMe
        {
            get => rememberMe;
            set => SetProperty(ref rememberMe, value, nameof(RememberMe));
        }

        public string FullApplicationName => $"SpotifySongTagger v{VersionStr}";
        public string VersionStr => typeof(HomeViewModel).Assembly.GetName().Version.ToString(3);
    }
}
