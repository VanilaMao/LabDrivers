using HardDriveTestView.AttachPropities;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace HardDriveTestView.Behaviors
{
    public class TextBoxEnterKeyUpdateBehavior : Behavior<TextBox>, IBehaviorCreator
    {
        public Behavior Create()
        {
            return  new TextBoxEnterKeyUpdateBehavior();
        }

        protected override void OnAttached()
        {
            if (this.AssociatedObject != null)
            {
                base.OnAttached();
                this.AssociatedObject.KeyDown += AssociatedObject_KeyDown;
            }
        }

        protected override void OnDetaching()
        {
            if (this.AssociatedObject != null)
            {
                this.AssociatedObject.KeyDown -= AssociatedObject_KeyDown;
                base.OnDetaching();
            }
        }

        private void AssociatedObject_KeyDown(object sender, KeyEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                if (e.Key == Key.Return)
                {
                    if (e.Key == Key.Enter)
                    {
                        FrameworkElement parent = (FrameworkElement)textBox.Parent;
                        while (parent != null && parent is IInputElement && !((IInputElement)parent).Focusable)
                        {
                            parent = (FrameworkElement)parent.Parent;
                        }

                        DependencyObject scope = FocusManager.GetFocusScope(textBox);
                        FocusManager.SetFocusedElement(scope, parent as IInputElement);
                    }
                }
            }
        }
    }
}
