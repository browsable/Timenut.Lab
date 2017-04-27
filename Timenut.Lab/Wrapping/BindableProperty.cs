using System;
using System.Windows;

namespace Timenut.Lab.Wrapping
{
    public delegate void PropertyChangedCallback2(DependencyObject d, object oldValue, object newValue);

    public static class BindableProperty
    {
        public static DependencyProperty Create(string propertyName, Type propertyType, Type ownerType,
            object defaultValue = null,
            PropertyChangedCallback2 propertyChanged = null)
        {
            PropertyChangedCallback callback = null;

            if (propertyChanged != null)
            {
                callback = new PropertyChangedCallback(
                    (o, e) =>
                    {
                        propertyChanged?.Invoke(o, e.OldValue, e.NewValue);
                    });
            }

            return DependencyProperty.Register(
                propertyName, propertyType, ownerType,
                new PropertyMetadata(defaultValue, callback));
        }
    }
}
