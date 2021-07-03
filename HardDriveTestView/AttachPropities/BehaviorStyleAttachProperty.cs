using System.Collections.ObjectModel;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace HardDriveTestView.AttachPropities
{
    public static class BehaviorStyleAttachProperty
    {
        public static readonly DependencyProperty BehaviorsProperty =
       DependencyProperty.RegisterAttached(
           "Behaviors",
           typeof(BehaviorCreatorCollection),
           typeof(BehaviorStyleAttachProperty),
           new UIPropertyMetadata(null, OnBehaviorsChanged));


        public static BehaviorCreatorCollection GetBehaviors(TreeView treeView)
        {
            return (BehaviorCreatorCollection)treeView.GetValue(BehaviorsProperty);
        }

        public static void SetBehaviors(
            TreeView treeView, BehaviorCreatorCollection value)
        {
            treeView.SetValue(BehaviorsProperty, value);
        }


        private static void OnBehaviorsChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is BehaviorCreatorCollection == false)
                return;

            BehaviorCreatorCollection newBehaviorCollection = e.NewValue as BehaviorCreatorCollection;

            BehaviorCollection behaviorCollection = Interaction.GetBehaviors(depObj);
            behaviorCollection.Clear();
            foreach (IBehaviorCreator behavior in newBehaviorCollection)
            {
                behaviorCollection.Add(behavior.Create());
            }
        }
    }
}