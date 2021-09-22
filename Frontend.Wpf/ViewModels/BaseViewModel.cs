using Backend;
using Frontend.Wpf.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Util;

namespace Frontend.Wpf.ViewModels
{
    public abstract class BaseViewModel : NotifyPropertyChangedBase
    {
        public DataContainer DataContainer => DataContainer.Instance;
        public Settings Settings => Settings.Instance;
        public PlayerManager PlayerManager => PlayerManager.Instance;

    }
}
