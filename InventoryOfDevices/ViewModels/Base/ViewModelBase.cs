using InventoryOfDevices.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace InventoryOfDevices.ViewModels
{
    public abstract class ViewModelBase: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        //[CallerMemberName] - атрибут позволяет компилятору автоматически предоставлять имя вызывающего члена(то есть свойства, которое вызвало уведомление о изменении) в качестве значения по умолчанию для этого параметра, не требуя явного указания вызывающим кодом.
        protected virtual void OnPropertyChanged ([CallerMemberName]string PropertyName=null)
        {
            PropertyChanged?.Invoke ( this, new PropertyChangedEventArgs (PropertyName));
        }
        //ref T field: ссылочный параметр типа T, который представляет поле в классе, которое будет изменяться
        protected virtual bool Set <T>(ref T field, T value, [CallerMemberName] string PropertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(PropertyName);
            return true;
        }

    }
}
