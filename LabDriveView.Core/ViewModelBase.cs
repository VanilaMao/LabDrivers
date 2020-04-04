using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace LabDriveView.Core
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void SetProperty<T>(ref T value, T newValue, [CallerMemberName]string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(value, newValue))
            {
                return;
            }
            value = newValue;
            OnPropertyChanged(propertyName);
        }

        protected virtual void SetProperty<T, TModel>(Expression<Func<TModel, T>> expression, TModel model, T newValue, [CallerMemberName] string propertyName = "")
        {
            model.SetProperty(expression, newValue);
            OnPropertyChanged(propertyName);
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
