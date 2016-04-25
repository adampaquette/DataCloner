using DataCloner.Infrastructure.Modularity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DataCloner.Uwp.Services
{
    public class TreeMenuBuilder
    {
        private HashSet<NavigationMenuItem> _allItems;
        private ObservableCollection<NavigationMenuItem> _itemsTree;

        public TreeMenuBuilder()
        {
            _allItems = new HashSet<NavigationMenuItem>();
            _itemsTree = new ObservableCollection<NavigationMenuItem>();
        }

        public void Append(List<NavigationMenuItem> items)
        {
            foreach (var item in items)
            {
                if (_allItems.Contains(item))
                    throw new Exception($"Menu element '{item.PathId}' is already declared! MenuItem.PathId must be unique!");
                _allItems.Add(item);

                //Is a root element
                if (String.IsNullOrWhiteSpace(item.ContainerPath))
                {
                    _itemsTree.Add(item);
                }
                else
                {
                    //Is a children 
                    _itemsTree.First(i => i.PathId == item.ContainerPath).Children.Add(item);
                }
            }
        }

        public ObservableCollection<NavigationMenuItem> ToObservableCollection()
        {
            return _itemsTree;
        }
    }
}
