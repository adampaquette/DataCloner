using DataCloner.Infrastructure.Modularity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DataCloner.Uwp.Services
{
    public class TreeMenuBuilder
    {
        private HashSet<MenuItem> _allItems;
        private ObservableCollection<MenuItem> _itemsTree;

        public TreeMenuBuilder()
        {
            _allItems = new HashSet<MenuItem>();
            _itemsTree = new ObservableCollection<MenuItem>();
        }

        public void Append(List<MenuItem> items)
        {
            foreach (var item in items)
            {
                if (_allItems.Contains(item))
                    throw new Exception("Menu element is already declared! MenuItem.PathId must be unique!");
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

        public ObservableCollection<MenuItem> ToObservableCollection()
        {
            return _itemsTree;
        }
    }
}
