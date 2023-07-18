using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using PropertyChanged;
using System.Collections.ObjectModel;

namespace SudokuGame.ViewModels
{


    [AddINotifyPropertyChangedInterface]
    public class CustomMaterialDesignPopupViewModel
    {
        public string PopupTitle { get; set; }

        public double WidthGrid { get; set; }

        public double HeightGrid { get; set; }


        public bool IsValueValid { get; set; }
        public ObservableCollection<FrameworkElement> Controls { get; set; }

        public CustomMaterialDesignPopupViewModel()
        {
            Controls = new ObservableCollection<FrameworkElement>();
        }
    }

}
