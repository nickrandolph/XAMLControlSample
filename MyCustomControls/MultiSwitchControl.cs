using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace MyCustomControls
{
    [TemplatePart(Name = "PART_Off")]
    [TemplatePart(Name = "PART_Up")]
    [TemplatePart(Name = "PART_Right")]
    [TemplatePart(Name = "PART_Down")]
    [TemplatePart(Name = "PART_Left")]
    public partial class MultiSwitchControl : Control
    {
        public static readonly DependencyProperty PressedOpacityProperty =
            DependencyProperty.Register(nameof(PressedOpacity), typeof(double), typeof(MultiSwitchControl), new PropertyMetadata(0));

        public static readonly DependencyProperty ValueProperty =
DependencyProperty.Register(nameof(Value), typeof(SwitchState), typeof(MultiSwitchControl), new PropertyMetadata(SwitchState.None, ValuePropertyChanged));

        private const string NormalState = "Normal";
        private const string PartPrefix = "PART_";
        private const string PointerOverStatePrefix = "PointerOver";
        private const string PressedStatePrefix = "Pressed";
        private const string SelectionStatePrefix = "Selection";

        public MultiSwitchControl()
        {
            this.DefaultStyleKey = typeof(MultiSwitchControl);
        }

        public enum SwitchState
        {
            None,
            Off,
            Up,
            Right,
            Down,
            Left
        }

        public double PressedOpacity
        {
            get => (double)GetValue(PressedOpacityProperty);
            set => SetValue(PressedOpacityProperty, value);
        }

        public SwitchState Value
        {
            get => (SwitchState)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        private IDictionary<UIElement, (SwitchState state, bool isInside, bool isPressed)> Parts { get; } = new Dictionary<UIElement, (SwitchState state, bool isInside, bool isPressed)>();

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var switchStates = new[] { SwitchState.Off, SwitchState.Up, SwitchState.Right, SwitchState.Down, SwitchState.Left };
            foreach (var s in switchStates)
            {
                SetupPart(s);
            }
        }

        private static void ValuePropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            var switchControl = dependencyObject as MultiSwitchControl;
            switchControl?.UpdateSwitchState();
        }

        private void PartPointerEntered(object sender, PointerRoutedEventArgs e)
        {
            var partElement = sender as UIElement;
            if (partElement == null)
            {
                return;
            }

            var part = Parts[partElement];
            Parts[partElement] = (part.state, true, part.isPressed);
            if (!part.isPressed)
            {
                VisualStateManager.GoToState(this, PointerOverStatePrefix + part.state, true);
            }
        }

        private void PartPointerExited(object sender, PointerRoutedEventArgs e)
        {
            var partElement = sender as UIElement;
            if (partElement == null)
            {
                return;
            }

            var part = Parts[partElement];
            Parts[partElement] = (part.state, false, part.isPressed);
            if (!part.isPressed)
            {
                VisualStateManager.GoToState(this, NormalState, true);
            }
        }

        private void PartPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var partElement = sender as UIElement;
            if (partElement == null)
            {
                return;
            }

            var part = Parts[partElement];
            if (!part.isInside && !part.isPressed)
            {
                // Hack to deal with Android not firing events correctly
                //VisualStateManager.GoToState(this, "Selection" + part.state, true);
                Value = part.state;
                VisualStateManager.GoToState(this, NormalState, true);
                return;
            }
            Parts[partElement] = (part.state, part.isInside, true);
            VisualStateManager.GoToState(this, PressedStatePrefix + part.state, true);
            partElement.CapturePointer(e.Pointer);
        }

        private void PartPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            var partElement = sender as UIElement;
            if (partElement == null)
            {
                return;
            }

            partElement.ReleasePointerCaptures();
            var part = Parts[partElement];
            Parts[partElement] = (part.state, part.isInside, false);
            if (part.isInside)
            {
                Value = part.state;
            }
            VisualStateManager.GoToState(this, NormalState, true);
        }

        private void SetupPart(SwitchState state)
        {
            var partName = PartPrefix + state;
            var partOff = GetTemplateChild(partName) as UIElement;
            if (partOff == null) throw new NullReferenceException($"{partName} expected in control template");
            Parts[partOff] = (state: state, isInside: false, isPressed: false);
            partOff.PointerPressed += PartPointerPressed;
            partOff.PointerReleased += PartPointerReleased;
            partOff.PointerEntered += PartPointerEntered;
            partOff.PointerExited += PartPointerExited;
        }

        private void UpdateSwitchState()
        {
            VisualStateManager.GoToState(this, SelectionStatePrefix + this.Value, true);
        }
    }
}