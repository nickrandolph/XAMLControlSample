using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace MyCustomControls
{
    public partial class MultiSwitchControl : Control
    {
        public double PressedOpacity
        {
            get => (double)GetValue(PressedOpacityProperty);
            set => SetValue(PressedOpacityProperty, value);
        }

        public static readonly DependencyProperty PressedOpacityProperty =
            DependencyProperty.Register(nameof(PressedOpacity), typeof(double), typeof(MultiSwitchControl), new PropertyMetadata(0));

        public MultiSwitchControl()
        {
            this.DefaultStyleKey = typeof(MultiSwitchControl);
        }

        private IDictionary<UIElement, (string name, bool isInside, bool isPressed)> Parts { get; } = new Dictionary<UIElement, (string name, bool isInside, bool isPressed)>();

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            SetupPart("PART_Off");
            SetupPart("PART_Up");
            SetupPart("PART_Right");
            SetupPart("PART_Down");
            SetupPart("PART_Left");
        }

        private void SetupPart(string partName)
        {
            var partOff = GetTemplateChild(partName) as UIElement;
            if (partOff == null) throw new NullReferenceException($"{partName} expected in control template");
            Parts[partOff] = (name: partName.Replace("PART_", ""), isInside: false, isPressed: false);
            partOff.PointerPressed += PartPointerPressed;
            partOff.PointerReleased += PartPointerReleased;
            partOff.PointerEntered += PartPointerEntered;
            partOff.PointerExited += PartPointerExited;
        }

        private void PartPointerExited(object sender, PointerRoutedEventArgs e)
        {
            var part = Parts[sender as UIElement];
            Parts[sender as UIElement] = (part.name, false, part.isPressed);
            if (!part.isPressed)
            {
                VisualStateManager.GoToState(this, "Normal", true);
            }
        }

        private void PartPointerEntered(object sender, PointerRoutedEventArgs e)
        {
            var part = Parts[sender as UIElement];
            Debug.WriteLine("Entered " + part);
            Parts[sender as UIElement] = (part.name, true, part.isPressed);
            if (!part.isPressed)
            {
                VisualStateManager.GoToState(this, "PointerOver" + part.name, true);
            }
        }

        private void PartPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            Debug.WriteLine(DateTime.Now.Ticks);
        }

        private void PartPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var part = Parts[sender as UIElement];
            Parts[sender as UIElement] = (part.name, part.isInside, true);
            VisualStateManager.GoToState(this, "Pressed" + part.name, true);
            (sender as UIElement).CapturePointer(e.Pointer);
        }

        private void PartPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            var partElement = (sender as UIElement);
            partElement.ReleasePointerCaptures();
            var part = Parts[sender as UIElement];
            Parts[sender as UIElement] = (part.name, part.isInside, false);
            if (!part.isInside)
            {
                VisualStateManager.GoToState(this, "Normal", true);
            }
            else
            {
                VisualStateManager.GoToState(this, "Selection" + part.name, true);
                VisualStateManager.GoToState(this, "PointerOver" + part.name, true);
            }
        }
    }
}