using Backend;
using SpotifySongTagger.Utils;
using Util;

namespace SpotifySongTagger.ViewModels
{
    public abstract class BaseViewModel : NotifyPropertyChangedBase
    {
        public DataContainer DataContainer => DataContainer.Instance;
        public Settings Settings => Settings.Instance;
        public PlayerManager PlayerManager => PlayerManager.Instance;

    }
}
