using System.ComponentModel.DataAnnotations;
using GalaSoft.MvvmLight;
using DataCloner.GUI.Message;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Runtime.CompilerServices;

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

        protected bool ValidateAndSet<T>(ref T field, T newValue = default(T), [CallerMemberName]string propertyName = null, bool broadcast = false)
        {
            ValidateProperty(propertyName, newValue);
            return Set(propertyName, ref field, newValue, broadcast);
        }

        protected bool Set<T>(ref T field, T newValue = default(T), [CallerMemberName]string propertyName = null, bool broadcast = false)
        {
            return Set(propertyName, ref field, newValue, broadcast);
        }
    }
}
