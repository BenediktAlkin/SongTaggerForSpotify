using Backend;
using SpotifySongTagger.Utils;
using Util;

namespace SpotifySongTagger.ViewModels
{
    public abstract class BaseViewModel : NotifyPropertyChangedBase
    {
        public static ConnectionManager ConnectionManager => ConnectionManager.Instance;
        public static DataContainer DataContainer => DataContainer.Instance;
        public static Settings Settings => Settings.Instance;
        public static PlayerManager PlayerManager => PlayerManager.Instance;

    }
}
