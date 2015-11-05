﻿using DataCloner.GUI.UserControls;
using System.Windows.Media;

namespace DataCloner.GUI.ViewModel.SolutionExplorer
{
    public class DatabaseTreeViewModel : TreeViewLazyItemViewModel
    {
        private static readonly ImageSource _image = (ImageSource)new ImageSourceConverter().ConvertFromString("pack://application:,,,/Resources/Images/database.png");

        public DatabaseTreeViewModel()
            : base(null, false)
        {
            Image = _image;
        }
    }
}