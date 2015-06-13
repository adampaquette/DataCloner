using System;
using System.ComponentModel.DataAnnotations;
using GalaSoft.MvvmLight;
using System.Diagnostics;

namespace DataCloner.GUI.ViewModel
{
    internal abstract class AnnotationViewModelBase : ViewModelBase
    {
        protected virtual void ValidateProperty(string property, object value)
        {
            var ctx = new ValidationContext(this, null, null);
            ctx.MemberName = property;
            Validator.ValidateProperty(value, ctx);
        }

        new protected bool Set<T>(string propertyName, ref T field, T newValue = default(T), bool broadcast = false)
        {
            ValidateProperty(propertyName, newValue);
            return base.Set(propertyName, ref field, newValue, broadcast);
        }
    }
}
